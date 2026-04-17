// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ICueSheetEditorModel
    {
        ICueSheetParameterPaneModel CueSheetParameterPaneModel { get; }
        ICueListEditorPaneModel CueListEditorPaneModel { get; }
        IOtherOperationPaneModel OtherOperationPaneModel { get; }

        IObservableProperty<CueSheetEditorPresenter.Pane> ObservablePaneState { get; }
    }
}
