// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
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
        private readonly List<FadeState> _fadeStates = new();
        private readonly List<FadeState> _fadeUpdateTempList = new();
        private readonly AudioClipPlayerProvider _oneShotProvider;
        private readonly List<OneShotState> _oneShotStates = new();
        private readonly List<OneShotState> _oneShotUpdateTempList = new();
        private readonly Dictionary<uint, PlaybackState> _playbacks = new();
        private readonly AudioClipPlayerProvider _playerProvider;
        private readonly ICueSheetProvider _provider;
        private readonly AudioConductorSettings _settings;
        private readonly List<PlaybackState> _updateTempList = new();
        private ConductorBehaviour _behaviour;
        private uint _cueSheetHandleCounter;
        private uint _playStateCounter;
        private GameObject _rootObject;

        /// <summary>
        ///     Initializes a new instance of <see cref="AudioConductor" /> with the specified settings.
        ///     Creates a DontDestroyOnLoad root GameObject and attaches a <see cref="ConductorBehaviour" />.
        /// </summary>
        /// <param name="settings">The runtime settings for this conductor instance.</param>
        /// <param name="provider">Optional provider for async CueSheet loading and releasing.</param>
        public AudioConductor(AudioConductorSettings settings, ICueSheetProvider provider = null)
        {
            _settings = settings;
            _provider = provider;

            _rootObject = new GameObject(nameof(AudioConductor));
            if (Application.isPlaying)
                Object.DontDestroyOnLoad(_rootObject);

            _behaviour = _rootObject.AddComponent<ConductorBehaviour>();
            _behaviour.Conductor = this;

            var managedRoot = new GameObject("Managed");
            managedRoot.transform.SetParent(_rootObject.transform, false);
            _playerProvider = new AudioClipPlayerProvider(managedRoot.transform);

            var oneShotRoot = new GameObject("OneShot");
            oneShotRoot.transform.SetParent(_rootObject.transform, false);
            _oneShotProvider = new AudioClipPlayerProvider(oneShotRoot.transform);

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
            _fadeStates.Clear();

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

        /// <summary>
        ///     Plays a cue from the registered CueSheet identified by the handle.
        ///     Returns a <see cref="PlaybackHandle" /> for controlling the playback.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue to play.</param>
        /// <param name="options">Optional playback settings. If null, CueSheet defaults are used.</param>
        /// <returns>A handle for controlling this playback instance.</returns>
        public PlaybackHandle Play(CueSheetHandle sheetHandle, string cueName, PlayOptions? options = null)
        {
            if (options?.TrackIndex.HasValue == true && !string.IsNullOrEmpty(options?.TrackName))
                throw new ArgumentException("TrackIndex and TrackName are mutually exclusive.");

            if (!sheetHandle.IsValid)
                return default;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return default;

            var cueSheet = registration.Asset.cueSheet;
            var cue = cueSheet.cueList.Find(c => c.name == cueName);
            if (cue == null)
                return default;

            var cueState = new CueState(sheetHandle.Id, cue);

            Track track;
            if (options?.TrackIndex.HasValue == true && options.Value.TrackIndex.HasValue)
                track = cueState.GetTrack(options.Value.TrackIndex.Value);
            else if (!string.IsNullOrEmpty(options?.TrackName))
                track = cueState.GetTrack(options.Value.TrackName);
            else
                track = cueState.NextTrack();

            if (track == null || track.audioClip == null)
                return default;

            if (!CanPlay(sheetHandle.Id, cue, track))
                return default;

            var player = _playerProvider.Rent();
            _categories.TryGetValue(cue.categoryId, out var category);
            var volume = Calculator.CalcVolume(cueSheet, cue, track);
            var pitch = Calculator.CalcPitch(cueSheet, cue, track);
            var isLoop = options?.IsLoop == true || track.isLoop;
            player.Setup(category?.audioMixerGroup, track.audioClip, cue.categoryId, volume, pitch, isLoop,
                track.startSample, track.loopStartSample, track.endSample);
            player.Play();

            var id = ++_playStateCounter;
            var state = new PlaybackState(id, sheetHandle.Id, cue, player) { Priority = track.priority };
            _playbacks[id] = state;

            if (options?.FadeTime > 0f)
            {
                var fader = options.Value.Fader ?? Faders.Linear;
                var fadeState = new FadeState(player, fader);
                fadeState.Setup(0f, volume, options.Value.FadeTime.Value, false);
                player.SetVolumeInternal(0f);
                _fadeStates.Add(fadeState);
            }

            return new PlaybackHandle(id);
        }

        /// <summary>
        ///     Stops the playback identified by the handle.
        ///     When <paramref name="fadeTime" /> is greater than zero, a fade-out begins instead of an immediate stop.
        ///     After stopping (or when fade completes), the handle remains valid but operations become no-ops.
        /// </summary>
        /// <param name="handle">The playback handle to stop.</param>
        /// <param name="fadeTime">Fade-out duration in seconds. When null or zero, the stop is immediate.</param>
        /// <param name="fader">Custom fader curve. When null, <see cref="Faders.Linear" /> is used.</param>
        public void Stop(PlaybackHandle handle, float? fadeTime = null, IFader fader = null)
        {
            if (!handle.IsValid)
                return;

            if (!_playbacks.TryGetValue(handle.Id, out var state))
                return;

            if (fadeTime > 0f)
            {
                // Do not add a duplicate fade-out entry for the same player.
                if (_fadeStates.Exists(f => f.IsStopTarget && ReferenceEquals(f.Fadeable, state.Player)))
                    return;

                var effectiveFader = fader ?? Faders.Linear;
                var fadeState = new FadeState(state.Player, effectiveFader);
                fadeState.Setup(state.Player.VolumeInternal, 0f, fadeTime.Value, true);
                _fadeStates.Add(fadeState);
                return;
            }

            StopPlayback(state);
            _playbacks.Remove(handle.Id);
        }

        /// <summary>
        ///     Pauses the playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle to pause.</param>
        public void Pause(PlaybackHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (!_playbacks.TryGetValue(handle.Id, out var state) || state.Player == null)
                return;

            state.Player.Pause();
        }

        /// <summary>
        ///     Resumes the paused playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle to resume.</param>
        public void Resume(PlaybackHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (!_playbacks.TryGetValue(handle.Id, out var state) || state.Player == null)
                return;

            state.Player.Resume();
        }

        /// <summary>
        ///     Sets the volume of the playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle.</param>
        /// <param name="volume">The volume value.</param>
        public void SetVolume(PlaybackHandle handle, float volume)
        {
            if (!handle.IsValid)
                return;

            if (!_playbacks.TryGetValue(handle.Id, out var state) || state.Player == null)
                return;

            state.Player.SetVolume(volume);
        }

        /// <summary>
        ///     Sets the pitch of the playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle.</param>
        /// <param name="pitch">The pitch value.</param>
        public void SetPitch(PlaybackHandle handle, float pitch)
        {
            if (!handle.IsValid)
                return;

            if (!_playbacks.TryGetValue(handle.Id, out var state) || state.Player == null)
                return;

            state.Player.SetPitch(pitch);
        }

        /// <summary>
        ///     Returns true if the playback identified by the handle is currently playing.
        /// </summary>
        /// <param name="handle">The playback handle.</param>
        /// <returns>True if the playback is active; false otherwise.</returns>
        public bool IsPlaying(PlaybackHandle handle)
        {
            if (!handle.IsValid)
                return false;

            if (!_playbacks.TryGetValue(handle.Id, out var state) || state.Player == null)
                return false;

            return state.Player.IsPlaying;
        }

        internal void Update(float deltaTime)
        {
            // Process active fade states.
            _fadeUpdateTempList.Clear();
            _fadeUpdateTempList.AddRange(_fadeStates);
            foreach (var fade in _fadeUpdateTempList)
            {
                var finished = fade.Elapsed(deltaTime);
                if (finished)
                {
                    _fadeStates.Remove(fade);
                    // When the fade target was stopped, remove its playback entry.
                    if (fade.IsStopTarget)
                        foreach (var kv in _playbacks)
                            if (ReferenceEquals(kv.Value.Player, fade.Fadeable))
                            {
                                _playbacks.Remove(kv.Key);
                                _playerProvider.Return(kv.Value.Player);
                                kv.Value.ReleasePlayer();
                                break;
                            }
                }
            }

            // Copy to temp list first so that _playbacks.Remove inside the loop is safe.
            _updateTempList.Clear();
            foreach (var playback in _playbacks.Values)
                _updateTempList.Add(playback);

            foreach (var playback in _updateTempList)
            {
                if (playback.Player == null)
                    continue;

                playback.Player.ManualUpdate(deltaTime);

                var isFading = _fadeStates.Exists(f => ReferenceEquals(f.Fadeable, playback.Player));
                if (!playback.Player.IsPlaying && !playback.Player.IsPaused && !isFading)
                {
                    _playerProvider.Return(playback.Player);
                    playback.ReleasePlayer();
                    _playbacks.Remove(playback.Id);
                }
            }

            // Copy to temp list first so that _oneShotStates.Remove inside the loop is safe.
            _oneShotUpdateTempList.Clear();
            _oneShotUpdateTempList.AddRange(_oneShotStates);

            foreach (var state in _oneShotUpdateTempList)
            {
                if (state.Player == null)
                    continue;

                state.Player.ManualUpdate(deltaTime);

                if (!state.Player.IsPlaying && !state.Player.IsPaused)
                {
                    _oneShotProvider.Return(state.Player);
                    _oneShotStates.Remove(state);
                }
            }
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

        /// <summary>
        ///     Plays a cue as a fire-and-forget OneShot.
        ///     No handle is returned; the playback cannot be controlled after it starts.
        ///     The AudioClipPlayer is automatically returned to the OneShot pool when playback completes.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue to play.</param>
        public void PlayOneShot(CueSheetHandle sheetHandle, string cueName)
        {
            if (!sheetHandle.IsValid)
                return;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cueSheet = registration.Asset.cueSheet;
            var cue = cueSheet.cueList.Find(c => c.name == cueName);
            if (cue == null)
                return;

            var cueState = new CueState(sheetHandle.Id, cue);
            var track = cueState.NextTrack();

            if (track == null || track.audioClip == null)
                return;

            if (!CanPlay(sheetHandle.Id, cue, track))
                return;

            var player = _oneShotProvider.Rent();
            _categories.TryGetValue(cue.categoryId, out var category);
            var volume = Calculator.CalcVolume(cueSheet, cue, track);
            var pitch = Calculator.CalcPitch(cueSheet, cue, track);
            player.Setup(category?.audioMixerGroup, track.audioClip, cue.categoryId, volume, pitch, false,
                track.startSample, track.loopStartSample, track.endSample);
            player.Play();
            _oneShotStates.Add(new OneShotState(++_playStateCounter, sheetHandle.Id, cue, player, track.priority));
        }

        private bool CanPlay(uint cueSheetId, Cue cue, Track track)
        {
            if (_playbacks.Count == 0 && _oneShotStates.Count == 0)
                return true;

            for (var i = 0; i < (int)LimitCheckType.Count; i++)
            {
                ThrottleType throttleType = default;
                int throttleLimit = default;

                switch ((LimitCheckType)i)
                {
                    case LimitCheckType.Cue:
                        throttleType = cue.throttleType;
                        throttleLimit = cue.throttleLimit;
                        break;
                    case LimitCheckType.CueSheet:
                        if (_cueSheets.TryGetValue(cueSheetId, out var reg))
                        {
                            throttleType = reg.Asset.cueSheet.throttleType;
                            throttleLimit = reg.Asset.cueSheet.throttleLimit;
                        }

                        break;
                    case LimitCheckType.Category:
                        if (_categories.TryGetValue(cue.categoryId, out var cat))
                        {
                            throttleType = cat.throttleType;
                            throttleLimit = cat.throttleLimit;
                        }

                        break;
                    case LimitCheckType.Global:
                        throttleType = _settings.throttleType;
                        throttleLimit = _settings.throttleLimit;
                        break;
                }

                if (throttleLimit <= 0)
                    continue;

                var playNum = (LimitCheckType)i switch
                {
                    LimitCheckType.Cue => CountPlayingByCue(cue),
                    LimitCheckType.CueSheet => CountPlayingByCueSheet(cueSheetId),
                    LimitCheckType.Category => CountPlayingByCategory(cue.categoryId),
                    LimitCheckType.Global => CountAllPlaying(),
                    _ => 0
                };

                if (playNum < throttleLimit)
                    continue;

                // Compute minimum priority within the scope of this limit check.
                var minPriority = (LimitCheckType)i switch
                {
                    LimitCheckType.Cue => MinPriorityByCue(cue),
                    LimitCheckType.CueSheet => MinPriorityByCueSheet(cueSheetId),
                    LimitCheckType.Category => MinPriorityByCategory(cue.categoryId),
                    LimitCheckType.Global => MinPriorityGlobal(),
                    _ => int.MaxValue
                };

                if (minPriority > track.priority)
                    return false;

                if (minPriority < track.priority)
                    throttleType = ThrottleType.PriorityOrder;

                switch (throttleType)
                {
                    case ThrottleType.PriorityOrder:
                        switch ((LimitCheckType)i)
                        {
                            case LimitCheckType.Cue:
                                StopOldestByCue(cue, minPriority);
                                break;
                            case LimitCheckType.CueSheet:
                                StopOldestByCueSheet(cueSheetId, minPriority);
                                break;
                            case LimitCheckType.Category:
                                StopOldestByCategory(cue.categoryId, minPriority);
                                break;
                            case LimitCheckType.Global:
                                StopOldestGlobal(minPriority);
                                break;
                        }

                        break;
                    case ThrottleType.FirstComeFirstServed:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        private void StopPlayback(PlaybackState playback)
        {
            if (playback.Player == null)
                return;

            playback.Player.Stop();
            _playerProvider.Return(playback.Player);
            playback.ReleasePlayer();
        }

        private int CountAllPlaying()
        {
            var count = 0;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying)
                    count++;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying)
                    count++;
            return count;
        }

        private int CountPlayingByCue(Cue cue)
        {
            var count = 0;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Cue == cue)
                    count++;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Cue == cue)
                    count++;
            return count;
        }

        private int CountPlayingByCueSheet(uint cueSheetId)
        {
            var count = 0;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.CueSheetId == cueSheetId)
                    count++;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.CueSheetId == cueSheetId)
                    count++;
            return count;
        }

        private int CountPlayingByCategory(int categoryId)
        {
            var count = 0;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Cue.categoryId == categoryId)
                    count++;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Cue.categoryId == categoryId)
                    count++;
            return count;
        }

        private int MinPriorityByCue(Cue cue)
        {
            var min = int.MaxValue;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Cue == cue && p.Priority < min)
                    min = p.Priority;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Cue == cue && s.Priority < min)
                    min = s.Priority;
            return min;
        }

        private int MinPriorityByCueSheet(uint cueSheetId)
        {
            var min = int.MaxValue;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.CueSheetId == cueSheetId && p.Priority < min)
                    min = p.Priority;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.CueSheetId == cueSheetId && s.Priority < min)
                    min = s.Priority;
            return min;
        }

        private int MinPriorityByCategory(int categoryId)
        {
            var min = int.MaxValue;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Cue.categoryId == categoryId && p.Priority < min)
                    min = p.Priority;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Cue.categoryId == categoryId && s.Priority < min)
                    min = s.Priority;
            return min;
        }

        private int MinPriorityGlobal()
        {
            var min = int.MaxValue;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Priority < min)
                    min = p.Priority;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Priority < min)
                    min = s.Priority;
            return min;
        }

        // Stops the oldest (lowest Id) managed or one-shot playback in scope matching the given priority.
        private void StopOldestByCue(Cue cue, int minPriority)
        {
            PlaybackState oldestManaged = null;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Cue == cue && p.Priority == minPriority)
                    if (oldestManaged == null || p.Id < oldestManaged.Id)
                        oldestManaged = p;

            OneShotState oldestOneShot = null;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Cue == cue && s.Priority == minPriority)
                    if (oldestOneShot == null || s.Id < oldestOneShot.Id)
                        oldestOneShot = s;

            StopOldestCandidate(oldestManaged, oldestOneShot);
        }

        private void StopOldestByCueSheet(uint cueSheetId, int minPriority)
        {
            PlaybackState oldestManaged = null;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.CueSheetId == cueSheetId &&
                    p.Priority == minPriority)
                    if (oldestManaged == null || p.Id < oldestManaged.Id)
                        oldestManaged = p;

            OneShotState oldestOneShot = null;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.CueSheetId == cueSheetId &&
                    s.Priority == minPriority)
                    if (oldestOneShot == null || s.Id < oldestOneShot.Id)
                        oldestOneShot = s;

            StopOldestCandidate(oldestManaged, oldestOneShot);
        }

        private void StopOldestByCategory(int categoryId, int minPriority)
        {
            PlaybackState oldestManaged = null;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Cue.categoryId == categoryId &&
                    p.Priority == minPriority)
                    if (oldestManaged == null || p.Id < oldestManaged.Id)
                        oldestManaged = p;

            OneShotState oldestOneShot = null;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Cue.categoryId == categoryId &&
                    s.Priority == minPriority)
                    if (oldestOneShot == null || s.Id < oldestOneShot.Id)
                        oldestOneShot = s;

            StopOldestCandidate(oldestManaged, oldestOneShot);
        }

        private void StopOldestGlobal(int minPriority)
        {
            PlaybackState oldestManaged = null;
            foreach (var p in _playbacks.Values)
                if (p.Player != null && p.Player.IsPlaying && p.Priority == minPriority)
                    if (oldestManaged == null || p.Id < oldestManaged.Id)
                        oldestManaged = p;

            OneShotState oldestOneShot = null;
            foreach (var s in _oneShotStates)
                if (s.Player != null && s.Player.IsPlaying && s.Priority == minPriority)
                    if (oldestOneShot == null || s.Id < oldestOneShot.Id)
                        oldestOneShot = s;

            StopOldestCandidate(oldestManaged, oldestOneShot);
        }

        private void StopOldestCandidate(PlaybackState oldestManaged, OneShotState oldestOneShot)
        {
            // Compare Managed and OneShot by their insertion-order Id to find the globally oldest.
            if (oldestManaged != null && (oldestOneShot == null || oldestManaged.Id <= oldestOneShot.Id))
            {
                StopPlayback(oldestManaged);
                _playbacks.Remove(oldestManaged.Id);
            }
            else if (oldestOneShot != null)
            {
                oldestOneShot.Player.Stop();
                _oneShotProvider.Return(oldestOneShot.Player);
                _oneShotStates.Remove(oldestOneShot);
            }
        }

        private sealed class CueSheetRegistration
        {
            internal CueSheetRegistration(CueSheetAsset asset)
            {
                Asset = asset;
            }

            internal CueSheetAsset Asset { get; }
        }

        private sealed class PlaybackState
        {
            internal PlaybackState(uint id, uint cueSheetId, Cue cue, AudioClipPlayer player)
            {
                Id = id;
                CueSheetId = cueSheetId;
                Cue = cue;
                Player = player;
                Priority = 0;
            }

            internal uint Id { get; }
            internal uint CueSheetId { get; }
            internal Cue Cue { get; }
            internal AudioClipPlayer Player { get; private set; }
            internal int Priority { get; set; }

            internal void ReleasePlayer()
            {
                Player = null;
            }
        }

        private sealed class OneShotState
        {
            internal OneShotState(uint id, uint cueSheetId, Cue cue, AudioClipPlayer player, int priority)
            {
                Id = id;
                CueSheetId = cueSheetId;
                Cue = cue;
                Player = player;
                Priority = priority;
            }

            internal uint Id { get; }
            internal uint CueSheetId { get; }
            internal Cue Cue { get; }
            internal AudioClipPlayer Player { get; }
            internal int Priority { get; }
        }

        private enum LimitCheckType
        {
            Cue,
            CueSheet,
            Category,
            Global,

            Count
        }
    }
}
