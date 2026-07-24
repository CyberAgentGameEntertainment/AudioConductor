// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Core
{
    internal sealed partial class AudioClipPlayer
    {
        private void PauseLoop()
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
                // behavior per Unity docs, so stop both and reschedule fresh on Resume.
                _sources[0].Stop();
                _sources[1].Stop();
#if UNITY_WEBGL
                CancelPendingBinds();
#endif
                _wasStoppedBeforePlay = true;
            }
        }

        private int GetCurrentSampleLoop()
        {
            var source = GetPlayingSource();
            return source == null ? 0 : source.TimeSamples;
        }

        private bool SetCurrentSampleLoop(int sample)
        {
            var source = GetPlayingSource();
            if (source == null)
                return false;
            source.TimeSamples = sample;
            return true;
        }

        private void ScheduleNextLoop()
        {
            SchedulePlayback(_scheduledEndTime, _loopStartSample);
        }
    }
}
