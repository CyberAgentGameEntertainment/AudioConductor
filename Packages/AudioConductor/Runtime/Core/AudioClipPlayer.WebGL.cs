// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL
using System;
using UnityEngine;

namespace AudioConductor.Core
{
    internal sealed partial class AudioClipPlayer
    {
        private bool _isSystemPaused;

        private bool _resumeFromLoopStart;

        // Sample to resume from when _resumeFromLoopStart is set. Captured in PauseBySystem so an intro
        // region (start..loopStart) interrupted by a system pause is not truncated on resume.
        private int _resumeSample;
        private bool _armedWhileContextSuspended;
        private int _lastScheduledSourceIndex;
        private double _lastScheduledPlayStartTime;

        private bool _webGLNativeLoopActive;

        // Always true for DecompressOnLoad; for CompressedInMemory/Streaming it depends on the
        // backend (buffer vs MediaElement) and is resolved by SetupNativeLoopMode /
        // ResolvePendingBackend. MediaElement-backed clips use the crossover instead.
        private bool _nativeLoopSupported;

#if !UNITY_EDITOR
        // Stable per-instance id that namespaces this player's WebGL scheduling state on the JS side.
        // Without it, concurrently-playing AudioClipPlayers share the global slot registers and
        // cross-talk: a second player's play overwrites the first player's slot/native-loop bindings.
        // Pooled players keep their id across Rent/Return, matching the JS channel reuse per source.
        private readonly int _webglPlayerId = NextWebGLPlayerId();

        private static int _nextWebGLPlayerId;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetWebGLPlayerIdCounter() => _nextWebGLPlayerId = 0;

