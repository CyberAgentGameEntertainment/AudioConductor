// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    /// <summary>
    ///     The unit of play.
    /// </summary>
    [Serializable]
    public sealed class Track
    {
#if UNITY_EDITOR
        [SerializeField]
        private string id = IdentifierFactory.Create();
        public string Id => id;
        
        public string colorId;
#endif
        
        /// <summary>
        ///     Track name.
        /// </summary>
        public string name;

        /// <summary>
        ///     Audio clip.
        /// </summary>
        public AudioClip audioClip;

        /// <summary>
        ///     Volume of the audio.
        /// </summary>
        public float volume = 1;

        /// <summary>
        ///     Random range of the volume.
        /// </summary>
        public float volumeRange;

        /// <summary>
        ///     Pitch of the audio.
        /// </summary>
        public float pitch = 1;

        /// <summary>
        ///     Random range of the pitch.
        /// </summary>
        public float pitchRange;

        /// <summary>
        ///     True if the pitch is negative number.
        /// </summary>
        public bool pitchInvert;

        /// <summary>
        ///     Play start position.
        /// </summary>
        public int startSample;

        /// <summary>
        ///     Play end position.
        /// </summary>
        public int endSample;

        /// <summary>
        ///     Loop start position.
        /// </summary>
        public int loopStartSample;

        /// <summary>
        ///     True if the audio loops.
        /// </summary>
        public bool isLoop;

        /// <summary>
        ///     Weight for random play.
        /// </summary>
        public int randomWeight;

        /// <summary>
        ///     Priority for concurrent play control.
        /// </summary>
        public int priority;

        /// <summary>
        ///     Fade-in/fade-out time.
        /// </summary>
        public float fadeTime;
    }
}
