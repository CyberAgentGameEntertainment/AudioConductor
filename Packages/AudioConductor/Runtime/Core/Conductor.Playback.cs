// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;
using UnityEngine;

namespace AudioConductor.Core
{
    public sealed partial class Conductor
    {
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
            if (!sheetHandle.IsValid)
                return default;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return default;

            var cue = registration.FindCue(cueName);
            if (cue == null)
                return default;

            return PlayCue(sheetHandle.Id, registration, cue, options);
        }

        /// <summary>
        ///     Plays a cue from the registered CueSheet identified by the handle using an integer cue ID.
        ///     Returns a <see cref="PlaybackHandle" /> for controlling the playback.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueId">The integer ID of the cue to play.</param>
        /// <param name="options">Optional playback settings. If null, CueSheet defaults are used.</param>
        /// <returns>A handle for controlling this playback instance.</returns>
        public PlaybackHandle Play(CueSheetHandle sheetHandle, int cueId, PlayOptions? options = null)
        {
            if (!sheetHandle.IsValid)
                return default;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return default;

            var cue = registration.FindCue(cueId);
            if (cue == null)
                return default;

            return PlayCue(sheetHandle.Id, registration, cue, options);
        }

        /// <summary>
        ///     Plays a cue as a fire-and-forget OneShot using an integer cue ID.
        ///     No handle is returned; the playback cannot be controlled after it starts.
        ///     The AudioClipPlayer is automatically returned to the OneShot pool when playback completes.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueId">The integer ID of the cue to play.</param>
        public void PlayOneShot(CueSheetHandle sheetHandle, int cueId)
        {
            if (!sheetHandle.IsValid)
                return;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cue = registration.FindCue(cueId);
            if (cue == null)
                return;

            PlayOneShotCue(sheetHandle.Id, registration, cue);
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
                if (_fadeManager.IsFadingOut(state.Player))
                    return;

                _fadeManager.StartFade(state.Player, fader ?? Faders.Linear, state.Player.VolumeFade, 0f,
                    fadeTime.Value);
                _fadeManager.MarkFadeOut(state.Player);
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

            PlayOneShotCue(sheetHandle.Id, registration, cue);
        }

        private PlaybackHandle PlayCue(uint cueSheetId, CueSheetRegistration registration, Cue cue,
            PlayOptions? options)
        {
            var hasTrackIndex = options?.TrackIndex.HasValue == true;
            var trackName = options?.TrackName;
            var selector = options?.Selector;

            if (hasTrackIndex && !string.IsNullOrEmpty(trackName))
                throw new ArgumentException("TrackIndex and TrackName are mutually exclusive.");

            if (hasTrackIndex && selector != null)
                throw new ArgumentException("TrackIndex and Selector are mutually exclusive.");

            if (!string.IsNullOrEmpty(trackName) && selector != null)
                throw new ArgumentException("TrackName and Selector are mutually exclusive.");

            var cueState = registration.GetOrCreateCueState(cueSheetId, cue);

            Track? track;
            if (hasTrackIndex)
                track = cueState.GetTrack(options!.Value.TrackIndex!.Value);
            else if (!string.IsNullOrEmpty(trackName))
                track = cueState.GetTrack(trackName!);
            else
                track = cueState.NextTrack(selector);

            if (track == null || track.audioClip == null)
                return default;

            if (!CanPlay(cueSheetId, cue, track))
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
            player.SetCategoryVolume(GetCategoryVolume(cue.categoryId));

            var id = _playStateCounter.Next();
            var state = new PlaybackState(id, cueSheetId, cue, player, track.priority);
            _playbacks[id] = state;

            if (options?.FadeTime > 0f)
            {
                player.SetVolumeFade(0f);
                _fadeManager.StartFade(player, options.Value.Fader ?? Faders.Linear, 0f, 1f,
                    options.Value.FadeTime.Value);
            }

            return new PlaybackHandle(id);
        }

