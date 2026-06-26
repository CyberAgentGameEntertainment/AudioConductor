// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal interface ICueListEditorPanePresenter : ICueSheetEditorPanePresenter
    {
        event Action? TrackClipChanged;

        void FocusCue(string cueEditorId);
    }
}
