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
            if (p.Player == null || p.Player.State == PlayerState.Stopped)
                return;

            globalCount++;
            if (p.Priority < globalMin ||
                p.Priority == globalMin && (!globalOldest.HasValue || p.Id < globalOldest.Value.Id))
            {
                globalMin = p.Priority;
                globalOldest = p;
            }

            if (p.CueSheetId == targetCueSheetId)
            {
                sheetCount++;
                if (p.Priority < sheetMin ||
                    p.Priority == sheetMin && (!sheetOldest.HasValue || p.Id < sheetOldest.Value.Id))
                {
                    sheetMin = p.Priority;
                    sheetOldest = p;
                }
            }

            if (p.Cue == targetCue)
            {
                cueCount++;
                if (p.Priority < cueMin || p.Priority == cueMin && (!cueOldest.HasValue || p.Id < cueOldest.Value.Id))
                {
                    cueMin = p.Priority;
                    cueOldest = p;
                }
            }

            if (p.Cue.categoryId == targetCategoryId)
            {
                catCount++;
                if (p.Priority < catMin || p.Priority == catMin && (!catOldest.HasValue || p.Id < catOldest.Value.Id))
                {
                    catMin = p.Priority;
                    catOldest = p;
                }
            }
        }
    }
}
