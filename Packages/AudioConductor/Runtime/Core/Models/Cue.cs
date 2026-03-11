// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Shared;
using UnityEngine;

namespace AudioConductor.Core.Models
{
    /// <summary>
    ///     An object that groups tracks.
    /// </summary>
    [Serializable]
    public sealed class Cue
    {
        /// <summary>
        ///     Cue name.
        /// </summary>
        public string name = null!;

        /// <summary>
        ///     <see cref="Category" /> id.
        /// </summary>
        public int categoryId;

        /// <summary>
        ///     <see cref="Enums.ThrottleType" />.
        /// </summary>
        public ThrottleType throttleType;

        /// <summary>
        ///     Limit of concurrent play.
        /// </summary>
        public int throttleLimit;

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
        ///     <see cref="CuePlayType" />.
        /// </summary>
        public CuePlayType playType;

        /// <summary>
        ///     Stable integer ID for runtime playback.
        ///     0 is reserved as an unassigned sentinel value; valid IDs start from 1.
        /// </summary>
        public int cueId;

        /// <summary>
        ///     List of <see cref="Track" />.
        /// </summary>
        public List<Track> trackList = new();
#if UNITY_EDITOR
        [SerializeField] private string id = IdentifierFactory.Create();

        public string Id => id;

        public string? colorId;
#endif
    }
}
