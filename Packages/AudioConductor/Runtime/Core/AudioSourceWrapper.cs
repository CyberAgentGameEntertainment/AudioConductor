// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core
{
    internal sealed class AudioSourceWrapper : IAudioSourceWrapper
    {
        private readonly AudioSource _source;

        internal AudioSourceWrapper(AudioSource source)
        {
            _source = source;
        }

        public bool IsPlaying
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.isPlaying;
        }

        public int TimeSamples
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.timeSamples;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.timeSamples = value;
        }

        public float Volume
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.volume = value;
        }

        public float Pitch
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.pitch = value;
        }

        public bool Enabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.enabled;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.enabled = value;
        }

        public bool Loop
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.loop = value;
        }

        public bool PlayOnAwake
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.playOnAwake = value;
        }

        public AudioClip? Clip
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source.clip;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.clip = value;
        }

        public AudioMixerGroup? OutputAudioMixerGroup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _source.outputAudioMixerGroup = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            _source.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Pause()
        {
            _source.Pause();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnPause()
        {
            _source.UnPause();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PlayScheduled(double time)
        {
            _source.PlayScheduled(time);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetScheduledEndTime(double time)
        {
            _source.SetScheduledEndTime(time);
        }
    }
}
