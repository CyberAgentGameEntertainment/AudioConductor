// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal interface ICueSheetEditorPanePresenter : IDisposable
    {
        void Setup();
        void Open();
        void Close();
    }
}
