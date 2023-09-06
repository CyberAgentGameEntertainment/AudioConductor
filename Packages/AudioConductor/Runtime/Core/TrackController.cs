// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Runtime.Core
{
    internal sealed class TrackController : ITrackControllerInternal
    {
        public TrackController(uint manageNumber,
                               AudioClipPlayer player,
                               uint cueSheetManageNumber,
                               uint cueManageNumber)
        {
            ManageNumber = manageNumber;
            Player = player;
            CueSheetManageNumber = cueSheetManageNumber;
            CueManageNumber = cueManageNumber;
            Priority = 0;
        }

        internal AudioClipPlayer Player { get; private set; }

        public uint ManageNumber { get; }
        public uint CueSheetManageNumber { get; }
        public uint CueManageNumber { get; }
        public int Priority { get; private set; }
        public Track Track { get; private set; }

        IAudioClipPlayer ITrackControllerInternal.Player => Player;

        /// <inheritdoc />
        public int ClipSamples => Player == null ? 0 : Player.ClipSamples;

        /// <inheritdoc />
        public void Play()
        {
            if (Player == null)
                return;

            Player.Play();
        }

        /// <inheritdoc />
        public void Stop(bool isFade)
        {
            if (Player == null)
                return;

            AudioConductorInternal.Instance.StopController(this, isFade);
        }

        /// <inheritdoc />
        public void Restart()
        {
            if (Player == null)
                return;

            Player.Restart();
        }

        /// <inheritdoc />
        public void Pause()
        {
            if (Player == null)
                return;

            Player.Pause();
        }

        /// <inheritdoc />
        public void Resume()
        {
            if (Player == null)
                return;

            Player.Resume();
        }

        /// <inheritdoc />
        public TrackState GetState()
        {
            if (Player == null)
                return TrackState.Stopped;

            if (Player.IsPaused)
                return TrackState.Paused;

            return Player.IsPlaying ? TrackState.Playing : TrackState.Stopped;
        }

        /// <inheritdoc />
        public int GetCategoryId() => Player == null ? 0 : Player.CategoryId;

        /// <inheritdoc />
        public float GetActualVolume() => Player == null ? 0f : Player.GetActualVolume();

        /// <inheritdoc />
        public float GetVolume() => Player == null ? 0f : Player.GetVolume();

        /// <inheritdoc />
        public void SetVolume(float volume)
        {
            if (Player == null)
                return;

            Player.SetVolume(volume);
        }

        /// <inheritdoc />
        public float GetActualPitch() => Player == null ? 0f : Player.GetActualPitch();

        /// <inheritdoc />
        public float GetPitch() => Player == null ? 0f : Player.GetPitch();

        /// <inheritdoc />
        public void SetPitch(float pitch)
        {
            if (Player == null)
                return;

            Player.SetPitch(pitch);
        }

        /// <inheritdoc />
        public void AddStopAction(Action onStop)
        {
            if (Player == null)
                return;

            Player.AddStopAction(onStop);
        }

        /// <inheritdoc />
        public void AddEndAction(Action onEnd)
        {
            if (Player == null)
                return;

            Player.AddEndAction(onEnd);
        }

        /// <inheritdoc />
        public int GetCurrentSample() => Player == null ? 0 : Player.GetCurrentSample();

        /// <inheritdoc />
        public void ReleasePlayer()
        {
            Player = null;
        }

        public void Setup(Category category, Track track, float volume, float pitch, bool isLoop)
        {
            Priority = track.priority;
            Player.Setup(category?.audioMixerGroup, track.audioClip, category?.id ?? -1, volume, pitch, isLoop,
                         track.startSample, track.loopStartSample, track.endSample);

            Track = track;
        }
    }
}
