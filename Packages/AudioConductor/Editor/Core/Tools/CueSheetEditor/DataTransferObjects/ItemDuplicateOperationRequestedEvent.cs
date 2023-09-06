// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal struct ItemDuplicateOperationRequestedEvent
    {
        public readonly int insertIndex;
        public readonly CueListItem parent;
        public readonly CueListItem target;

        public ItemDuplicateOperationRequestedEvent(int insertIndex, CueListItem parent, CueListItem target)
        {
            this.insertIndex = insertIndex;
            this.parent = parent;
            this.target = target;
        }
    }
}
