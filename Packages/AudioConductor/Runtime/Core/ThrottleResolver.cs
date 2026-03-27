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
            Playback? oldestManaged, Playback? oldestOneShot,
            out EvictionResult eviction)
        {
            eviction = default;

            if (throttleLimit <= 0 || playNum < throttleLimit)
                return true;

            if (minPriority > trackPriority)
                return false;

            if (minPriority < trackPriority)
                throttleType = ThrottleType.PriorityOrder;

            switch (throttleType)
            {
                case ThrottleType.PriorityOrder:
                    eviction = SelectEvictionCandidate(oldestManaged, oldestOneShot);
                    return eviction.Id != 0;
                case ThrottleType.FirstComeFirstServed:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AdjustCountsAfterEviction(in EvictionResult eviction,
            uint targetCueSheetId, Cue targetCue, int targetCategoryId,
            ref int cueCount, ref int sheetCount, ref int catCount, ref int globalCount)
        {
            if (eviction.Id == 0)
                return;

            globalCount--;
            if (eviction.CueSheetId == targetCueSheetId)
                sheetCount--;
            if (eviction.Cue == targetCue)
                cueCount--;
            if (eviction.Cue.categoryId == targetCategoryId)
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

        internal static EvictionResult SelectEvictionCandidate(Playback? oldestManaged,
            Playback? oldestOneShot)
        {
            // Compare Managed and OneShot by their insertion-order Id to find the globally oldest.
            if (oldestManaged.HasValue &&
                (!oldestOneShot.HasValue || oldestManaged.Value.Id <= oldestOneShot.Value.Id))
            {
                var m = oldestManaged.Value;
                return new EvictionResult(m.Id, m.CueSheetId, m.Cue, true);
            }

            if (oldestOneShot.HasValue)
            {
                var s = oldestOneShot.Value;
                return new EvictionResult(s.Id, s.CueSheetId, s.Cue, false);
            }

            return default;
        }
    }
}
