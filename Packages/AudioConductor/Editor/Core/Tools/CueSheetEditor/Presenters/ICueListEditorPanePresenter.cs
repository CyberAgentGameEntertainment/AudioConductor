// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal interface ICueListEditorPanePresenter : ICueSheetEditorPanePresenter
    {
        void FocusCue(string cueEditorId);
    }
}
