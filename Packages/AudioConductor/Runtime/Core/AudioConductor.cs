// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     The main entry point for AudioConductor v2.
    ///     Manages audio playback lifecycle on a per-instance basis.
    /// </summary>
    public sealed class AudioConductor : IDisposable
    {
        private readonly Dictionary<int, Category> _categories = new();
        private ConductorBehaviour _behaviour;
        private GameObject _rootObject;

        /// <summary>
        ///     Initializes a new instance of <see cref="AudioConductor" /> with the specified settings.
        ///     Creates a DontDestroyOnLoad root GameObject and attaches a <see cref="ConductorBehaviour" />.
        /// </summary>
        /// <param name="settings">The runtime settings for this conductor instance.</param>
        public AudioConductor(AudioConductorSettings settings)
        {
            _rootObject = new GameObject(nameof(AudioConductor));
            if (Application.isPlaying)
                Object.DontDestroyOnLoad(_rootObject);

            _behaviour = _rootObject.AddComponent<ConductorBehaviour>();
            _behaviour.Conductor = this;

            foreach (var category in settings.categoryList)
                _categories[category.id] = category;
        }

        /// <summary>
        ///     Stops all playback, disconnects the MonoBehaviour delegate, and destroys the root GameObject.
        /// </summary>
        public void Dispose()
        {
            if (_behaviour != null)
            {
                _behaviour.Conductor = null;
                _behaviour = null;
            }

            if (_rootObject != null)
            {
                if (Application.isPlaying)
                    Object.Destroy(_rootObject);
                else
                    Object.DestroyImmediate(_rootObject);
                _rootObject = null;
            }
        }

        internal void Update(float deltaTime)
        {
        }

        /// <summary>
        ///     Gets the AudioMixerGroup assigned to the specified category.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>The AudioMixerGroup, or null if not found.</returns>
        public AudioMixerGroup GetAudioMixerGroup(int categoryId)
        {
            if (_categories.TryGetValue(categoryId, out var category))
                return category.audioMixerGroup;

            return null;
        }
    }
}
