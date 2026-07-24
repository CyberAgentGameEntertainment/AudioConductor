// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL
using System;

namespace AudioConductor.Core
{
    internal sealed partial class AudioClipPlayer
    {
        private bool _isSystemPaused;
        private bool _armedWhileContextSuspended;
        private int _lastScheduledSourceIndex;

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
            if (TryArmNativeLoop(sourceIndex))
                return;

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
                // Distinguish the pre-audible window (source queued but not yet audible) from
                // catch-up overshot (source already stopped past the clip end) by comparing
                // against the scheduled end time: this holds regardless of PlayStartDelay (0 or
                // PlayScheduleDelay) and loop/non-loop, unlike a start-time comparison which goes
                // stale once the schedule's start time has already elapsed either way.
                // TimeSamples itself is unreliable (may be 0 or stale) after a source stops.
                if (_dspClock.DspTime < _scheduledEndTime)
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

            if (_isLoop)
            {
                PauseBySystemLoop();
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

        internal void ResumeBySystem()
        {
            if (!_isSystemPaused)
                return;

            if (_resumeFromLoopStart)
            {
                ResumeBySystemFromLoopStart();
                return;
            }

            if (_wasStoppedBeforePlay)
            {
                _isSystemPaused = false;
                if (_isPlaybackActive && !IsPaused)
                {
                    _wasStoppedBeforePlay = false;
                    _nextPlayAudioSourceIndex = 0;
                    SchedulePlayback(_dspClock.DspTime + PlayStartDelay, _startSample);
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
