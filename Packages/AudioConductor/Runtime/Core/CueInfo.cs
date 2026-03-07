// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Runtime.Core
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

        internal CueInfo(string name, int categoryId)
        {
            Name = name;
            CategoryId = categoryId;
        }
    }
}
