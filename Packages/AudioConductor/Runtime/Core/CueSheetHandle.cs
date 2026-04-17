// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Identifies a registered CueSheet within an <see cref="Conductor" /> instance.
    /// </summary>
    public readonly struct CueSheetHandle : IEquatable<CueSheetHandle>
    {
        internal readonly uint Id;

        internal CueSheetHandle(uint id)
        {
            Id = id;
        }

        /// <summary>
        ///     Returns true if this handle refers to a registered CueSheet (Id != 0).
        /// </summary>
        public bool IsValid => Id != 0;

        /// <inheritdoc />
        public bool Equals(CueSheetHandle other)
        {
            return Id == other.Id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is CueSheetHandle other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(CueSheetHandle left, CueSheetHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CueSheetHandle left, CueSheetHandle right)
        {
            return !left.Equals(right);
        }
    }
}
