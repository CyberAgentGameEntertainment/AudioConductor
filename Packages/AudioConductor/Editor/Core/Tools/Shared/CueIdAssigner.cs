// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    /// <summary>
    ///     Assigns sequential cue IDs within a cue sheet.
    ///     Valid cue IDs start from 1; 0 is reserved as an unassigned sentinel value.
    /// </summary>
    internal static class CueIdAssigner
    {
        /// <summary>
        ///     Returns true if the cue list contains duplicate cue IDs (including all-zero case).
        /// </summary>
        public static bool HasDuplicateCueIds(IReadOnlyList<Cue> cueList)
        {
            if (cueList == null || cueList.Count == 0)
                return false;

            var seen = new HashSet<int>();
            foreach (var cue in cueList)
                // cueId = 0 is always treated as duplicate (unassigned)
                if (cue.cueId == 0 || !seen.Add(cue.cueId))
                    return true;

            return false;
        }

        /// <summary>
        ///     Assigns sequential cue IDs to cues that have cueId = 0.
        ///     IDs are assigned starting from max(existing cueId) + 1.
        ///     Cues with a non-zero cueId are left unchanged; callers that need full
        ///     deduplication (including non-zero duplicates) should reset all cueIds to 0
        ///     before calling this method.
        ///     If the cue list is empty, no assignment is performed.
        /// </summary>
        public static void AssignMissingCueIds(IList<Cue> cueList)
        {
            if (cueList == null || cueList.Count == 0)
                return;

            var nextId = GetNextCueIdCore(cueList);
            foreach (var cue in cueList)
                if (cue.cueId == 0)
                    cue.cueId = nextId++;
        }

        /// <summary>
        ///     Returns the next available cue ID for the given cue list.
        ///     Returns 1 if the list is empty or all IDs are 0.
        /// </summary>
        public static int GetNextCueId(IReadOnlyList<Cue> cueList)
        {
            return GetNextCueIdCore(cueList);
        }

        private static int GetNextCueIdCore(IEnumerable<Cue> cueList)
        {
            if (cueList == null)
                return 1;

            var max = 0;
            foreach (var cue in cueList)
                if (cue.cueId > max)
                    max = cue.cueId;

            return max == 0 ? 1 : max + 1;
        }
    }
}
