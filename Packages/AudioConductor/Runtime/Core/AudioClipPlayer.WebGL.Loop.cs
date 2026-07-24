// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL
using UnityEngine;

namespace AudioConductor.Core
{
    internal sealed partial class AudioClipPlayer
    {
        private bool _resumeFromLoopStart;

        // Sample to resume from when _resumeFromLoopStart is set. Captured in PauseBySystem so an intro
        // region (start..loopStart) interrupted by a system pause is not truncated on resume.
        private int _resumeSample;

        private bool _webGLNativeLoopActive;

        // Always true for DecompressOnLoad; for CompressedInMemory/Streaming it depends on the
        // backend (buffer vs MediaElement) and is resolved by SetupNativeLoopMode /
        // ResolvePendingBackend. MediaElement-backed clips use the crossover instead.
        private bool _nativeLoopSupported;

        // True while a loop clip whose backend is still unknown is waiting for the first source to be
        // created so its backend can be resolved (see ResolvePendingBackend).
        private bool _backendDetectPending;

        // Decides the loop strategy for a clip at Setup. DecompressOnLoad is always a buffer (native
        // loop). For CompressedInMemory/Streaming the backend (buffer vs MediaElement) is not fixed by
        // loadType and is only knowable once a source exists, so it is resolved per clip after the
        // first play (see ResolvePendingBackend); start without a loop strategy and mark it pending.
        // Test seam: AudioClip.Create yields DecompressOnLoad in EditMode, so the
        // backend-detect-pending path (CompressedInMemory/Streaming) cannot be reached
        // through the clip. Null means "use the clip's loadType".
        internal AudioClipLoadType? LoadTypeOverride;

        private void SetupNativeLoopMode(AudioClipLoadType loadType)
        {
            loadType = LoadTypeOverride ?? loadType;

            if (loadType == AudioClipLoadType.DecompressOnLoad)
            {
                _backendDetectPending = false;
                _nativeLoopSupported = true;
                return;
            }

            _nativeLoopSupported = false;
            _backendDetectPending = _isLoop;
        }

        // Returns true once the backend is resolved; false while the source is not yet created.
        private bool ResolvePendingBackend()
        {
#if !UNITY_EDITOR
            var slot = _lastScheduledSourceIndex;
            var backend = AudioConductor_SlotBackend(_webglPlayerId, slot);
            if (backend < 0)
                return false; // source not created yet; retry next update

            _backendDetectPending = false;

            if (backend == 1)
            {
                // Buffer source: loop the partial region natively, no crossover re-scheduling.
                _nativeLoopSupported = true;
                _webGLNativeLoopActive = true;
                _scheduledEndTime = double.MaxValue;
                AudioConductor_ApplyNativeLoopToSlot(_webglPlayerId, slot, (float)_loopStartSample / _frequency,
                    (float)_endSample / _frequency);
            }
            else
            {
                // MediaElement source: arm the crossover now (deferred from the first play).
                _nativeLoopSupported = false;
                AudioConductor_SetScheduledEndTime(_webglPlayerId, slot, _scheduledEndTime);
                UpdateNextEventTime();
            }

            return true;
#else
            _backendDetectPending = false;
            return true;
#endif
        }

#if !UNITY_EDITOR
        // Returns true if the loop was armed natively (AudioBufferSourceNode.loop), meaning the
        // caller must skip its own (non-loop) scheduled-end arming for this call.
        private bool TryArmNativeLoop(int sourceIndex)
        {
            if (!_isLoop || !_nativeLoopSupported)
                return false;

            // Prevent ScheduleNextLoop from firing; AudioBufferSourceNode.loop handles itself.
            _scheduledEndTime = double.MaxValue;
            _webGLNativeLoopActive = true;
            _lastScheduledSourceIndex = sourceIndex;
            _armedWhileContextSuspended = !IsAudioContextRunning();
            return true;
        }
#endif

        private void PauseBySystemLoop()
        {
            if (_backendDetectPending)
            {
                // Backend not yet resolved: the source may turn out to be a buffer (native loop).
                // Pausing would risk the MediaElement catch-up glitch if it resolves to MediaElement,
                // so stop both and restart cleanly. Restart from the start sample (not loopStart) so a
                // cue with an intro region is not truncated; ResolvePendingBackend re-runs and commits
                // the loop strategy on resume.
                _sources[0].Stop();
                _sources[1].Stop();
                CancelPendingBinds();
                _wasStoppedBeforePlay = true;
                return;
            }

            if (!_nativeLoopSupported)
            {
                // MediaElement crossover loop: do not Pause()/UnPause(). Unity's AudioContext-resume
                // catch-up fast-forwards a paused MediaElement by the suspended duration, overshooting
                // past the partial loop region (which a MediaElement cannot wrap natively), so it would
                // play out-of-region audio until the scheduled stop fires. Stop both and reschedule the
                // loop cleanly on resume instead.
                //
                // Capture the live position before stopping: a pause inside the intro region
                // (start..loopStart) must resume from there so the remaining intro is not truncated,
                // mirroring the _backendDetectPending branch. A pause inside the loop body resolves to
                // loopStart, the loop's own restart point (the crossfade may overlap two sources there,
                // making a single live position ambiguous).
                _resumeSample = ResolveSystemResumeSample();
                _sources[0].Stop();
                _sources[1].Stop();
                CancelPendingBinds();
                _resumeFromLoopStart = true;
                return;
            }

            if (_sources[0].IsPlaying)
            {
                _sources[0].Pause();
                _pausedIndex = 0;
                _sources[1].Stop();
            }
            else if (_sources[1].IsPlaying)
            {
                _sources[0].Stop();
                _sources[1].Pause();
                _pausedIndex = 1;
            }
            else
            {
                // Neither source is playing yet (within PlayScheduleDelay window).
                // AudioSource.Pause() on a scheduled-but-not-yet-playing source has undefined
                // behavior per Unity docs, so stop both and reschedule fresh on ResumeBySystem.
                _sources[0].Stop();
                _sources[1].Stop();
                CancelPendingBinds();
                _wasStoppedBeforePlay = true;
            }
        }

        // Resolves the sample a system-paused MediaElement crossover loop resumes from: the live
        // content position while still inside the intro region (start..loopStart) so its remainder is
        // not truncated, otherwise loopStart (the loop body's restart point).
        private int ResolveSystemResumeSample()
        {
            var source = GetPlayingSource();
            if (source == null) return _startSample;
            return source.TimeSamples < _loopStartSample ? source.TimeSamples : _loopStartSample;
        }

        private void ResumeBySystemFromLoopStart()
        {
            _resumeFromLoopStart = false;
            _isSystemPaused = false;
            if (_isPlaybackActive && !IsPaused)
            {
                _nextPlayAudioSourceIndex = 0;
                SchedulePlayback(_dspClock.DspTime + PlayStartDelay, _resumeSample);
            }
        }
    }
}

#endif
