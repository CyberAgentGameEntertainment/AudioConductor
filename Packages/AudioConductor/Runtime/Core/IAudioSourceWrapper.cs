// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core
{
    internal interface IAudioSourceWrapper
    {
        bool IsPlaying { get; }
        int TimeSamples { get; set; }
        float Volume { set; }
        float Pitch { set; }
        bool Enabled { get; set; }
        bool Loop { set; }
        bool PlayOnAwake { set; }
        AudioClip? Clip { get; set; }
        AudioMixerGroup? OutputAudioMixerGroup { set; }
        void Stop();
        void Pause();
        void UnPause();
        void PlayScheduled(double time);
        void SetScheduledEndTime(double time);
    }
}
