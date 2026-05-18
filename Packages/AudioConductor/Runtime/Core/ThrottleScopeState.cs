// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Enums;
using static AudioConductor.Core.Conductor;

namespace AudioConductor.Core
{
    internal struct ThrottleScopeState
    {
        internal int Count;
        internal int Min;
        internal Playback? Oldest;

        internal void Accumulate(in Playback p)
        {
            Count++;
            if (!Oldest.HasValue || p.Priority < Min || p.Priority == Min && p.Id < Oldest.Value.Id)
            {
                Min = p.Priority;
                Oldest = p;
            }
        }

        internal void Decrement()
        {
            Count--;
        }

        internal readonly bool Resolve(ThrottleType type, int limit, int incomingPriority,
            out Playback? eviction)
        {
            eviction = null;
            if (limit <= 0 || Count < limit)
                return true;
            if (Min > incomingPriority)
                return false;
            if (Min < incomingPriority)
                type = ThrottleType.PriorityOrder;
            switch (type)
            {
                case ThrottleType.PriorityOrder:
                    if (!Oldest.HasValue)
                        return false;
                    eviction = Oldest;
                    return true;
                case ThrottleType.FirstComeFirstServed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
