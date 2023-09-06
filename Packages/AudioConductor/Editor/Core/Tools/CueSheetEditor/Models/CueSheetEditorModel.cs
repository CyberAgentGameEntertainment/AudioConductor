// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueSheetEditorModel : ICueSheetEditorModel
    {
        private readonly OtherOperationPaneModel _otherOperationPaneModel;

        public CueSheetEditorModel([NotNull] CueSheet cueSheet,
                                   [NotNull] AutoIncrementHistory history,
                                   [NotNull] IAssetSaveService assetSaveService,
                                   IObservableProperty<CueSheetEditorPresenter.Pane> paneState,
                                   IObservableProperty<bool> inspectorUnCollapsed,
                                   CueListTreeView.State cueListTreeViewState
        )
        {
            CueSheetParameterPaneModel =
                new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            CueListEditorPaneModel
                = new CueListEditorPaneModel(cueSheet, history, assetSaveService, inspectorUnCollapsed,
                                             cueListTreeViewState);

            OtherOperationPaneModel = new OtherOperationPaneModel(cueSheet, history, assetSaveService);

            ObservablePaneState = paneState;
        }

        public ICueSheetParameterPaneModel CueSheetParameterPaneModel { get; }

        public ICueListEditorPaneModel CueListEditorPaneModel { get; }

        public IOtherOperationPaneModel OtherOperationPaneModel { get; }

        public IObservableProperty<CueSheetEditorPresenter.Pane> ObservablePaneState { get; }
    }
}
