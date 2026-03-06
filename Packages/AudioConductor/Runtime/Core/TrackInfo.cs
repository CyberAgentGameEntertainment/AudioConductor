// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Read-only snapshot of a Track within a Cue.
    /// </summary>
    public readonly struct TrackInfo
    {
        /// <summary>
        ///     Track name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The AudioClip assigned to this track.
        /// </summary>
        public AudioClip? AudioClip { get; }

        /// <summary>
        ///     True if this track loops.
        /// </summary>
        public bool IsLoop { get; }

        /// <summary>
        ///     Priority for concurrent play control.
        /// </summary>
        public int Priority { get; }

        internal TrackInfo(string name, AudioClip? audioClip, bool isLoop, int priority)
        {
            Name = name;
            AudioClip = audioClip;
            IsLoop = isLoop;
            Priority = priority;
        }
    }
}
