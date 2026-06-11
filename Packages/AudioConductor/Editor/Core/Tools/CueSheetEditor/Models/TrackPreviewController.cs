// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core;
using AudioConductor.Core.Enums;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    /// <summary>
    ///     Provide only the functions needed for editor
    /// </summary>
    internal sealed class TrackPreviewController : IDisposable
    {
        private GameObject? _gameObject;
        private AudioClipPlayer? _player;

        public TrackPreviewController(AudioClip clip,
            int categoryId,
            float volume,
            float pitch,
            bool isLoop,
            int startSample,
            int loopStartSample,
            int endSample)
        {
            _gameObject = new GameObject("AudioConductor_TrackPreview")
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            _player = AudioClipPlayer.Create(_gameObject.transform, HideFlags.HideAndDontSave);

            var category = CategoryListRepository.instance.Find(categoryId);
            _player.Setup(category.audioMixerGroup, clip, categoryId, volume, pitch, isLoop,
                startSample, loopStartSample, endSample);

            EditorApplication.update += OnEditorUpdate;
        }

        public bool IsPlaying => _player is { State: PlayerState.Playing };

        public void Dispose()
        {
            EditorApplication.update -= OnEditorUpdate;

            if (_gameObject != null)
            {
                Object.DestroyImmediate(_gameObject);
                _gameObject = null;
                _player = null;
            }
        }

        public void Play()
        {
            _player?.Play();
        }

        public void Stop()
        {
            _player?.Stop();
        }

        public void Pause()
        {
            _player?.Pause();
        }

        public void UnPause()
        {
            _player?.Resume();
        }

        public void SetCurrentSample(int sample)
        {
            _player?.SetCurrentSample(sample);
        }

        public int GetCurrentSample()
        {
            return _player?.GetCurrentSample() ?? 0;
        }

        private void OnEditorUpdate()
        {
            _player?.ManualUpdate(0f);
        }
    }
}
