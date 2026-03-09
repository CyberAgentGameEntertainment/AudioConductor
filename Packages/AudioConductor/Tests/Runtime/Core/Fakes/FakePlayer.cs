// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class FakePlayer : IInternalPlayer
    {
        public int StopCount { get; private set; }
        public bool StopCalled => StopCount > 0;
        public bool IsPlaying { get; set; }
        public bool IsPaused { get; set; }
        public uint ActiveFadeId { get; set; }
        public bool IsFading { get; set; }
        public int ClipSamples { get; private set; }
        public int CategoryId { get; private set; }
        public float VolumeFade { get; private set; } = 1f;

        public void Setup(AudioMixerGroup? audioMixerGroup, AudioClip clip, int categoryId, float volume, float pitch,
            bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            CategoryId = categoryId;
            if (clip != null)
                ClipSamples = clip.samples;
        }

        public void Play()
        {
            IsPlaying = true;
            IsPaused = false;
        }

        public void Restart()
        {
            Play();
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                IsPlaying = false;
                IsPaused = true;
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsPlaying = true;
                IsPaused = false;
            }
        }

        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            StopCount++;
        }

        public float GetActualVolume()
        {
            return 1f;
        }

        public float GetVolume()
        {
            return 1f;
        }

        public void SetVolume(float volume)
        {
        }

        public float GetActualPitch()
        {
            return 1f;
        }

        public float GetPitch()
        {
            return 1f;
        }

        public void SetPitch(float pitch)
        {
        }

        public void AddStopAction(Action onStop)
        {
        }

        public void AddEndAction(Action onEnd)
        {
        }

        public int GetCurrentSample()
        {
            return 0;
        }

        public void SetCurrentSample(int sample)
        {
        }

        public void SetVolumeFade(float fade)
        {
            VolumeFade = fade;
        }

        public void SetMasterVolume(float volume)
        {
        }

        public void ManualUpdate(float deltaTime)
        {
        }

        public void ResetState()
        {
            IsPlaying = false;
            IsPaused = false;
            ActiveFadeId = 0;
            IsFading = false;
            StopCount = 0;
            VolumeFade = 1f;
        }
    }
}
