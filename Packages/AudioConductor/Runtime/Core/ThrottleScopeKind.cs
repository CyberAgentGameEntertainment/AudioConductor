// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;

namespace AudioConductor.Core
{
    internal enum ThrottleScopeKind
    {
        // NOTE: Enum order IS the Phase 1 resolve order in Conductor.CanPlay.
        Cue = 0,
        Sheet = 1,
        Category = 2,
        Global = 3,

        // Count is a sentinel (number of scopes), not a valid scope value.
        Count = 4
    }

    internal readonly struct ThrottleSetting // unmanaged: stackalloc-able
    {
        internal const int Unlimited = 0; // limit <= Unlimited means no throttling
        internal readonly ThrottleType Type;
        internal readonly int Limit;

        internal ThrottleSetting(ThrottleType type, int limit)
        {
            Type = type;
            Limit = limit;
        }

        internal bool IsLimited => Limit > Unlimited;
    }
}
