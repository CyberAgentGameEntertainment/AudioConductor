// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class SpyAudioSourceWrapper : IAudioSourceWrapper
    {
        private bool _wasPlayingBeforePause;
        public int StopCount { get; private set; }
        public int PauseCount { get; private set; }
        public int UnPauseCount { get; private set; }
        public double LastPlayScheduledTime { get; private set; }
        public double LastScheduledEndTime { get; private set; }
        public bool IsPlaying { get; set; }
        public int TimeSamples { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Loop { get; set; }
        public bool PlayOnAwake { get; set; }
        public AudioClip? Clip { get; set; }
        public AudioMixerGroup? OutputAudioMixerGroup { get; set; }

        public void Stop()
        {
            IsPlaying = false;
            StopCount++;
        }

        public void Pause()
        {
            _wasPlayingBeforePause = IsPlaying;
            IsPlaying = false;
            PauseCount++;
        }

        public void UnPause()
        {
            if (_wasPlayingBeforePause)
                IsPlaying = true;
            _wasPlayingBeforePause = false;
            UnPauseCount++;
        }

        public void PlayScheduled(double time)
        {
            IsPlaying = true;
            LastPlayScheduledTime = time;
        }

        public void SetScheduledEndTime(double time)
        {
            LastScheduledEndTime = time;
        }
    }
}