        private static int NextWebGLPlayerId() => _nextWebGLPlayerId++;

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AudioConductor_IsAudioContextRunning();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AudioConductor_HasFakemodTime();

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_SetPendingNativeLoop(int playerId, float loopStartSecs, float loopEndSecs);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_ClearNativeLoop(int playerId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_PrepareScheduledPlaySlot(int playerId, int slot, float startOffsetSecs);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_CancelPendingBind(int playerId, int slot);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_ClearScheduledEndTimes(int playerId);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_SetScheduledEndTime(int playerId, int slot, double endTime);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AudioConductor_SlotBackend(int playerId, int slot);

        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void AudioConductor_ApplyNativeLoopToSlot(int playerId, int slot, float loopStartSecs, float loopEndSecs);
#endif

        // AudioConductor's WebGL loop scheduling is expressed in the FakeMod dsp clock. The backend
        // only exposes it on some versions (see AudioConductor_HasFakemodTime); where it is missing
        // we must use the stock AudioSource.SetScheduledEndTime path instead. The build version does
        // not determine this (e.g. 6000.0.38 lacks it, 6000.0.40 has it), so detect it at runtime
        // and cache it. Null means "query the platform"; tests may override it.
        internal static bool? UsesAudioConductorSchedulingOverride;
#if !UNITY_EDITOR
        private static int _hasFakemodTimeCache = -1;
#endif

        private static bool UsesAudioConductorScheduling
        {
            get
            {
                if (UsesAudioConductorSchedulingOverride is not null)
                    return UsesAudioConductorSchedulingOverride.Value;
#if UNITY_EDITOR
                // The Editor has no browser audio backend; use the stock path.
                return false;
#else
                if (_hasFakemodTimeCache < 0)
                    _hasFakemodTimeCache = AudioConductor_HasFakemodTime();
                return _hasFakemodTimeCache == 1;
#endif
            }
        }

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

        // Test seam: the AudioContext state comes from the browser and cannot be driven
        // from EditMode tests. Null means "query the platform".
        internal Func<bool>? IsAudioContextRunningOverride;

        private bool IsAudioContextRunning()
        {
            if (IsAudioContextRunningOverride is not null)
                return IsAudioContextRunningOverride();
#if UNITY_EDITOR
            // The Editor has no browser AudioContext.
            return true;
#else
            return AudioConductor_IsAudioContextRunning() == 1;
#endif
        }

        // Must be called before PlayScheduled() when UsesAudioConductorScheduling is true.
        // JS_Sound_SetScheduledEndTime is absent from Unity 2022.3 WebGL WASM (the WASM import
        // table never references it, so a jslib override is never invoked). AudioConductor
        // uses AudioConductor_SetScheduledEndTime instead, which calls channel.stop(delay).
        private void PrepareNativeLoopBeforePlay(int startSample)
        {
#if !UNITY_EDITOR
            AudioConductor_PrepareScheduledPlaySlot(_webglPlayerId, _nextPlayAudioSourceIndex, (float)startSample / _frequency);
            if (!_isLoop || !_nativeLoopSupported)
                return;
            var loopStartSecs = (float)_loopStartSample / _frequency;
            var loopEndSecs = (float)_endSample / _frequency;
            AudioConductor_SetPendingNativeLoop(_webglPlayerId, loopStartSecs, loopEndSecs);
#endif
        }

        // Resets this player's JS-side scheduling state on stop/reset. Cancels not-yet-bound
        // channel-creation requests so a bind pushed by PrepareScheduledPlaySlot that will never be
        // consumed (the play was stopped before its channel was created) does not leak in
        // acPendingBinds and later mis-bind an unrelated channel, and clears both slots' scheduled
        // end times so a pooled player (stable _webglPlayerId across Rent/Return) does not carry a
        // prior play's stale future end into its next play. No-op when the AudioConductor scheduling
        // path is not in use.
        private void CancelPendingBinds()
        {
#if !UNITY_EDITOR
            if (!UsesAudioConductorScheduling)
                return;
            AudioConductor_CancelPendingBind(_webglPlayerId, 0);
            AudioConductor_CancelPendingBind(_webglPlayerId, 1);
            AudioConductor_ClearScheduledEndTimes(_webglPlayerId);
#endif
        }

        private void ArmScheduledEnd(int sourceIndex, double endTime)
        {
            _webGLNativeLoopActive = false;

#if !UNITY_EDITOR
            if (_isLoop && _nativeLoopSupported)
            {
                // Prevent ScheduleNextLoop from firing; AudioBufferSourceNode.loop handles itself.
                _scheduledEndTime = double.MaxValue;
                _webGLNativeLoopActive = true;
                _lastScheduledSourceIndex = sourceIndex;
                _armedWhileContextSuspended = !IsAudioContextRunning();
                return;
            }

            // Non-loop path: clear stale native loop tracking so JS_Sound_SetLoop(ch, 0)
            // is not suppressed for subsequent non-loop plays on the same channel.
            AudioConductor_ClearNativeLoop(_webglPlayerId);
            AudioConductor_SetScheduledEndTime(_webglPlayerId, sourceIndex, endTime);
#else
            _sources[sourceIndex].SetScheduledEndTime(endTime);
#endif
            _lastScheduledSourceIndex = sourceIndex;
            _armedWhileContextSuspended = !IsAudioContextRunning();
        }

        private void TryRearmSchedule()
        {
            if (!_armedWhileContextSuspended || !IsAudioContextRunning())
                return;

            _armedWhileContextSuspended = false;

            // Native loop manages itself; re-arming would schedule a spurious stop.
            if (_webGLNativeLoopActive)
                return;

            var source = GetPlayingSource();
            if (source == null)
            {
                // Distinguish PlayScheduleDelay window (source queued but not yet audible)
                // from catch-up overshot (source already stopped) using scheduled start time,
                // because TimeSamples is unreliable (may be 0 or stale) after a source stops.
                if (_dspClock.DspTime < _lastScheduledPlayStartTime)
#if !UNITY_EDITOR
                    AudioConductor_SetScheduledEndTime(_webglPlayerId, _lastScheduledSourceIndex, _scheduledEndTime);
#else
                    _sources[_lastScheduledSourceIndex].SetScheduledEndTime(_scheduledEndTime);
#endif
                // else: catch-up overshot the clip end; ManualUpdate reschedules below.
                return;
            }

            // The engine fast-forwards a queued sound by the suspended duration
            // (catch-up), so TimeSamples already reflects the real content position.
            var pastEnd = GetActualPitch() >= 0f
                ? source.TimeSamples >= _endSample
                : source.TimeSamples <= _endSample;
            if (pastEnd)
            {
                // Past the end region: force the boundary so this very update schedules
                // the next loop (or fires the end of a one-shot).
                _scheduledEndTime = _dspClock.DspTime;
                var pastEndSlot = ReferenceEquals(source, _sources[0]) ? 0 : 1;
#if !UNITY_EDITOR
                AudioConductor_SetScheduledEndTime(_webglPlayerId, pastEndSlot, _scheduledEndTime);
#else
                source.SetScheduledEndTime(_scheduledEndTime);
#endif
                UpdateNextEventTime();
                return;
            }

            RecalculateScheduledEndTime();
        }

        // The browser suspends rAF callbacks when the tab is hidden (MDN: Page Visibility API),
        // and Unity's game loop runs on rAF by default (Unity Manual: WebGL performance).
        // Therefore Stop()/Pause() cannot be called while system-paused.
        internal void PauseBySystem()
        {
            if (_isSystemPaused || IsPaused || !_isPlaybackActive)
                return;

            _isSystemPaused = true;
            _pauseStartTime = _dspClock.DspTime;

            if (_isLoop && _backendDetectPending)
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

            if (_isLoop && !_nativeLoopSupported)
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

            if (_isLoop)
            {
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

                return;
            }

            if (_sources[0].IsPlaying)
            {
                _sources[0].Pause();
            }
            else
            {
                _sources[0].Stop();
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
            return source != null && source.TimeSamples < _loopStartSample
                ? source.TimeSamples
                : _loopStartSample;
        }

        internal void ResumeBySystem()
        {
            if (!_isSystemPaused)
                return;

            if (_resumeFromLoopStart)
            {
                _resumeFromLoopStart = false;
                _isSystemPaused = false;
                if (_isPlaybackActive && !IsPaused)
                {
                    _nextPlayAudioSourceIndex = 0;
                    SchedulePlayback(_dspClock.DspTime + PlayScheduleDelay, _resumeSample);
                }

                return;
            }

            if (_wasStoppedBeforePlay)
            {
                _isSystemPaused = false;
                if (_isPlaybackActive && !IsPaused)
                {
                    _wasStoppedBeforePlay = false;
                    _nextPlayAudioSourceIndex = 0;
                    SchedulePlayback(_dspClock.DspTime + PlayScheduleDelay, _startSample);
                }
                else if (!_isPlaybackActive)
                {
                    _wasStoppedBeforePlay = false;
                }

                return;
            }

            var pausedDuration = _dspClock.DspTime - _pauseStartTime;
            ShiftScheduleByPauseDuration(pausedDuration);
            _isSystemPaused = false;

            if (_isLoop)
            {
                _sources[_pausedIndex].UnPause();
                return;
            }

            _sources[0].UnPause();
        }
    }
}

#endif
