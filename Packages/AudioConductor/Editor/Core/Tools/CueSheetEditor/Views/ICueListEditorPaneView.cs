// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal interface ICueListEditorPaneView : IDisposable
    {
        IObservable<bool> InspectorToggleChangedAsObservable { get; }
        IObservable<bool> VolumeToggleChangedAsObservable { get; }
        IObservable<bool> PlayInfoToggleChangedAsObservable { get; }
        IObservable<bool> ThrottleToggleChangedAsObservable { get; }
        IObservable<bool> MemoToggleChangedAsObservable { get; }
        IObservable<string> SearchFieldChangedAsObservable { get; }
        void Setup();
        void Open();
        void Close();
        void SetButtonState(IReadOnlyCollection<int> visibleColumns);
        void SetSearchString(string searchString);
        void ClearSearch();
        void SetInspector(bool unCollapsed);
    }
}
