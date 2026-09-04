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

            if (!_managedPlaybacks.TryGetValue(handle.Id, out var playback))
                return;

            if (fadeTime > 0f)
            {
                // Do not start a duplicate fade-out if one is already in progress or completed.
                if (playback.Player.FadeState == FadeState.FadingOut ||
                    playback.Player.FadeState == FadeState.FadingOutComplete)
                    return;

                playback.Player.ClearEndAction();
                _fadeManager.StartFade(playback.Player, fader ?? Faders.Linear, playback.Player.VolumeFade, 0f,
                    fadeTime.Value);
                return;
            }

            StopPlayback(playback);
            _managedPlaybacks.Remove(handle.Id);
        }

        /// <summary>
        ///     Pauses the playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle to pause.</param>
        public void Pause(PlaybackHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (!_managedPlaybacks.TryGetValue(handle.Id, out var playback))
                return;

            playback.Player.Pause();
        }

        /// <summary>
        ///     Resumes the paused playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle to resume.</param>
        public void Resume(PlaybackHandle handle)
        {
            if (!handle.IsValid)
                return;

            if (!_managedPlaybacks.TryGetValue(handle.Id, out var playback))
                return;

            playback.Player.Resume();
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

        /// <summary>
        ///     Plays a cue as a fire-and-forget OneShot with optional callbacks.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue to play.</param>
        /// <param name="options">Optional callbacks for stop and end events.</param>
        public void PlayOneShot(CueSheetHandle sheetHandle, string cueName, PlayOneShotOptions? options)
        {
            if (!sheetHandle.IsValid)
                return;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cue = registration.FindCue(cueName);
            if (cue == null)
                return;

            PlayOneShotCue(sheetHandle.Id, registration, cue, options);
        }

        /// <summary>
        ///     Plays a cue as a fire-and-forget OneShot with optional callbacks using an integer cue ID.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueId">The integer ID of the cue to play.</param>
        /// <param name="options">Optional callbacks for stop and end events.</param>
        public void PlayOneShot(CueSheetHandle sheetHandle, int cueId, PlayOneShotOptions? options)
        {
            if (!sheetHandle.IsValid)
                return;

            if (!_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cue = registration.FindCue(cueId);
            if (cue == null)
                return;

            PlayOneShotCue(sheetHandle.Id, registration, cue, options);
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
                track.startSample, track.loopStartSample, track.endSample, cueSheet.referenceSampleRate);
            player.Play();
            if (options?.OnStop is { } onStop) player.SetStopAction(onStop);
            if (options?.OnEnd is { } onEnd) player.SetEndAction(onEnd);
            player.SetMasterVolume(_masterVolume);
            player.SetCategoryVolume(GetCategoryVolume(cue.categoryId));

            var id = _playStateCounter.Next();
            var state = new ManagedPlayback(id, cueSheetId, cue, player, track.priority);
            _managedPlaybacks[id] = state;

            if (options?.FadeTime > 0f)
            {
                player.SetVolumeFade(0f);
                _fadeManager.StartFade(player, options.Value.Fader ?? Faders.Linear, 0f, 1f,
                    options.Value.FadeTime.Value);
            }

            return new PlaybackHandle(id);
        }

        private void PlayOneShotCue(uint cueSheetId, CueSheetRegistration registration, Cue cue,
            PlayOneShotOptions? options = null)
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
                track.startSample, track.loopStartSample, track.endSample, cueSheet.referenceSampleRate);
            player.Play();
            if (options?.OnStop is { } onStop) player.SetStopAction(onStop);
            if (options?.OnEnd is { } onEnd) player.SetEndAction(onEnd);
            player.SetMasterVolume(_masterVolume);
            player.SetCategoryVolume(GetCategoryVolume(cue.categoryId));
            var oneShotId = _playStateCounter.Next();
            _oneShotPlaybacks.Add(new OneShotPlayback(oneShotId, cueSheetId, cue, player, track.priority));
        }

        private bool CanPlay(uint cueSheetId, Cue cue, Track track)
        {
            if (_managedPlaybacks.Count == 0 && _oneShotPlaybacks.Count == 0)
                return true;

            // Gather throttle settings per scope.
            // This block is the ONLY place that knows where each scope's config lives.
            Span<ThrottleSetting> settings = stackalloc ThrottleSetting[(int)ThrottleScopeKind.Count];

            settings[(int)ThrottleScopeKind.Cue] = new ThrottleSetting(cue.throttleType, cue.throttleLimit);
            settings[(int)ThrottleScopeKind.Sheet] = _cueSheets.TryGetValue(cueSheetId, out var reg)
                ? new ThrottleSetting(reg.Asset.cueSheet.throttleType, reg.Asset.cueSheet.throttleLimit)
                : default;
            settings[(int)ThrottleScopeKind.Category] = _categories.TryGetValue(cue.categoryId, out var cat)
                ? new ThrottleSetting(cat.throttleType, cat.throttleLimit)
                : default;
            settings[(int)ThrottleScopeKind.Global] = new ThrottleSetting(_throttleType, _throttleLimit);

            // Early-out derived from the same data Phase 1 consumes — cannot drift when a scope is added.
            var anyLimited = false;
            foreach (var s in settings)
                anyLimited |= s.IsLimited;
            if (!anyLimited)
                return true;

            // Single pass: count playing states and track oldest per scope at once.
            var ctx = new ThrottleContext(cueSheetId, cue);

            foreach (var p in _managedPlaybacks.Values)
                ctx.Accumulate(p.Core);

            foreach (var s in _oneShotPlaybacks)
                ctx.Accumulate(s.Core);

#if UNITY_EDITOR
            // Invariant: counts must not exceed their respective limits.
            // Violated only if throttle limits are mutated while players are active (unsupported).
            for (var i = 0; i < (int)ThrottleScopeKind.Count; i++)
                Debug.Assert(!settings[i].IsLimited || ctx.Count((ThrottleScopeKind)i) <= settings[i].Limit,
                    "count exceeds throttle limit");
#endif

            // Phase 1: Resolve eviction candidates per scope without executing.
            // Enum order == resolve order. Resolve updates counts so subsequent scopes see
            // the effect of prior evictions; actual stop is deferred to Phase 2.
            for (var i = 0; i < (int)ThrottleScopeKind.Count; i++)
                if (!ctx.Resolve((ThrottleScopeKind)i, settings[i], track.priority))
                    return false;

            // Phase 2: All scopes passed — execute deferred evictions.
            for (var i = 0; i < (int)ThrottleScopeKind.Count; i++)
                ExecuteEviction(ctx.PendingEviction((ThrottleScopeKind)i));

            return true;
        }

        private void StopAllPlaybacksImmediate()
        {
            foreach (var playback in _managedPlaybacks.Values)
                StopPlayback(playback);
            _managedPlaybacks.Clear();
        }

        private void StopAllPlaybacksWithFade(float fadeTime, IFader? fader)
        {
            _stopAllKeyBuffer.Clear();
            foreach (var id in _managedPlaybacks.Keys)
                _stopAllKeyBuffer.Add(id);
            for (var i = 0; i < _stopAllKeyBuffer.Count; i++)
                Stop(new PlaybackHandle(_stopAllKeyBuffer[i]), fadeTime, fader);
        }

        private void StopAllOneShots()
        {
            for (var i = _oneShotPlaybacks.Count - 1; i >= 0; i--)
            {
                var playback = _oneShotPlaybacks[i];
                _fadeManager.CancelFade(playback.Player);
                playback.Player.Stop();
                _oneShotProvider.Return(playback.Player);
            }

            _oneShotPlaybacks.Clear();
        }

        private void StopPlayback(ManagedPlayback playback)
        {
            _fadeManager.CancelFade(playback.Player);
            playback.Player.Stop();
            _playerProvider.Return(playback.Player);
        }

        private void ExecuteEviction(Playback? eviction)
        {
            if (!eviction.HasValue)
                return;

            if (_managedPlaybacks.TryGetValue(eviction.Value.Id, out var pb))
            {
                StopPlayback(pb);
                _managedPlaybacks.Remove(eviction.Value.Id);
            }
            else if (RemoveOneShotById(eviction.Value.Id, out var player))
            {
                _fadeManager.CancelFade(player);
                player.Stop();
                _oneShotProvider.Return(player);
            }
        }

        private bool RemoveOneShotById(uint id, out AudioClipPlayer player)
        {
            for (var i = 0; i < _oneShotPlaybacks.Count; i++)
                if (_oneShotPlaybacks[i].Id == id)
                {
                    player = _oneShotPlaybacks[i].Player;
                    _oneShotPlaybacks[i] = _oneShotPlaybacks[^1];
                    _oneShotPlaybacks.RemoveAt(_oneShotPlaybacks.Count - 1);
                    return true;
                }

            player = null!;
            return false;
        }
    }
}
