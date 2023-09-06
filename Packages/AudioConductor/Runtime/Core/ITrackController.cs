// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Enums;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Track control user interface.
    /// </summary>
    public interface ITrackController
    {
        /// <summary>
        ///     The length of the audio clip in samples.
        /// </summary>
        int ClipSamples { get; }

        /// <summary>
        ///     Play from start.
        /// </summary>
        void Restart();

        /// <summary>
        ///     Pause if playing.
        /// </summary>
        void Pause();

        /// <summary>
        ///     Resume if paused.
        /// </summary>
        void Resume();

        /// <summary>
        ///     Gets the <see cref="TrackState" />.
        /// </summary>
        TrackState GetState();

        /// <summary>
        ///     Get the <see cref="Models.Category" /> id.
        /// </summary>
        /// <returns></returns>
        int GetCategoryId();

        /// <summary>
        ///     Get the actual volume. (Value range 0.00 to 1.00)
        /// </summary>
        float GetActualVolume();

        /// <summary>
        ///     Get the volume. (Value range 0.00 to 1.00)
        /// </summary>
        /// <remarks>This value can set on a per play and does not edit the value set in the editor.</remarks>
        float GetVolume();

        /// <summary>
        ///     Set the volume. (Value range 0.00 to 1.00)
        /// </summary>
        /// <remarks>This value can set on a per play and does not edit the value set in the editor.</remarks>
        void SetVolume(float volume);

        /// <summary>
        ///     Get the actual pitch.  (Value range -3.00 to 3.00 except 0)
        /// </summary>
        float GetActualPitch();

        /// <summary>
        ///     Get the pitch.  (Value range -3.00 to 3.00 except 0)
        /// </summary>
        /// <remarks>This value can set on a per play and does not edit the value set in the editor.</remarks>
        float GetPitch();

        /// <summary>
        ///     Set the pitch.  (Value range -3.00 to 3.00 except 0)
        /// </summary>
        /// <remarks>This value can set on a per play and does not edit the value set in the editor.</remarks>
        void SetPitch(float pitch);

        /// <summary>
        ///     Register a callback on stop.
        /// </summary>
        void AddStopAction(Action onStop);

        /// <summary>
        ///     Register a callback on end. Not called if loop is enabled.
        /// </summary>
        void AddEndAction(Action onEnd);

        /// <summary>
        ///     Get the current position.
        /// </summary>
        int GetCurrentSample();
    }
}
