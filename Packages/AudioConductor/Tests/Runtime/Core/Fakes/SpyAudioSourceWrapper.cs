// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class SpyAudioSourceWrapper : StubAudioSourceWrapper
    {
        public int StopCount { get; private set; }
        public int PauseCount { get; private set; }
        public int UnPauseCount { get; private set; }
        public double LastPlayScheduledTime { get; private set; }
        public double LastScheduledEndTime { get; private set; }

        public override void Stop()
        {
            base.Stop();
            StopCount++;
        }

        public override void Pause()
        {
            base.Pause();
            PauseCount++;
        }

        public override void UnPause()
        {
            base.UnPause();
            UnPauseCount++;
        }

        public override void PlayScheduled(double time)
        {
            base.PlayScheduled(time);
            LastPlayScheduledTime = time;
        }

        public override void SetScheduledEndTime(double time)
        {
            base.SetScheduledEndTime(time);
            LastScheduledEndTime = time;
        }
    }
}
