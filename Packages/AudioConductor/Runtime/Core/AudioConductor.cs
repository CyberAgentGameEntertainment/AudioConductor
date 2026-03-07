// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private const int BufferInitialCapacity = 64;
        private const int FadePoolInitialCapacity = 8;
        private readonly Dictionary<int, Category> _categories = new();
        private readonly Dictionary<uint, CueSheetRegistration> _cueSheets = new();
        private readonly HashSet<IFadeable> _fadeOutTargets = new(FadePoolInitialCapacity);
        private readonly Stack<FadeState> _fadePool = new(FadePoolInitialCapacity);
        private readonly List<FadeState> _fadeStates = new();
        private readonly AudioClipPlayerProvider _oneShotProvider;
        private readonly List<OneShotState> _oneShotStates = new();
        private readonly Dictionary<uint, PlaybackState> _playbacks = new();
        private readonly AudioClipPlayerProvider _playerProvider;
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
        ///     Initializes a new instance of <see cref="AudioConductor" /> with the specified settings.
        ///     Creates a DontDestroyOnLoad root GameObject and attaches a <see cref="ConductorBehaviour" />.
        /// </summary>
        /// <param name="settings">The runtime settings for this conductor instance.</param>
        /// <param name="provider">Optional provider for async CueSheet loading and releasing.</param>
        public AudioConductor(AudioConductorSettings settings, ICueSheetProvider? provider = null)
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
            _playerProvider = new AudioClipPlayerProvider(managedRoot.transform, settings.deactivatePooledObjects);

            var oneShotRoot = new GameObject("OneShot");
            oneShotRoot.transform.SetParent(_rootObject.transform, false);
            _oneShotProvider = new AudioClipPlayerProvider(oneShotRoot.transform, settings.deactivatePooledObjects);

            foreach (var category in settings.categoryList)
                _categories[category.id] = category;

            _playerProvider.Prewarm(settings.managedPoolCapacity);
            _oneShotProvider.Prewarm(settings.oneShotPoolCapacity);

            for (var i = 0; i < FadePoolInitialCapacity; i++)
                _fadePool.Push(new FadeState());
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
            _fadeOutTargets.Clear();

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
            if (asset == null)
                throw new InvalidOperationException($"Failed to load CueSheet with key '{key}'.");
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

            if (options?.TrackIndex.HasValue == true && options?.Selector != null)
                throw new ArgumentException("TrackIndex and Selector are mutually exclusive.");

            if (!string.IsNullOrEmpty(options?.TrackName) && options?.Selector != null)
                throw new ArgumentException("TrackName and Selector are mutually exclusive.");

            if (!sheetHandle.IsValid)
                return default;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return default;

            var cue = registration.FindCue(cueName);
            if (cue == null)
                return default;

            var cueState = registration.GetOrCreateCueState(sheetHandle.Id, cue);

            Track? track;
            if (options?.TrackIndex.HasValue == true && options.Value.TrackIndex.HasValue)
                track = cueState.GetTrack(options.Value.TrackIndex.Value);
            else if (!string.IsNullOrEmpty(options?.TrackName))
                track = cueState.GetTrack(options!.Value.TrackName!);
            else
                track = cueState.NextTrack(options?.Selector);

            if (track == null || track.audioClip == null)
                return default;

            if (!CanPlay(sheetHandle.Id, cue, track))
                return default;

            var player = _playerProvider.Rent();
            _categories.TryGetValue(cue.categoryId, out var category);
            var cueSheet = registration.Asset.cueSheet;
            var volume = Calculator.CalcVolume(cueSheet, cue, track);
            var pitch = Calculator.CalcPitch(cueSheet, cue, track);
            var isLoop = options?.IsLoop == true || track.isLoop;
            player.Setup(category?.audioMixerGroup, track.audioClip, cue.categoryId, volume, pitch, isLoop,
                track.startSample, track.loopStartSample, track.endSample);
            player.Play();
            player.SetMasterVolume(_masterVolume);

            var id = ++_playStateCounter;
            var state = new PlaybackState(id, sheetHandle.Id, cue, player, track.priority);
            _playbacks[id] = state;

            if (options?.FadeTime > 0f)
            {
                var fader = options.Value.Fader ?? Faders.Linear;
                var fadeState = RentFadeState();
                fadeState.Setup(player, fader, 0f, 1f, options.Value.FadeTime.Value, false);
                player.SetVolumeFade(0f);
                player.IsFading = true;
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
        public void Stop(PlaybackHandle handle, float? fadeTime = null, IFader? fader = null)
        {
            if (!handle.IsValid)
                return;

            if (!_playbacks.TryGetValue(handle.Id, out var state))
                return;

            if (fadeTime > 0f)
            {
                // Do not add a duplicate fade-out entry for the same player.
                if (_fadeOutTargets.Contains(state.Player))
                    return;

                var effectiveFader = fader ?? Faders.Linear;
                var fadeState = RentFadeState();
                fadeState.Setup(state.Player, effectiveFader, state.Player.VolumeFade, 0f, fadeTime.Value, true);
                state.Player.IsFading = true;
                _fadeOutTargets.Add(state.Player);
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
            // Process active fade states (swap-remove for O(1) removal).
            for (var i = 0; i < _fadeStates.Count; i++)
            {
                var fade = _fadeStates[i];
                var finished = fade.Elapsed(deltaTime);
                if (finished)
                {
                    ((AudioClipPlayer)fade.Fadeable).IsFading = false;
                    if (fade.IsStopTarget)
                        _fadeOutTargets.Remove(fade.Fadeable);
                    _fadeStates[i] = _fadeStates[_fadeStates.Count - 1];
                    _fadeStates.RemoveAt(_fadeStates.Count - 1);
                    _fadePool.Push(fade);
                    i--;
                }
            }

            // Iterate _playbacks.Values directly; collect removal keys in _removeKeyBuffer.
            _removeKeyBuffer.Clear();
            foreach (var playback in _playbacks.Values)
            {
                if (playback.Player == null)
                    continue;

                playback.Player.ManualUpdate(deltaTime);

                if (!playback.Player.IsPlaying && !playback.Player.IsPaused && !playback.Player.IsFading)
                {
                    _playerProvider.Return(playback.Player);
                    _removeKeyBuffer.Add(playback.Id);
                }
            }

            for (var i = 0; i < _removeKeyBuffer.Count; i++)
                _playbacks.Remove(_removeKeyBuffer[i]);

            // Process one-shot states (swap-remove for O(1) removal).
            for (var i = 0; i < _oneShotStates.Count; i++)
            {
                var state = _oneShotStates[i];
                if (state.Player == null)
                    continue;

                state.Player.ManualUpdate(deltaTime);

                if (!state.Player.IsPlaying && !state.Player.IsPaused)
                {
                    _oneShotProvider.Return(state.Player);
                    _oneShotStates[i] = _oneShotStates[_oneShotStates.Count - 1];
                    _oneShotStates.RemoveAt(_oneShotStates.Count - 1);
                    i--;
                }
            }
        }

        /// <summary>
        ///     Returns information about all currently registered CueSheets.
        /// </summary>
        public List<CueSheetInfo> GetCueSheetInfos()
        {
            var list = new List<CueSheetInfo>();
            GetCueSheetInfos(list);
            return list;
        }

        /// <summary>
        ///     Fills the provided list with information about all currently registered CueSheets.
        ///     The list is cleared before filling.
        /// </summary>
        /// <param name="result">The list to fill with CueSheet information.</param>
        public void GetCueSheetInfos(List<CueSheetInfo> result)
        {
            result.Clear();
            if (result.Capacity < _cueSheets.Count)
                result.Capacity = _cueSheets.Count;
            foreach (var kv in _cueSheets)
                result.Add(new CueSheetInfo(new CueSheetHandle(kv.Key), kv.Value.Asset.cueSheet.name));
        }

        /// <summary>
        ///     Returns information about all Cues in the specified CueSheet.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <returns>Cue information list, or an empty collection if the handle is invalid.</returns>
        public List<CueInfo> GetCueInfos(CueSheetHandle sheetHandle)
        {
            var list = new List<CueInfo>();
            GetCueInfos(sheetHandle, list);
            return list;
        }

        /// <summary>
        ///     Fills the provided list with information about all Cues in the specified CueSheet.
        ///     The list is cleared before filling.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="result">The list to fill with Cue information.</param>
        public void GetCueInfos(CueSheetHandle sheetHandle, List<CueInfo> result)
        {
            result.Clear();
            if (!sheetHandle.IsValid || !_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cueList = registration.Asset.cueSheet.cueList;
            if (result.Capacity < cueList.Count)
                result.Capacity = cueList.Count;
            for (var i = 0; i < cueList.Count; i++)
                result.Add(new CueInfo(cueList[i].name, cueList[i].categoryId));
        }

        /// <summary>
        ///     Returns information about all Tracks in the specified Cue.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue.</param>
        /// <returns>Track information list, or an empty collection if the handle or cue name is invalid.</returns>
        public List<TrackInfo> GetTrackInfos(CueSheetHandle sheetHandle, string cueName)
        {
            var list = new List<TrackInfo>();
            GetTrackInfos(sheetHandle, cueName, list);
            return list;
        }

        /// <summary>
        ///     Fills the provided list with information about all Tracks in the specified Cue.
        ///     The list is cleared before filling.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue.</param>
        /// <param name="result">The list to fill with Track information.</param>
        public void GetTrackInfos(CueSheetHandle sheetHandle, string cueName, List<TrackInfo> result)
        {
            result.Clear();
            if (!sheetHandle.IsValid || !_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cue = registration.FindCue(cueName);
            if (cue == null)
                return;

            var trackList = cue.trackList;
            if (result.Capacity < trackList.Count)
                result.Capacity = trackList.Count;
            for (var i = 0; i < trackList.Count; i++)
                result.Add(new TrackInfo(trackList[i].name, trackList[i].audioClip, trackList[i].isLoop,
                    trackList[i].priority));
        }

        /// <summary>
        ///     Gets the AudioMixerGroup assigned to the specified category.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>The AudioMixerGroup, or null if not found.</returns>
        public AudioMixerGroup? GetAudioMixerGroup(int categoryId)
        {
            if (_categories.TryGetValue(categoryId, out var category))
                return category.audioMixerGroup;

            return null;
        }

        /// <summary>
        ///     Returns the current master volume of this conductor.
        /// </summary>
        /// <returns>Master volume in the range [0, 1].</returns>
        public float GetMasterVolume()
        {
            return _masterVolume;
        }

        /// <summary>
        ///     Sets the master volume for all active playbacks (Managed and OneShot) under this conductor.
        ///     The value is clamped to [0, 1].
        /// </summary>
        /// <param name="volume">Target master volume.</param>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = ValueRangeConst.Volume.Clamp(volume);
            foreach (var playback in _playbacks.Values)
                playback.Player?.SetMasterVolume(_masterVolume);
            foreach (var state in _oneShotStates)
                state.Player?.SetMasterVolume(_masterVolume);
        }

        /// <summary>
        ///     Stops all active Managed and OneShot playbacks under this conductor.
        ///     When <paramref name="fadeTime" /> is greater than zero, Managed playbacks fade out instead of stopping immediately.
        ///     OneShot playbacks are always stopped immediately.
        /// </summary>
        /// <param name="fadeTime">Fade-out duration in seconds for Managed playbacks. When null or zero, the stop is immediate.</param>
        /// <param name="fader">Custom fader curve for Managed fade-out. When null, <see cref="Faders.Linear" /> is used.</param>
        public void StopAll(float? fadeTime = null, IFader? fader = null)
        {
            if (fadeTime > 0f)
                StopAllPlaybacksWithFade(fadeTime.Value, fader);
            else
                StopAllPlaybacksImmediate();

            StopAllOneShots();
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

            var cue = registration.FindCue(cueName);
            if (cue == null)
                return;

            var cueState = registration.GetOrCreateCueState(sheetHandle.Id, cue);
            var track = cueState.NextTrack();

            if (track == null || track.audioClip == null)
                return;

            if (!CanPlay(sheetHandle.Id, cue, track))
                return;

            var player = _oneShotProvider.Rent();
            _categories.TryGetValue(cue.categoryId, out var category);
            var cueSheet = registration.Asset.cueSheet;
            var volume = Calculator.CalcVolume(cueSheet, cue, track);
            var pitch = Calculator.CalcPitch(cueSheet, cue, track);
            player.Setup(category?.audioMixerGroup, track.audioClip, cue.categoryId, volume, pitch, false,
                track.startSample, track.loopStartSample, track.endSample);
            player.Play();
            player.SetMasterVolume(_masterVolume);
            _oneShotStates.Add(new OneShotState(++_playStateCounter, sheetHandle.Id, cue, player, track.priority));
        }

        private bool CanPlay(uint cueSheetId, Cue cue, Track track)
        {
            if (_playbacks.Count == 0 && _oneShotStates.Count == 0)
                return true;

            // Gather throttle settings per scope.
            var cueThrottleType = cue.throttleType;
            var cueThrottleLimit = cue.throttleLimit;

            ThrottleType sheetThrottleType = default;
            var sheetThrottleLimit = 0;
            if (_cueSheets.TryGetValue(cueSheetId, out var reg))
            {
                sheetThrottleType = reg.Asset.cueSheet.throttleType;
                sheetThrottleLimit = reg.Asset.cueSheet.throttleLimit;
            }

            ThrottleType catThrottleType = default;
            var catThrottleLimit = 0;
            if (_categories.TryGetValue(cue.categoryId, out var cat))
            {
                catThrottleType = cat.throttleType;
                catThrottleLimit = cat.throttleLimit;
            }

            var globalThrottleType = _settings.throttleType;
            var globalThrottleLimit = _settings.throttleLimit;

            if (cueThrottleLimit <= 0 && sheetThrottleLimit <= 0 && catThrottleLimit <= 0 &&
                globalThrottleLimit <= 0)
                return true;

            // Single pass: count playing states and track oldest per scope at once.
            int cueCount = 0, sheetCount = 0, catCount = 0, globalCount = 0;
            int cueMin = int.MaxValue, sheetMin = int.MaxValue, catMin = int.MaxValue, globalMin = int.MaxValue;
            PlaybackState? cueOldestManaged = null, sheetOldestManaged = null,
                catOldestManaged = null, globalOldestManaged = null;
            OneShotState? cueOldestOneShot = null, sheetOldestOneShot = null,
                catOldestOneShot = null, globalOldestOneShot = null;

            foreach (var p in _playbacks.Values)
                AccumulateAllScopes(p, cueSheetId, cue, cue.categoryId,
                    ref cueCount, ref cueMin, ref cueOldestManaged,
                    ref sheetCount, ref sheetMin, ref sheetOldestManaged,
                    ref catCount, ref catMin, ref catOldestManaged,
                    ref globalCount, ref globalMin, ref globalOldestManaged);

            foreach (var s in _oneShotStates)
                AccumulateAllScopes(s, cueSheetId, cue, cue.categoryId,
                    ref cueCount, ref cueMin, ref cueOldestOneShot,
                    ref sheetCount, ref sheetMin, ref sheetOldestOneShot,
                    ref catCount, ref catMin, ref catOldestOneShot,
                    ref globalCount, ref globalMin, ref globalOldestOneShot);

            // Check each scope.
            if (!ApplyThrottle(cueThrottleType, cueThrottleLimit,
                    cueCount, cueMin, track.priority, cueOldestManaged, cueOldestOneShot))
                return false;

            if (!ApplyThrottle(sheetThrottleType, sheetThrottleLimit,
                    sheetCount, sheetMin, track.priority, sheetOldestManaged, sheetOldestOneShot))
                return false;

            if (!ApplyThrottle(catThrottleType, catThrottleLimit,
                    catCount, catMin, track.priority, catOldestManaged, catOldestOneShot))
                return false;

            if (!ApplyThrottle(globalThrottleType, globalThrottleLimit,
                    globalCount, globalMin, track.priority, globalOldestManaged, globalOldestOneShot))
                return false;

            return true;
        }

        private bool ApplyThrottle(ThrottleType throttleType, int throttleLimit,
            int playNum, int minPriority, int trackPriority,
            PlaybackState? oldestManaged, OneShotState? oldestOneShot)
        {
            if (throttleLimit <= 0 || playNum < throttleLimit)
                return true;

            if (minPriority > trackPriority)
                return false;

            if (minPriority < trackPriority)
                throttleType = ThrottleType.PriorityOrder;

            switch (throttleType)
            {
                case ThrottleType.PriorityOrder:
                    StopOldestCandidate(oldestManaged, oldestOneShot);
                    return true;
                case ThrottleType.FirstComeFirstServed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StopAllPlaybacksImmediate()
        {
            foreach (var playback in _playbacks.Values)
                StopPlayback(playback);
            _playbacks.Clear();
        }

        private void StopAllPlaybacksWithFade(float fadeTime, IFader? fader)
        {
            _stopAllKeyBuffer.Clear();
            foreach (var id in _playbacks.Keys)
                _stopAllKeyBuffer.Add(id);
            for (var i = 0; i < _stopAllKeyBuffer.Count; i++)
                Stop(new PlaybackHandle(_stopAllKeyBuffer[i]), fadeTime, fader);
        }

        private void StopAllOneShots()
        {
            for (var i = _oneShotStates.Count - 1; i >= 0; i--)
            {
                var state = _oneShotStates[i];
                if (state.Player != null)
                {
                    state.Player.Stop();
                    _oneShotProvider.Return(state.Player);
                }
            }

            _oneShotStates.Clear();
        }

        private void StopPlayback(PlaybackState playback)
        {
            if (playback.Player == null)
                return;

            playback.Player.Stop();
            _playerProvider.Return(playback.Player);
        }

        private FadeState RentFadeState()
        {
            return _fadePool.Count > 0 ? _fadePool.Pop() : new FadeState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AccumulateAllScopes(in PlaybackState p, uint targetCueSheetId, Cue targetCue,
            int targetCategoryId,
            ref int cueCount, ref int cueMin, ref PlaybackState? cueOldest,
            ref int sheetCount, ref int sheetMin, ref PlaybackState? sheetOldest,
            ref int catCount, ref int catMin, ref PlaybackState? catOldest,
            ref int globalCount, ref int globalMin, ref PlaybackState? globalOldest)
        {
            if (p.Player == null || !p.Player.IsPlaying)
                return;

            globalCount++;
            if (p.Priority < globalMin || (p.Priority == globalMin && (!globalOldest.HasValue || p.Id < globalOldest.Value.Id)))
            {
                globalMin = p.Priority;
                globalOldest = p;
            }

            if (p.CueSheetId == targetCueSheetId)
            {
                sheetCount++;
                if (p.Priority < sheetMin || (p.Priority == sheetMin && (!sheetOldest.HasValue || p.Id < sheetOldest.Value.Id)))
                {
                    sheetMin = p.Priority;
                    sheetOldest = p;
                }
            }

            if (p.Cue == targetCue)
            {
                cueCount++;
                if (p.Priority < cueMin || (p.Priority == cueMin && (!cueOldest.HasValue || p.Id < cueOldest.Value.Id)))
                {
                    cueMin = p.Priority;
                    cueOldest = p;
                }
            }

            if (p.Cue.categoryId == targetCategoryId)
            {
                catCount++;
                if (p.Priority < catMin || (p.Priority == catMin && (!catOldest.HasValue || p.Id < catOldest.Value.Id)))
                {
                    catMin = p.Priority;
                    catOldest = p;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void AccumulateAllScopes(in OneShotState s, uint targetCueSheetId, Cue targetCue,
            int targetCategoryId,
            ref int cueCount, ref int cueMin, ref OneShotState? cueOldest,
            ref int sheetCount, ref int sheetMin, ref OneShotState? sheetOldest,
            ref int catCount, ref int catMin, ref OneShotState? catOldest,
            ref int globalCount, ref int globalMin, ref OneShotState? globalOldest)
        {
            if (s.Player == null || !s.Player.IsPlaying)
                return;

            globalCount++;
            if (s.Priority < globalMin || (s.Priority == globalMin && (!globalOldest.HasValue || s.Id < globalOldest.Value.Id)))
            {
                globalMin = s.Priority;
                globalOldest = s;
            }

            if (s.CueSheetId == targetCueSheetId)
            {
                sheetCount++;
                if (s.Priority < sheetMin || (s.Priority == sheetMin && (!sheetOldest.HasValue || s.Id < sheetOldest.Value.Id)))
                {
                    sheetMin = s.Priority;
                    sheetOldest = s;
                }
            }

            if (s.Cue == targetCue)
            {
                cueCount++;
                if (s.Priority < cueMin || (s.Priority == cueMin && (!cueOldest.HasValue || s.Id < cueOldest.Value.Id)))
                {
                    cueMin = s.Priority;
                    cueOldest = s;
                }
            }

            if (s.Cue.categoryId == targetCategoryId)
            {
                catCount++;
                if (s.Priority < catMin || (s.Priority == catMin && (!catOldest.HasValue || s.Id < catOldest.Value.Id)))
                {
                    catMin = s.Priority;
                    catOldest = s;
                }
            }
        }

        private void StopOldestCandidate(PlaybackState? oldestManaged, OneShotState? oldestOneShot)
        {
            // Compare Managed and OneShot by their insertion-order Id to find the globally oldest.
            if (oldestManaged.HasValue &&
                (!oldestOneShot.HasValue || oldestManaged.Value.Id <= oldestOneShot.Value.Id))
            {
                StopPlayback(oldestManaged.Value);
                _playbacks.Remove(oldestManaged.Value.Id);
            }
            else if (oldestOneShot.HasValue)
            {
                oldestOneShot.Value.Player.Stop();
                _oneShotProvider.Return(oldestOneShot.Value.Player);
                RemoveOneShotById(oldestOneShot.Value.Id);
            }
        }

        private void RemoveOneShotById(uint id)
        {
            for (var i = 0; i < _oneShotStates.Count; i++)
                if (_oneShotStates[i].Id == id)
                {
                    _oneShotStates[i] = _oneShotStates[_oneShotStates.Count - 1];
                    _oneShotStates.RemoveAt(_oneShotStates.Count - 1);
                    return;
                }
        }

        private sealed class CueSheetRegistration
        {
            private readonly Dictionary<string, Cue> _cueNameLookup;
            private readonly Dictionary<Cue, CueState> _cueStateCache = new();

            internal CueSheetRegistration(CueSheetAsset asset)
            {
                Asset = asset;
                var cueList = asset.cueSheet.cueList;
                _cueNameLookup = new Dictionary<string, Cue>(cueList.Count);
                for (var i = 0; i < cueList.Count; i++)
                    _cueNameLookup[cueList[i].name] = cueList[i];
            }

            internal CueSheetAsset Asset { get; }

            internal Cue? FindCue(string cueName)
            {
                _cueNameLookup.TryGetValue(cueName, out var cue);
                return cue;
            }

            internal CueState GetOrCreateCueState(uint cueSheetId, Cue cue)
            {
                if (!_cueStateCache.TryGetValue(cue, out var state))
                {
                    state = new CueState(cueSheetId, cue);
                    _cueStateCache[cue] = state;
                }

                return state;
            }
        }

        private readonly struct PlaybackState
        {
            internal PlaybackState(uint id, uint cueSheetId, Cue cue, AudioClipPlayer player, int priority)
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

        private readonly struct OneShotState
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

    }
}
