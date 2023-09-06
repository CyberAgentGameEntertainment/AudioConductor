// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal readonly struct ColumnValueChangedEvent
    {
        public readonly CueListTreeView.ColumnType columnType;
        public readonly object newValue;
        public readonly int itemId;

        public ColumnValueChangedEvent(CueListTreeView.ColumnType columnType, object newValue, int itemId)
        {
            this.columnType = columnType;
            this.newValue = newValue;
            this.itemId = itemId;
        }
    }
}
