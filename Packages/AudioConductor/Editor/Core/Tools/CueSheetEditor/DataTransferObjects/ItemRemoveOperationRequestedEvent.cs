// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal readonly struct ItemRemoveOperationRequestedEvent
    {
        public readonly int index;
        public readonly CueListItem target;

        public ItemRemoveOperationRequestedEvent(CueListItem target)
        {
            index = target.parent.children.IndexOf(target);
            this.target = target;
        }
    }
}
