// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Runtime.Core;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    /// <summary>
    ///     Provide only the functions needed for editor
    /// </summary>
    internal sealed class TrackPreviewController : IDisposable
    {
        private IAudioClipPlayer _player;

        public TrackPreviewController(AudioClip clip,
                                      int categoryId,
                                      float volume,
                                      float pitch,
                                      bool isLoop,
                                      int startSample, int loopStartSample, int endSample)
        {
            _player = AudioConductorInterface.RentUnmanagedPlayer();
            var category = CategoryListRepository.instance.Find(categoryId);
            _player.Setup(category.audioMixerGroup, clip, category.id, volume, pitch, isLoop,
                          startSample, loopStartSample, endSample);
        }

        public bool IsPlaying => _player.IsPlaying;

        public void Dispose()
        {
            if (_player == null)
                return;

            AudioConductorInterface.ReturnUnmanagedPlayer(_player);
            _player = null;
        }

        public void Play()
            => _player.Play();

        public void Stop()
            => _player.Stop();

        public void SetCurrentSample(int sample)
            => _player.SetCurrentSample(sample);

        public int GetCurrentSample()
            => _player.GetCurrentSample();
    }
}
