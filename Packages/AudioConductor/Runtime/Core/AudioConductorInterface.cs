// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Models;
using UnityEngine.Audio;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     AudioConductor user interface.
    /// </summary>
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class AudioConductorInterface
    {
        /// <summary>
        ///     Setup AudioConductor.
        /// </summary>
        /// <param name="settings">A runtime settings instance</param>
        /// <param name="callback">Callback action called when a cue-sheet becomes unused.</param>
        public static void Setup(AudioConductorSettings settings, Action<CueSheetAsset> callback = null)
        {
            if (AudioConductorInternal.Instance.AudioConductorSettings == settings)
                return;
            AudioConductorInternal.Instance.Setup(settings, callback);
        }

        internal static void ForceSetup(AudioConductorSettings settings, Action<CueSheetAsset> callback = null)
        {
            AudioConductorInternal.Instance.Setup(settings, callback);
        }

        /// <summary>
        ///     Create a controller for the specified cue.
        /// </summary>
        /// <param name="sheet">The cue-sheet asset.</param>
        /// <param name="cueIndex">The index value of the cue.</param>
        /// <returns>The created controller.</returns>
        public static ICueController CreateController(CueSheetAsset sheet, int cueIndex)
        {
            var controller = new CueController();
            controller.Setup(sheet, cueIndex);

            return controller;
        }

        /// <summary>
        ///     Create a controller for the specified cue.
        /// </summary>
        /// <param name="sheet">The cue-sheet asset.</param>
        /// <param name="cueName">The name of the cue.</param>
        /// <returns>The created controller.</returns>
        public static ICueController CreateController(CueSheetAsset sheet, string cueName)
        {
            var controller = new CueController();
            controller.Setup(sheet, cueName);

            return controller;
        }

        /// <summary>
        ///     Stop the track audio.
        /// </summary>
        /// <param name="controller">The track </param>
        /// <param name="isFade">True if fade-out.</param>
        public static void StopTrack(ITrackController controller, bool isFade)
        {
            AudioConductorInternal.Instance.StopController(controller, isFade);
        }

        /// <summary>
        ///     Rents an unmanaged AudioClipPlayer instance.
        /// </summary>
        /// <returns>The rented player.</returns>
        public static IAudioClipPlayer RentUnmanagedPlayer()
        {
            return AudioConductorInternal.Instance.RentPlayer(true);
        }

        /// <summary>
        ///     Returns an unmanaged AudioClipPlayer instance.
        /// </summary>
        /// <param name="player">The rented player.</param>
        public static void ReturnUnmanagedPlayer(IAudioClipPlayer player)
        {
            AudioConductorInternal.Instance.ReturnPlayer(player as AudioClipPlayer);
        }

        /// <summary>
        ///     Check cue-sheet is in use.
        /// </summary>
        /// <param name="sheet"></param>
        /// <returns>True if the cue-sheet is in use.</returns>
        public static bool IsCueSheetUsed(CueSheetAsset sheet)
        {
            return AudioConductorInternal.Instance.IsCueSheetUsed(sheet);
        }

        /// <summary>
        ///     Stop all audio.
        /// </summary>
        /// <param name="isFade">True if fade-out.</param>
        public static void StopAll(bool isFade)
        {
            AudioConductorInternal.Instance.StopAll(isFade);
        }

        /// <summary>
        ///     Gets the AudioMixerGroup assigned to the specified category.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>The AudioMixerGroup, or null if not found.</returns>
        public static AudioMixerGroup GetAudioMixerGroup(int categoryId)
        {
            return AudioConductorInternal.Instance.GetAudioMixerGroup(categoryId);
        }

        /// <summary>
        ///     Gets the current master volume.
        /// </summary>
        /// <returns>The master volume (0.0 to 1.0).</returns>
        public static float GetMasterVolume()
        {
            return AudioConductorInternal.Instance.GetMasterVolume();
        }

        /// <summary>
        ///     Sets the master volume applied to all audio.
        /// </summary>
        /// <param name="volume">The master volume (0.0 to 1.0).</param>
        public static void SetMasterVolume(float volume)
        {
            AudioConductorInternal.Instance.SetMasterVolume(volume);
        }
    }
}
