// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal readonly struct MixedValue<T> : IEquatable<MixedValue<T>>
    {
        public MixedValue(T value, bool hasMultipleDifferentValues)
        {
            Value = value;
            HasMultipleDifferentValues = hasMultipleDifferentValues;
        }

        public T Value { get; }
        public bool HasMultipleDifferentValues { get; }

        public static bool operator ==(MixedValue<T> left, MixedValue<T> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MixedValue<T> left, MixedValue<T> right)
        {
            return !Equals(left, right);
        }

        public bool Equals(MixedValue<T> other)
        {
            var valueIsNull = ReferenceEquals(Value, null);
            var otherValueIsNull = ReferenceEquals(other.Value, null);

            if (valueIsNull && otherValueIsNull)
                return HasMultipleDifferentValues == other.HasMultipleDifferentValues;

            if (valueIsNull || otherValueIsNull)
                return false;

            return Value!.Equals(other.Value) && HasMultipleDifferentValues == other.HasMultipleDifferentValues;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return GetType() == other.GetType() && Equals((MixedValue<T>)other);
        }

        public override int GetHashCode()
        {
            return (Value?.GetHashCode() ?? 0) ^ HasMultipleDifferentValues.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Value}:{HasMultipleDifferentValues}";
        }
    }
}
