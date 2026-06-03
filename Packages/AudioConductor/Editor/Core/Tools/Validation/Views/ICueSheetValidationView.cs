// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Validation.Models;
using AudioConductor.Editor.Foundation.TinyRx;

namespace AudioConductor.Editor.Core.Tools.Validation.Views
{
    internal interface ICueSheetValidationView : IDisposable
    {
        IObservable<ValidationResultRow> RowSelectedAsObservable { get; }
        IObservable<AudioConductorSettings?> SettingsChangedAsObservable { get; }
        IObservable<Empty> ValidateClickedAsObservable { get; }
        void Setup(AudioConductorSettings[] allSettings, bool showSettingsDropdown);
        void RenderResults(IReadOnlyList<ValidationResultRow> rows);
        void SetSelectedSettings(AudioConductorSettings? settings);
        void ShowEmpty();
    }
}