        private void PlayOneShotCue(uint cueSheetId, CueSheetRegistration registration, Cue cue)
        {
            var cueState = registration.GetOrCreateCueState(cueSheetId, cue);
            var track = cueState.NextTrack();

            if (track == null || track.audioClip == null)
                return;

            if (!CanPlay(cueSheetId, cue, track))
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
            player.SetCategoryVolume(GetCategoryVolume(cue.categoryId));
            var oneShotId = _playStateCounter.Next();
            _oneShotStates.Add(new OneShotState(oneShotId, cueSheetId, cue, player, track.priority));
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

            var globalThrottleType = _throttleType;
            var globalThrottleLimit = _throttleLimit;

            if (cueThrottleLimit <= 0 && sheetThrottleLimit <= 0 && catThrottleLimit <= 0 &&
                globalThrottleLimit <= 0)
                return true;

            // Single pass: count playing states and track oldest per scope at once.
            int cueCount = 0, sheetCount = 0, catCount = 0, globalCount = 0;
            int cueMin = int.MaxValue, sheetMin = int.MaxValue, catMin = int.MaxValue, globalMin = int.MaxValue;
            PlaybackState? cueOldestManaged = null,
                sheetOldestManaged = null,
                catOldestManaged = null,
                globalOldestManaged = null;
            OneShotState? cueOldestOneShot = null,
                sheetOldestOneShot = null,
                catOldestOneShot = null,
                globalOldestOneShot = null;

            foreach (var p in _playbacks.Values)
                ThrottleResolver.AccumulateAllScopes(p, cueSheetId, cue, cue.categoryId,
                    ref cueCount, ref cueMin, ref cueOldestManaged,
                    ref sheetCount, ref sheetMin, ref sheetOldestManaged,
                    ref catCount, ref catMin, ref catOldestManaged,
                    ref globalCount, ref globalMin, ref globalOldestManaged);

            foreach (var s in _oneShotStates)
                ThrottleResolver.AccumulateAllScopes(s, cueSheetId, cue, cue.categoryId,
                    ref cueCount, ref cueMin, ref cueOldestOneShot,
                    ref sheetCount, ref sheetMin, ref sheetOldestOneShot,
                    ref catCount, ref catMin, ref catOldestOneShot,
                    ref globalCount, ref globalMin, ref globalOldestOneShot);

#if UNITY_EDITOR
            // Invariant: counts must not exceed their respective limits.
            // Violated only if throttle limits are mutated while players are active (unsupported).
            Debug.Assert(cueThrottleLimit <= 0 || cueCount <= cueThrottleLimit,
                "cue count exceeds throttle limit");
            Debug.Assert(sheetThrottleLimit <= 0 || sheetCount <= sheetThrottleLimit,
                "sheet count exceeds throttle limit");
            Debug.Assert(catThrottleLimit <= 0 || catCount <= catThrottleLimit,
                "category count exceeds throttle limit");
            Debug.Assert(globalThrottleLimit <= 0 || globalCount <= globalThrottleLimit,
                "global count exceeds throttle limit");
#endif

            // Phase 1: Resolve eviction candidates per scope without executing.
            // AdjustCountsAfterEviction updates local counts so subsequent scopes
            // see the effect of prior evictions. Actual stop is deferred to Phase 2
            // to ensure no side effects when a later scope rejects the play.
            if (!ThrottleResolver.ResolveThrottle(cueThrottleType, cueThrottleLimit,
                    cueCount, cueMin, track.priority, cueOldestManaged, cueOldestOneShot,
                    out var cueEviction))
                return false;
            ThrottleResolver.AdjustCountsAfterEviction(cueEviction, cueSheetId, cue, cue.categoryId,
                ref cueCount, ref sheetCount, ref catCount, ref globalCount);

            if (!ThrottleResolver.ResolveThrottle(sheetThrottleType, sheetThrottleLimit,
                    sheetCount, sheetMin, track.priority, sheetOldestManaged, sheetOldestOneShot,
                    out var sheetEviction))
                return false;
            ThrottleResolver.AdjustCountsAfterEviction(sheetEviction, cueSheetId, cue, cue.categoryId,
                ref cueCount, ref sheetCount, ref catCount, ref globalCount);

            if (!ThrottleResolver.ResolveThrottle(catThrottleType, catThrottleLimit,
                    catCount, catMin, track.priority, catOldestManaged, catOldestOneShot,
                    out var catEviction))
                return false;
            ThrottleResolver.AdjustCountsAfterEviction(catEviction, cueSheetId, cue, cue.categoryId,
                ref cueCount, ref sheetCount, ref catCount, ref globalCount);

            if (!ThrottleResolver.ResolveThrottle(globalThrottleType, globalThrottleLimit,
                    globalCount, globalMin, track.priority, globalOldestManaged, globalOldestOneShot,
                    out var globalEviction))
                return false;

            // Phase 2: All scopes passed — execute deferred evictions.
            ExecuteEviction(cueEviction);
            ExecuteEviction(sheetEviction);
            ExecuteEviction(catEviction);
            ExecuteEviction(globalEviction);

            return true;
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
                    _fadeManager.CancelFade(state.Player);
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

            _fadeManager.CancelFade(playback.Player);
            playback.Player.Stop();
            _playerProvider.Return(playback.Player);
        }

        private void ExecuteEviction(in EvictionResult eviction)
        {
            if (eviction.Id == 0)
                return;

            if (eviction.IsManaged)
            {
                if (_playbacks.TryGetValue(eviction.Id, out var pb))
                {
                    StopPlayback(pb);
                    _playbacks.Remove(eviction.Id);
                }
            }
            else
            {
                if (RemoveOneShotById(eviction.Id, out var player))
                {
                    _fadeManager.CancelFade(player);
                    player.Stop();
                    _oneShotProvider.Return(player);
                }
            }
        }

        private bool RemoveOneShotById(uint id, out IInternalPlayer player)
        {
            for (var i = 0; i < _oneShotStates.Count; i++)
                if (_oneShotStates[i].Id == id)
                {
                    player = _oneShotStates[i].Player;
                    _oneShotStates[i] = _oneShotStates[_oneShotStates.Count - 1];
                    _oneShotStates.RemoveAt(_oneShotStates.Count - 1);
                    return true;
                }

            player = null!;
            return false;
        }
    }
}
