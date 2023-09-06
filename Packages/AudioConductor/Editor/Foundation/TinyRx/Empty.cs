// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;

namespace AudioConductor.Editor.Foundation.TinyRx
{
    [Serializable]
    public struct Empty : IEquatable<Empty>
    {
        public static Empty Default { get; } = new();

        public bool Equals(Empty other) => true;

        public static bool operator ==(Empty first, Empty second) => true;

        public static bool operator !=(Empty first, Empty second) => false;

        public override bool Equals(object obj) => obj is Empty;

        public override int GetHashCode() => 0;
    }
}
