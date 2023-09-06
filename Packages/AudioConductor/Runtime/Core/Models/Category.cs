// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

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
        public string name;

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
        public AudioMixerGroup audioMixerGroup;
    }
}
