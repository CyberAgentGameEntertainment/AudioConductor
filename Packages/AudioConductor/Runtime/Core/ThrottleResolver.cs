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
            ManagedPlayback? oldestManaged, OneShotPlayback? oldestOneShot,
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
        internal static void AccumulateAllScopes(in ManagedPlayback p, uint targetCueSheetId, Cue targetCue,
            int targetCategoryId,
            ref int cueCount, ref int cueMin, ref ManagedPlayback? cueOldest,
            ref int sheetCount, ref int sheetMin, ref ManagedPlayback? sheetOldest,
            ref int catCount, ref int catMin, ref ManagedPlayback? catOldest,
            ref int globalCount, ref int globalMin, ref ManagedPlayback? globalOldest)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AccumulateAllScopes(in OneShotPlayback s, uint targetCueSheetId, Cue targetCue,
            int targetCategoryId,
            ref int cueCount, ref int cueMin, ref OneShotPlayback? cueOldest,
            ref int sheetCount, ref int sheetMin, ref OneShotPlayback? sheetOldest,
            ref int catCount, ref int catMin, ref OneShotPlayback? catOldest,
            ref int globalCount, ref int globalMin, ref OneShotPlayback? globalOldest)
        {
            if (s.Player == null || s.Player.State == PlayerState.Stopped)
                return;

            globalCount++;
            if (s.Priority < globalMin ||
                s.Priority == globalMin && (!globalOldest.HasValue || s.Id < globalOldest.Value.Id))
            {
                globalMin = s.Priority;
                globalOldest = s;
            }

            if (s.CueSheetId == targetCueSheetId)
            {
                sheetCount++;
                if (s.Priority < sheetMin ||
                    s.Priority == sheetMin && (!sheetOldest.HasValue || s.Id < sheetOldest.Value.Id))
                {
                    sheetMin = s.Priority;
                    sheetOldest = s;
                }
            }

            if (s.Cue == targetCue)
            {
                cueCount++;
                if (s.Priority < cueMin || s.Priority == cueMin && (!cueOldest.HasValue || s.Id < cueOldest.Value.Id))
                {
                    cueMin = s.Priority;
                    cueOldest = s;
                }
            }

            if (s.Cue.categoryId == targetCategoryId)
            {
                catCount++;
                if (s.Priority < catMin || s.Priority == catMin && (!catOldest.HasValue || s.Id < catOldest.Value.Id))
                {
                    catMin = s.Priority;
                    catOldest = s;
                }
            }
        }

        internal static EvictionResult SelectEvictionCandidate(ManagedPlayback? oldestManaged,
            OneShotPlayback? oldestOneShot)
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
