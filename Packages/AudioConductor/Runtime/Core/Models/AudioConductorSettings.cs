// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
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
        ///     Master volume scale applied to all audio. (Value range 0.00 to 1.00)
        /// </summary>
        public float masterVolume = 1f;

        /// <summary>
        ///     Number of managed AudioClipPlayers to pre-create on construction.
        /// </summary>
        public int managedPoolCapacity;

        /// <summary>
        ///     Number of one-shot AudioClipPlayers to pre-create on construction.
        /// </summary>
        public int oneShotPoolCapacity;

        /// <summary>
        ///     When true, pooled AudioClipPlayer GameObjects are deactivated while idle.
        ///     Reduces the overhead of active GameObjects at the cost of SetActive calls on rent/return.
        /// </summary>
        public bool deactivatePooledObjects;

        /// <summary>
        ///     List of <see cref="Category" />.
        /// </summary>
        public List<Category> categoryList = new();
    }
}
