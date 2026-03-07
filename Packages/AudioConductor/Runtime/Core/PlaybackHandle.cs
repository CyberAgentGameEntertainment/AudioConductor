// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Identifies an active managed playback within an <see cref="AudioConductor" /> instance.
    /// </summary>
    public readonly struct PlaybackHandle : IEquatable<PlaybackHandle>
    {
        internal readonly uint Id;

        internal PlaybackHandle(uint id)
        {
            Id = id;
        }

        /// <summary>
        ///     Returns true if this handle refers to a registered playback (Id != 0).
        /// </summary>
        public bool IsValid => Id != 0;

        /// <inheritdoc />
        public bool Equals(PlaybackHandle other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return obj is PlaybackHandle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(PlaybackHandle left, PlaybackHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PlaybackHandle left, PlaybackHandle right)
        {
            return !left.Equals(right);
        }
    }
}
