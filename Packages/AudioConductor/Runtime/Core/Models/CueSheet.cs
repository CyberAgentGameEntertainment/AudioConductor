// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

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
        /// <summary>
        ///     Cue-sheet name.
        /// </summary>
        public string name = null!;

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
#if UNITY_EDITOR
        [SerializeField] private string id = IdentifierFactory.Create();

        public string Id => id;
#endif
    }
}
