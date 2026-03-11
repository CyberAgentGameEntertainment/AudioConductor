// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core
{
    /// <summary>
    ///     The main entry point for AudioConductor v2.
    ///     Manages audio playback lifecycle on a per-instance basis.
    /// </summary>
    public sealed partial class Conductor : IDisposable
    {
        private const int BufferInitialCapacity = 64;
        private readonly Dictionary<int, Category> _categories = new();
        private readonly Dictionary<int, float> _categoryVolumes = new();
        private readonly Dictionary<uint, CueSheetRegistration> _cueSheets = new();
        private readonly FadeManager _fadeManager = new();
        private readonly IPlayerProvider _oneShotProvider;
        private readonly List<OneShotState> _oneShotStates = new();
        private readonly Dictionary<uint, PlaybackState> _playbacks = new();
        private readonly IPlayerProvider _playerProvider;
        private readonly ICueSheetProvider? _provider;
        private readonly List<uint> _removeKeyBuffer = new(BufferInitialCapacity);
        private readonly AudioConductorSettings _settings;
        private readonly List<uint> _stopAllKeyBuffer = new(BufferInitialCapacity);
        private ConductorBehaviour? _behaviour;
        private uint _cueSheetHandleCounter;
        private float _masterVolume = 1f;
        private uint _playStateCounter;
        private GameObject? _rootObject;

        /// <summary>
        ///     Initializes a new instance of <see cref="Conductor" /> with the specified settings.
        ///     Creates a DontDestroyOnLoad root GameObject and attaches a <see cref="ConductorBehaviour" />.
        /// </summary>
        /// <param name="settings">The runtime settings for this conductor instance.</param>
        /// <param name="provider">Optional provider for async CueSheet loading and releasing.</param>
        public Conductor(AudioConductorSettings settings, ICueSheetProvider? provider = null)
        {
            _settings = settings;
            _provider = provider;

            _rootObject = new GameObject(nameof(Conductor));
            if (Application.isPlaying)
                Object.DontDestroyOnLoad(_rootObject);

            _behaviour = _rootObject.AddComponent<ConductorBehaviour>();
            _behaviour.Conductor = this;

            var managedRoot = new GameObject("Managed");
            managedRoot.transform.SetParent(_rootObject.transform, false);
            _playerProvider = new AudioClipPlayerProvider(managedRoot.transform, settings.deactivatePooledObjects);

            var oneShotRoot = new GameObject("OneShot");
            oneShotRoot.transform.SetParent(_rootObject.transform, false);
            _oneShotProvider = new AudioClipPlayerProvider(oneShotRoot.transform, settings.deactivatePooledObjects);

            foreach (var category in settings.categoryList)
                _categories[category.id] = category;

            _playerProvider.Prewarm(settings.managedPoolCapacity);
            _oneShotProvider.Prewarm(settings.oneShotPoolCapacity);
        }

        /// <summary>
        ///     Test-only constructor that bypasses GameObject/MonoBehaviour creation.
        /// </summary>
        internal Conductor(AudioConductorSettings settings, IPlayerProvider managedProvider,
            IPlayerProvider oneShotProvider)
        {
            _settings = settings;
            _provider = null;
            _playerProvider = managedProvider;
            _oneShotProvider = oneShotProvider;
            foreach (var category in settings.categoryList)
                _categories[category.id] = category;
        }

        /// <summary>
        ///     Unregisters all CueSheets, stops all playback, disconnects the MonoBehaviour delegate,
        ///     and destroys the root GameObject.
        /// </summary>
        public void Dispose()
        {
            foreach (var playback in _playbacks.Values)
                StopPlayback(playback);
            _playbacks.Clear();

            foreach (var state in _oneShotStates)
                if (state.Player != null)
                {
                    state.Player.Stop();
                    _oneShotProvider.Return(state.Player);
                }

            _oneShotStates.Clear();
            _fadeManager.Dispose();

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
    }
}
