// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal interface ICueSheetEditorView : IDisposable
    {
        IObservable<int> TabSelectedAsObservable { get; }
        void SelectTab(int tabIndex);
        void Setup();
    }
}
