// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    /// <summary>
    ///     Provide only the functions needed for editor
    /// </summary>
    internal sealed class TrackPreviewController : IDisposable
    {
        private AudioSource? _audioSource;
        private GameObject? _gameObject;

        public TrackPreviewController(AudioClip clip,
            int categoryId,
            float volume,
            float pitch,
            bool isLoop,
            int startSample)
        {
            _gameObject = new GameObject("AudioConductor_TrackPreview");
            _gameObject.hideFlags = HideFlags.HideAndDontSave;

            _audioSource = _gameObject.AddComponent<AudioSource>();

            var category = CategoryListRepository.instance.Find(categoryId);
            _audioSource.outputAudioMixerGroup = category.audioMixerGroup;
            _audioSource.clip = clip;
            _audioSource.volume = volume;
            _audioSource.pitch = pitch;
            _audioSource.loop = isLoop;
            _audioSource.timeSamples = startSample;
            _audioSource.playOnAwake = false;
        }

        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

        public void Dispose()
        {
            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
                _gameObject = null;
                _audioSource = null;
            }
        }

        public void Play()
        {
            if (_audioSource != null)
                _audioSource.Play();
        }

        public void Stop()
        {
            if (_audioSource != null)
                _audioSource.Stop();
        }

        public void Pause()
        {
            if (_audioSource != null)
                _audioSource.Pause();
        }

        public void UnPause()
        {
            if (_audioSource != null)
                _audioSource.UnPause();
        }

        public void SetCurrentSample(int sample)
        {
            if (_audioSource != null)
                _audioSource.timeSamples = sample;
        }

        public int GetCurrentSample()
        {
            return _audioSource != null ? _audioSource.timeSamples : 0;
        }
    }
}
