// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    /// <summary>
    ///     An object that groups cues.
    /// </summary>
    [Serializable]
    public sealed class CueSheet
    {
#if UNITY_EDITOR
        [SerializeField]
        private string id = IdentifierFactory.Create();
        public string Id => id;
#endif
        
        /// <summary>
        ///     Cue-sheet name.
        /// </summary>
        public string name;

        /// <summary>
        ///     <see cref="Enums.ThrottleType" />
        /// </summary>
        public ThrottleType throttleType;

        /// <summary>
        ///     Limit of concurrent play.
        /// </summary>
        public int throttleLimit;

        /// <summary>
        ///     Volume of the audio.
        /// </summary>
        public float volume = 1f;

        /// <summary>
        ///     Pitch of the audio.
        /// </summary>
        public float pitch = 1f;

        /// <summary>
        ///     True if the pitch is negative number.
        /// </summary>
        public bool pitchInvert;

        /// <summary>
        ///     List of <see cref="Cue" />.
        /// </summary>
        public List<Cue> cueList = new();
    }
}
