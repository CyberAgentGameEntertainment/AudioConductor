// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Core
{
    /// <summary>
    ///     Read-only snapshot of a Cue within a CueSheet.
    /// </summary>
    public readonly struct CueInfo
    {
        /// <summary>
        ///     Cue name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The category ID assigned to this cue.
        /// </summary>
        public int CategoryId { get; }

        /// <summary>
        ///     Stable integer ID for runtime playback. 0 means unassigned.
        /// </summary>
        public int CueId { get; }

        internal CueInfo(string name, int categoryId, int cueId)
        {
            Name = name;
            CategoryId = categoryId;
            CueId = cueId;
        }
    }
}
