// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly Dictionary<uint, CueSheetRegistration> _cueSheets = new();
        private readonly ICueSheetProvider _provider;
        private ConductorBehaviour _behaviour;
        private uint _cueSheetHandleCounter;
        private GameObject _rootObject;

        /// <summary>
        ///     Initializes a new instance of <see cref="AudioConductor" /> with the specified settings.
        ///     Creates a DontDestroyOnLoad root GameObject and attaches a <see cref="ConductorBehaviour" />.
        /// </summary>
        /// <param name="settings">The runtime settings for this conductor instance.</param>
        /// <param name="provider">Optional provider for async CueSheet loading and releasing.</param>
        public AudioConductor(AudioConductorSettings settings, ICueSheetProvider provider = null)
        {
            _provider = provider;

            _rootObject = new GameObject(nameof(AudioConductor));
            if (Application.isPlaying)
                Object.DontDestroyOnLoad(_rootObject);

            _behaviour = _rootObject.AddComponent<ConductorBehaviour>();
            _behaviour.Conductor = this;

            foreach (var category in settings.categoryList)
                _categories[category.id] = category;
        }

        /// <summary>
        ///     Unregisters all CueSheets, stops all playback, disconnects the MonoBehaviour delegate,
        ///     and destroys the root GameObject.
        /// </summary>
        public void Dispose()
        {
            foreach (var registration in _cueSheets.Values)
                _provider?.Release(registration.Asset);
            _cueSheets.Clear();

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

        /// <summary>
        ///     Registers a loaded <see cref="CueSheetAsset" /> and returns a handle for playback.
        ///     Multiple registrations of the same asset are allowed; each returns a distinct handle.
        /// </summary>
        /// <param name="asset">The CueSheet asset to register.</param>
        /// <returns>A handle that identifies this registration.</returns>
        public CueSheetHandle RegisterCueSheet(CueSheetAsset asset)
        {
            var id = ++_cueSheetHandleCounter;
            _cueSheets[id] = new CueSheetRegistration(asset);
            return new CueSheetHandle(id);
        }

        /// <summary>
        ///     Asynchronously loads and registers a CueSheet via the configured <see cref="ICueSheetProvider" />.
        /// </summary>
        /// <param name="key">The asset key passed to the provider.</param>
        /// <returns>A handle that identifies this registration.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no provider is configured.</exception>
        public async Task<CueSheetHandle> RegisterCueSheetAsync(string key)
        {
            if (_provider == null)
                throw new InvalidOperationException("ICueSheetProvider is not set.");

            var asset = await _provider.LoadAsync(key);
            return RegisterCueSheet(asset);
        }

        /// <summary>
        ///     Unregisters the CueSheet identified by the handle.
        ///     If a provider is configured, <see cref="ICueSheetProvider.Release" /> is called.
        /// </summary>
        /// <param name="handle">The handle returned by <see cref="RegisterCueSheet" />.</param>
        public void UnregisterCueSheet(CueSheetHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (!_cueSheets.TryGetValue(handle.Id, out var registration))
                return;

            _provider?.Release(registration.Asset);
            _cueSheets.Remove(handle.Id);
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

        private sealed class CueSheetRegistration
        {
            internal CueSheetRegistration(CueSheetAsset asset)
            {
                Asset = asset;
            }

            internal CueSheetAsset Asset { get; }
        }
    }
}
