// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using UnityEngine;

namespace AudioConductor.Core.Models
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
