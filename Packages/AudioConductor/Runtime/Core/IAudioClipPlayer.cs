// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     User interface for AudioClip play.
    /// </summary>
    public interface IAudioClipPlayer
    {
        /// <summary>
        ///     The length of the audio clip in samples.
        /// </summary>
        int ClipSamples { get; }

        /// <summary>
        ///     Get the category id.
        /// </summary>
        int CategoryId { get; }

        /// <summary>
        ///     True if playing.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        ///     True if paused.
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        ///     Setup to play AudioClip.
        /// </summary>
        /// <param name="audioMixerGroup">The output AudioMixerGroup.</param>
        /// <param name="clip">The AudioClip to play.</param>
        /// <param name="categoryId">The category id.</param>
        /// <param name="volume">The volume. (Value range 0.00 to 1.00)</param>
        /// <param name="pitch">The pitch. (Value range -3.00 to 3.00 except 0)</param>
        /// <param name="isLoop">True if you want to the AudioClip to loop.</param>
        /// <param name="startSample">The play start position.</param>
        /// <param name="loopStartSample">The loop start position.</param>
        /// <param name="endSample">The play end position.</param>
        public void Setup(AudioMixerGroup audioMixerGroup,
                          AudioClip clip,
                          int categoryId,
                          float volume,
                          float pitch,
                          bool isLoop,
                          int startSample,
                          int loopStartSample,
                          int endSample);

        /// <summary>
        ///     Play the AudioClip.
        /// </summary>
        void Play();

        /// <summary>
        ///     Play from start..
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
        ///     Stop.
        /// </summary>
        void Stop();

        /// <summary>
        ///     Get the actual volume. (Value range 0.00 to 1.00)
        /// </summary>
        /// <returns></returns>
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
        ///     Get the actual pitch. (Value range -3.00 to 3.00 except 0)
        /// </summary>
        float GetActualPitch();

        /// <summary>
        ///     Get the pitch. (Value range -3.00 to 3.00 except 0)
        /// </summary>
        /// <remarks>This value can set on a per play and does not edit the value set in the editor.</remarks>
        float GetPitch();

        /// <summary>
        ///     Set the pitch. (Value range -3.00 to 3.00 except 0)
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

        /// <summary>
        ///     Set the current position.
        /// </summary>
        void SetCurrentSample(int sample);
    }
}
