// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueListEditorPaneModel : ICueListEditorPaneModel
    {
        public CueListEditorPaneModel([NotNull] CueSheet cueSheet,
                                      [NotNull] AutoIncrementHistory history,
                                      [NotNull] IAssetSaveService assetSaveService,
                                      IObservableProperty<bool> inspectorUnCollapsed,
                                      CueListTreeView.State cueListTreeViewState)
        {
            CueListModel = new CueListModel(cueSheet, history, assetSaveService, cueListTreeViewState);
            ObservableInspectorUnCollapsed = inspectorUnCollapsed;
            VisibleColumns = cueListTreeViewState.MultiColumnHeaderState.visibleColumns;
            SearchString = cueListTreeViewState.searchString;
        }

        public ICueListModel CueListModel { get; }

        public IObservableProperty<bool> ObservableInspectorUnCollapsed { get; }

        public IReadOnlyCollection<int> VisibleColumns { get; }

        public string SearchString { get; }
    }
}
