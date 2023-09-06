// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Cue control user interface.
    /// </summary>
    public interface ICueController : IDisposable
    {
        /// <summary>
        ///     True if it is playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        ///     Play the track depending on the <see cref="Enums.CuePlayType" />.
        /// </summary>
        /// <param name="isForceLoop">True if you want to force the track to loop.</param>
        /// <returns>
        ///     <see cref="ITrackController" />
        /// </returns>
        ITrackController Play(bool isForceLoop = false);

        /// <summary>
        ///     Play the specified track.
        /// </summary>
        /// <param name="index">The Index value of the track.</param>
        /// <param name="isForceLoop">True if you want to force the track to loop.</param>
        /// <returns>
        ///     <see cref="ITrackController" />
        /// </returns>
        ITrackController Play(int index, bool isForceLoop = false);

        /// <summary>
        ///     Play the specified track
        /// </summary>
        /// <param name="name">The track name.</param>
        /// <param name="isForceLoop">True if you want to force the track to loop.</param>
        /// <returns>
        ///     <see cref="ITrackController" />
        /// </returns>
        ITrackController Play(string name, bool isForceLoop = false);

        /// <summary>
        ///     Pauses playing the track.
        /// </summary>
        void Pause();

        /// <summary>
        ///     Resumes the paused track.
        /// </summary>
        void Resume();

        /// <summary>
        ///     Stops playing the track.
        /// </summary>
        /// <param name="isFade"></param>
        void Stop(bool isFade);

        /// <summary>
        ///     Gets the <see cref="Models.Category" /> id.
        /// </summary>
        int GetCategoryId();

        /// <summary>
        ///     Sets the volume of the playing track. (Value range 0.00 to 1.00)
        /// </summary>
        void SetVolume(float volume);

        /// <summary>
        ///     Sets the pitch of the playing track. (Value range -3.00 to 3.00 except 0)
        /// </summary>
        void SetPitch(float pitch);
    }
}
