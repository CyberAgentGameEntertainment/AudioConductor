// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal readonly struct TrackAddOperationRequestedEvent
    {
        public readonly ItemCue parent;

        public TrackAddOperationRequestedEvent(ItemCue parent)
        {
            this.parent = parent;
        }
    }
}
