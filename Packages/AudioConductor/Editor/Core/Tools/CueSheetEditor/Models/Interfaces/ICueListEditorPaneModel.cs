// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ICueListEditorPaneModel
    {
        ICueListModel CueListModel { get; }

        IObservableProperty<bool> ObservableInspectorUnCollapsed { get; }

        IReadOnlyCollection<int> VisibleColumns { get; }

        string SearchString { get; }
    }
}
