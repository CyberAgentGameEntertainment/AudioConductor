// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Runtime.CompilerServices;
using AudioConductor.Core.Enums;
using static AudioConductor.Core.Conductor;

namespace AudioConductor.Core
{
    internal struct ThrottleScopeState
    {
        internal int Count { get; private set; }
        private int _min;
        internal Playback? Oldest { get; private set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            // Incoming priority strictly higher than playing minimum: force eviction
            // regardless of ThrottleType. New ThrottleType members must not change this rule.
            if (_min < incomingPriority)
                return TryEvictOldest(out eviction);
            return type switch
            {
                ThrottleType.PriorityOrder => TryEvictOldest(out eviction),
                ThrottleType.FirstComeFirstServed => false,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };
        }

        private readonly bool TryEvictOldest(out Playback? eviction)
        {
            eviction = Oldest;
            return Oldest.HasValue;
        }
    }
}
