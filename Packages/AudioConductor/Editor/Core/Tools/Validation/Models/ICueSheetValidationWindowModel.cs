// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.Validation.Models
{
    internal interface ICueSheetValidationWindowModel
    {
        IReadOnlyObservableProperty<AudioConductorSettings?> SelectedSettings { get; }
        IReadOnlyObservableProperty<IReadOnlyList<ValidationResultRow>> ResultRows { get; }
        AudioConductorSettings[] AllSettings { get; }
        IEnumerable<CueSheetAsset> Assets { get; }
        ValidationScope Scope { get; }
        IObservable<ValidationResultRow> RowSelected { get; }
        void SelectSettings(AudioConductorSettings? settings);
        void RunValidation();
        void SelectRow(ValidationResultRow row);
    }
}
