// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Core
{
    /// <summary>
    ///     A sequential counter that produces non-zero uint IDs,
    ///     skipping 0 on overflow.
    /// </summary>
    internal struct NonZeroSequence
    {
        private uint _value;

        internal NonZeroSequence(uint initialValue)
        {
            _value = initialValue;
        }

        public uint Next()
        {
            var id = ++_value;
            if (id == 0) id = ++_value;
            return id;
        }
    }
}
