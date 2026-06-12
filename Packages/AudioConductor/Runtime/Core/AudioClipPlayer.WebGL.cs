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
        private double _lastScheduledPlayStartTime;

#if !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern int AudioConductor_IsAudioContextRunning();
#endif

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

        // Wraps SetScheduledEndTime to track the last armed source and record whether
        // the stop was issued while the AudioContext was suspended (in which case the
        // engine's JS layer silently discards it and TryRearmSchedule must re-issue it).
        private void ArmScheduledEnd(int sourceIndex, double endTime)
        {
            _sources[sourceIndex].SetScheduledEndTime(endTime);
            _lastScheduledSourceIndex = sourceIndex;
            _armedWhileContextSuspended = !IsAudioContextRunning();
        }

        private void TryRearmSchedule()
        {
            if (!_armedWhileContextSuspended || !IsAudioContextRunning())
                return;

            _armedWhileContextSuspended = false;

            var source = GetPlayingSource();
            if (source == null)
            {
                // Distinguish PlayScheduleDelay window (source queued but not yet audible)
                // from catch-up overshot (source already stopped) using scheduled start time,
                // because TimeSamples is unreliable (may be 0 or stale) after a source stops.
                if (_dspClock.DspTime < _lastScheduledPlayStartTime)
                    _sources[_lastScheduledSourceIndex].SetScheduledEndTime(_scheduledEndTime);
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
                source.SetScheduledEndTime(_scheduledEndTime);
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
                _wasStoppedBeforePlay = true;
            }
        }

        internal void ResumeBySystem()
        {
            if (!_isSystemPaused)
                return;

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
