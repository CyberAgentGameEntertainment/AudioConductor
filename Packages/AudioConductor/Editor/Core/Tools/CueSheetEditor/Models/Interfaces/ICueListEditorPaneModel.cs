// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ICueListEditorPaneModel
    {
        public ICueListModel CueListModel { get; }

        IObservableProperty<bool> ObservableInspectorUnCollapsed { get; }

        IReadOnlyCollection<int> VisibleColumns { get; }

        string SearchString { get; }
    }
}
