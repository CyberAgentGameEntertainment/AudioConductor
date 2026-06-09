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

                return;
            }

            _sources[0].Pause();
        }

        internal void ResumeBySystem()
        {
            if (!_isSystemPaused)
                return;

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
