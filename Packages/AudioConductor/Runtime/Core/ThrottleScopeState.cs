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
        internal int Count { get; private set; }
        private int _min;
        internal Playback? Oldest { get; private set; }

        internal void Accumulate(in Playback p)
        {
            Count++;
            if (!Oldest.HasValue || p.Priority < _min || p.Priority == _min && p.Id < Oldest.Value.Id)
            {
                _min = p.Priority;
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
            if (_min > incomingPriority)
                return false;
            if (_min < incomingPriority)
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
