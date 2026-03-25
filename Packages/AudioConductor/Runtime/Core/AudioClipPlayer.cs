// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Enums;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core
{
    internal sealed class AudioClipPlayer : MonoBehaviour, IInternalPlayer
    {
        private static readonly string[] AudioSourceNames = { "AudioSource1", "AudioSource2" };

        private AudioClipPlayerCore _core = null!;

        public uint ActiveFadeId
        {
            get => _core.ActiveFadeId;
            set => _core.ActiveFadeId = value;
        }

        public FadeState FadeState
        {
            get => _core.FadeState;
            set => _core.FadeState = value;
        }

        public int ClipSamples => _core.ClipSamples;
        public int CategoryId => _core.CategoryId;
        public PlayerState State => _core.State;
        public float VolumeFade => _core.VolumeFade;

        public void Setup(AudioMixerGroup? audioMixerGroup,
            AudioClip clip,
            int categoryId,
            float volume,
            float pitch,
            bool isLoop,
            int startSample,
            int loopStartSample,
            int endSample)
        {
            _core.Setup(audioMixerGroup, clip, categoryId, volume, pitch, isLoop, startSample, loopStartSample,
                endSample);
        }

        public void Play()
        {
            _core.Play();
        }

        public void Restart()
        {
            _core.Restart();
        }

        public void Pause()
        {
            _core.Pause();
        }

        public void Resume()
        {
            _core.Resume();
        }

        public void Stop()
        {
            _core.Stop();
        }

        public float GetActualVolume()
        {
            return _core.GetActualVolume();
        }

        public float GetVolume()
        {
            return _core.GetVolume();
        }

        public void SetVolume(float volume)
        {
            _core.SetVolume(volume);
        }

        public float GetActualPitch()
        {
            return _core.GetActualPitch();
        }

        public float GetPitch()
        {
            return _core.GetPitch();
        }

        public void SetPitch(float pitch)
        {
            _core.SetPitch(pitch);
        }

        public void AddStopAction(Action onStop)
        {
            _core.AddStopAction(onStop);
        }

        public void AddEndAction(Action onEnd)
        {
            _core.AddEndAction(onEnd);
        }

        public int GetCurrentSample()
        {
            return _core.GetCurrentSample();
        }

        public void SetCurrentSample(int sample)
        {
            _core.SetCurrentSample(sample);
        }

        public void SetVolumeFade(float fade)
        {
            _core.SetVolumeFade(fade);
        }

        public void SetMasterVolume(float volume)
        {
            _core.SetMasterVolume(volume);
        }

        public void SetCategoryVolume(float volume)
        {
            _core.SetCategoryVolume(volume);
        }

        public void ManualUpdate(float deltaTime)
        {
            _core.ManualUpdate(deltaTime);
        }

        public void ResetState()
        {
            _core.ResetState();
        }

        internal static AudioClipPlayer Create(Transform parent)
        {
            var root = new GameObject(nameof(AudioClipPlayer));
            root.transform.SetParent(parent);

            var player = root.AddComponent<AudioClipPlayer>();
            var sources = new IAudioSourceWrapper[2];
            for (var i = 0; i < 2; i++)
            {
                var child = new GameObject(AudioSourceNames[i]);
                child.transform.SetParent(root.transform);
                sources[i] = new AudioSourceWrapper(child.AddComponent<AudioSource>());
            }

            player._core = new AudioClipPlayerCore(sources, new DspClock());
            return player;
        }
    }
}
