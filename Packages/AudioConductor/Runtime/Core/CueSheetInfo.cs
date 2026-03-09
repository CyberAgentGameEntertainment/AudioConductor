// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Core
{
    /// <summary>
    ///     Read-only snapshot of a registered CueSheet.
    /// </summary>
    public readonly struct CueSheetInfo
    {
        /// <summary>
        ///     The handle that identifies this CueSheet within the conductor.
        /// </summary>
        public CueSheetHandle Handle { get; }

        /// <summary>
        ///     CueSheet name.
        /// </summary>
        public string Name { get; }

        internal CueSheetInfo(CueSheetHandle handle, string name)
        {
            Handle = handle;
            Name = name;
        }
    }
}
