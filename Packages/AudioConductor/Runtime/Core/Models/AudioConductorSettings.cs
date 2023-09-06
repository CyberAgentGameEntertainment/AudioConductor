// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Enums;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    /// <summary>
    ///     Contains runtime settings for the AudioConductor.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioConductorSettings),
                     menuName = "Audio Conductor/" + "Settings",
                     order = 0)]
    public sealed class AudioConductorSettings : ScriptableObject
    {
        /// <summary>
        ///     <see cref="Enums.ThrottleType" />
        /// </summary>
        public ThrottleType throttleType;

        /// <summary>
        ///     Limit of concurrent play.
        /// </summary>
        public int throttleLimit;

        /// <summary>
        ///     List of <see cref="Category" />.
        /// </summary>
        public List<Category> categoryList = new();
    }
}
