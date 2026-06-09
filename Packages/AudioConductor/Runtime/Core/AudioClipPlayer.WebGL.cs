// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL
namespace AudioConductor.Core
{
    internal sealed partial class AudioClipPlayer
    {
        private bool _isSystemPaused;

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
