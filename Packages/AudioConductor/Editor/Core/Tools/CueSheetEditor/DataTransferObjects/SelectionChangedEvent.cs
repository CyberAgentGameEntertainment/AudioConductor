// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal readonly struct SelectionChangedEvent
    {
        public readonly CueListItem[] items;

        public SelectionChangedEvent(CueListItem[] items)
        {
            this.items = items;
        }
    }
}
