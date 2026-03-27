// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Shared;

namespace AudioConductor.Core
{
    public sealed partial class Conductor
    {
        /// <summary>
        ///     Sets the volume of the playback identified by the handle.
        /// </summary>
        /// <param name="handle">The playback handle.</param>
        /// <param name="volume">The volume value.</param>
        public void SetVolume(PlaybackHandle handle, float volume)
        {
            if (!handle.IsValid)
                return;

            if (!_managedPlaybacks.TryGetValue(handle.Id, out var state) || state.Player == null)
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

            if (!_managedPlaybacks.TryGetValue(handle.Id, out var state) || state.Player == null)
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

            if (!_managedPlaybacks.TryGetValue(handle.Id, out var state) || state.Player == null)
                return false;

            return state.Player.State == PlayerState.Playing || state.Player.FadeState != FadeState.None;
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
            foreach (var playback in _managedPlaybacks.Values)
                playback.Player?.SetMasterVolume(_masterVolume);
            foreach (var state in _oneShotPlaybacks)
                state.Player?.SetMasterVolume(_masterVolume);
        }

        /// <summary>
        ///     Returns the current volume for the specified category.
        ///     Returns 1.0 if no volume has been set for the category.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>Category volume in the range [0, 1].</returns>
        public float GetCategoryVolume(int categoryId)
        {
            return _categoryVolumes.GetValueOrDefault(categoryId, 1f);
        }

        /// <summary>
        ///     Sets the volume for the specified category and applies it to all active playbacks of that category.
        ///     The value is clamped to [0, 1].
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <param name="volume">Target category volume.</param>
        public void SetCategoryVolume(int categoryId, float volume)
        {
            var clamped = ValueRangeConst.Volume.Clamp(volume);
            _categoryVolumes[categoryId] = clamped;
            foreach (var playback in _managedPlaybacks.Values)
                if (playback.Player?.CategoryId == categoryId)
                    playback.Player.SetCategoryVolume(clamped);
            foreach (var state in _oneShotPlaybacks)
                if (state.Player?.CategoryId == categoryId)
                    state.Player.SetCategoryVolume(clamped);
        }
    }
}
