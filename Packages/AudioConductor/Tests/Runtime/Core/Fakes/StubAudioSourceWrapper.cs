// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core.Tests.Fakes
{
    internal class StubAudioSourceWrapper : IAudioSourceWrapper
    {
        private bool _wasPlayingBeforePause;
        public bool IsPlaying { get; set; }
        public int TimeSamples { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Loop { get; set; }
        public bool PlayOnAwake { get; set; }
        public AudioClip? Clip { get; set; }
        public AudioMixerGroup? OutputAudioMixerGroup { get; set; }

        public virtual void Stop()
        {
            IsPlaying = false;
        }

        public virtual void Pause()
        {
            _wasPlayingBeforePause = IsPlaying;
            IsPlaying = false;
        }

        public virtual void UnPause()
        {
            if (_wasPlayingBeforePause)
                IsPlaying = true;
            _wasPlayingBeforePause = false;
        }

        public virtual void PlayScheduled(double time)
        {
            IsPlaying = true;
        }

        public virtual void SetScheduledEndTime(double time)
        {
        }
    }
}
