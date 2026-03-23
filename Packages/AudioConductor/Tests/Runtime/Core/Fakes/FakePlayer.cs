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
        public float CategoryVolume { get; private set; } = 1f;

        public int PlayCount { get; private set; }
        public int PauseCount { get; private set; }
        public int ResumeCount { get; private set; }
        public int SetupCount { get; private set; }
        public int ManualUpdateCount { get; private set; }
        public float Volume { get; private set; }
        public float Pitch { get; private set; }
        public float MasterVolume { get; private set; }
        public int CurrentSample { get; private set; }
        public float LastDeltaTime { get; private set; }
        public float SetupVolume { get; private set; }
        public float SetupPitch { get; private set; }
        public bool SetupIsLoop { get; private set; }
        public int SetupStartSample { get; private set; }
        public int SetupLoopStartSample { get; private set; }
        public int SetupEndSample { get; private set; }
        public Action? LastStopAction { get; private set; }
        public Action? LastEndAction { get; private set; }
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
            SetupCount++;
            SetupVolume = volume;
            SetupPitch = pitch;
            SetupIsLoop = isLoop;
            SetupStartSample = startSample;
            SetupLoopStartSample = loopStartSample;
            SetupEndSample = endSample;
        }

        public void Play()
        {
            IsPlaying = true;
            IsPaused = false;
            PlayCount++;
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
                PauseCount++;
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsPlaying = true;
                IsPaused = false;
                ResumeCount++;
            }
        }

        public void Stop()
        {
            IsPlaying = false;
            IsPaused = false;
            StopCount++;
            LastStopAction?.Invoke();
            LastStopAction = null;
        }

        public float GetActualVolume()
        {
            return Volume * VolumeFade * MasterVolume * CategoryVolume;
        }

        public float GetVolume()
        {
            return Volume;
        }

        public void SetVolume(float volume)
        {
            Volume = volume;
        }

        public float GetActualPitch()
        {
            return Pitch;
        }

        public float GetPitch()
        {
            return Pitch;
        }

        public void SetPitch(float pitch)
        {
            Pitch = pitch;
        }

        public void AddStopAction(Action onStop)
        {
            LastStopAction += onStop;
        }

        public void AddEndAction(Action onEnd)
        {
            LastEndAction += onEnd;
        }

        public int GetCurrentSample()
        {
            return CurrentSample;
        }

        public void SetCurrentSample(int sample)
        {
            CurrentSample = sample;
        }

        public void SetVolumeFade(float fade)
        {
            VolumeFade = fade;
        }

        public void SetMasterVolume(float volume)
        {
            MasterVolume = volume;
        }

        public void SetCategoryVolume(float volume)
        {
            CategoryVolume = volume;
        }

        public void ManualUpdate(float deltaTime)
        {
            ManualUpdateCount++;
            LastDeltaTime = deltaTime;
        }

        public void ResetState()
        {
            IsPlaying = false;
            IsPaused = false;
            ActiveFadeId = 0;
            IsFading = false;
            StopCount = 0;
            CategoryVolume = 1f;
            VolumeFade = 1f;
            PlayCount = 0;
            PauseCount = 0;
            ResumeCount = 0;
            SetupCount = 0;
            ManualUpdateCount = 0;
            Volume = 0f;
            Pitch = 0f;
            MasterVolume = 0f;
            CurrentSample = 0;
            LastDeltaTime = 0f;
            SetupVolume = 0f;
            SetupPitch = 0f;
            SetupIsLoop = false;
            SetupStartSample = 0;
            SetupLoopStartSample = 0;
            SetupEndSample = 0;
            LastStopAction = null;
            LastEndAction = null;
        }
    }
}
