// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core.Shared;

namespace AudioConductor.Runtime.Core
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
    }
}
