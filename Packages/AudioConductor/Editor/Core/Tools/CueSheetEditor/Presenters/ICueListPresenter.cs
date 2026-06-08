// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal interface ICueListPresenter : IDisposable
    {
        event Action<IInspectorModel> OnSelectionItemChanged;
        void Setup();
        void FocusItemById(int itemId);
        void OnVolumeToggleChanged(bool active);
        void OnPlayInfoToggleChanged(bool active);
        void OnThrottleToggleChanged(bool active);
        void OnMemoToggleChanged(bool active);
        void OnSearchFieldChanged(string text);
    }
}
