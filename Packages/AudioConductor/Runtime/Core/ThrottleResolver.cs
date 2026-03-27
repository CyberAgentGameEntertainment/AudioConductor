// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Runtime.CompilerServices;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using static AudioConductor.Core.Conductor;

namespace AudioConductor.Core
{
    internal static class ThrottleResolver
    {
        internal static bool ResolveThrottle(ThrottleType throttleType, int throttleLimit,
            int playNum, int minPriority, int trackPriority,
            Playback? oldest,
            out Playback? eviction)
        {
            eviction = null;

            if (throttleLimit <= 0 || playNum < throttleLimit)
                return true;

            if (minPriority > trackPriority)
                return false;

            if (minPriority < trackPriority)
                throttleType = ThrottleType.PriorityOrder;

            switch (throttleType)
            {
                case ThrottleType.PriorityOrder:
                    if (!oldest.HasValue)
                        return false;
                    eviction = oldest.Value;
                    return true;
                case ThrottleType.FirstComeFirstServed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AdjustCountsAfterEviction(Playback? eviction,
            uint targetCueSheetId, Cue targetCue, int targetCategoryId,
            ref int cueCount, ref int sheetCount, ref int catCount, ref int globalCount)
        {
            if (!eviction.HasValue)
                return;

            var e = eviction.Value;
            globalCount--;
            if (e.CueSheetId == targetCueSheetId)
                sheetCount--;
            if (e.Cue == targetCue)
                cueCount--;
            if (e.Cue.categoryId == targetCategoryId)
                catCount--;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AccumulateAllScopes(in Playback p, uint targetCueSheetId, Cue targetCue,
            int targetCategoryId,
            ref int cueCount, ref int cueMin, ref Playback? cueOldest,
            ref int sheetCount, ref int sheetMin, ref Playback? sheetOldest,
            ref int catCount, ref int catMin, ref Playback? catOldest,
            ref int globalCount, ref int globalMin, ref Playback? globalOldest)
        {
            if (p.Player.State == PlayerState.Stopped)
                return;

            globalCount++;
            if (IsMinOrOldest(in p, globalMin, globalOldest))
            {
                globalMin = p.Priority;
                globalOldest = p;
            }

            if (p.CueSheetId == targetCueSheetId)
            {
                sheetCount++;
                if (IsMinOrOldest(in p, sheetMin, sheetOldest))
                {
                    sheetMin = p.Priority;
                    sheetOldest = p;
                }
            }

            if (p.Cue == targetCue)
            {
                cueCount++;
                if (IsMinOrOldest(in p, cueMin, cueOldest))
                {
                    cueMin = p.Priority;
                    cueOldest = p;
                }
            }

            if (p.Cue.categoryId == targetCategoryId)
            {
                catCount++;
                if (IsMinOrOldest(in p, catMin, catOldest))
                {
                    catMin = p.Priority;
                    catOldest = p;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsMinOrOldest(in Playback p, int currentMin, Playback? currentOldest)
        {
            return p.Priority < currentMin ||
                   p.Priority == currentMin && (!currentOldest.HasValue || p.Id < currentOldest.Value.Id);
        }
    }
}
