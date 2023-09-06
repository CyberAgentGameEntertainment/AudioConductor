// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal readonly struct ItemMoveOperationRequestedEvent
    {
        public readonly int oldIndex;
        public readonly int newIndex;
        public readonly CueListItem oldParent;
        public readonly CueListItem newParent;
        public readonly CueListItem target;

        public ItemMoveOperationRequestedEvent(int oldIndex, int newIndex, CueListItem newParent, CueListItem target)
        {
            this.oldIndex = oldIndex;
            this.newIndex = newIndex;
            oldParent = (CueListItem)target.parent;
            this.newParent = newParent;
            this.target = target;
        }
    }
}
