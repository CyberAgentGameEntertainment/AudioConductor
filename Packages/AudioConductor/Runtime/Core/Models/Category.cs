// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Runtime.Core.Enums;
using UnityEngine.Audio;

namespace AudioConductor.Runtime.Core.Models
{
    /// <summary>
    ///     Audio Category.
    /// </summary>
    [Serializable]
    public sealed class Category
    {
        /// <summary>
        ///     Category id.
        /// </summary>
        public int id;

        /// <summary>
        ///     Category name.
        /// </summary>
        public string name = null!;

        /// <summary>
        ///     <see cref="Enums.ThrottleType" />.
        /// </summary>
        public ThrottleType throttleType;

        /// <summary>
        ///     Limit of concurrent play.
        /// </summary>
        public int throttleLimit;

        /// <summary>
        ///     Output AudioMixerGroup.
        /// </summary>
        public AudioMixerGroup? audioMixerGroup;
    }
}
