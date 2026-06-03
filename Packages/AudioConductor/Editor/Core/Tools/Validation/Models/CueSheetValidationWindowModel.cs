// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.Validation.Models
{
    internal sealed class CueSheetValidationWindowModel : ICueSheetValidationWindowModel, IDisposable
    {
        private readonly CueSheetAsset[] _assets;
        private readonly IValidationSettingsPreferences _preferences;

        private readonly ObservableProperty<IReadOnlyList<ValidationResultRow>> _resultRows =
            new(Array.Empty<ValidationResultRow>());

        private readonly Subject<ValidationResultRow> _rowSelected = new();
        private readonly ObservableProperty<AudioConductorSettings?> _selectedSettings = new(null);
        private readonly IAudioConductorSettingsProvider _settingsProvider;
        private readonly ICueSheetValidator _validator;

        internal CueSheetValidationWindowModel(
            IEnumerable<CueSheetAsset> assets,
            ValidationScope scope,
            IAudioConductorSettingsProvider settingsProvider,
            IValidationSettingsPreferences preferences,
            ICueSheetValidator validator)
        {
            _assets = assets.ToArray();
            Scope = scope;
            _settingsProvider = settingsProvider;
            _preferences = preferences;
            _validator = validator;

            AllSettings = settingsProvider.AllSettings;

            var savedGuid = preferences.LoadSelectedGuid();
            var initial = settingsProvider.GetByGuid(savedGuid);
            if (initial == null && AllSettings.Length == 1)
                initial = AllSettings[0];
            _selectedSettings.SetValueAndNotNotify(initial);
        }

        public IReadOnlyObservableProperty<AudioConductorSettings?> SelectedSettings => _selectedSettings;
        public IReadOnlyObservableProperty<IReadOnlyList<ValidationResultRow>> ResultRows => _resultRows;
        public AudioConductorSettings[] AllSettings { get; }
        public IEnumerable<CueSheetAsset> Assets => _assets;
        public ValidationScope Scope { get; }
        public IObservable<ValidationResultRow> RowSelected => _rowSelected;

        public void SelectSettings(AudioConductorSettings? settings)
        {
            _selectedSettings.SetValueAndNotify(settings);
            var guid = settings != null
                ? _settingsProvider.GetGuid(settings)
                : string.Empty;
            _preferences.SaveSelectedGuid(guid);
        }

        public void RunValidation()
        {
            var rows = new List<ValidationResultRow>();
            var settings = _selectedSettings.Value;

            if (settings == null)
                rows.Add(CreateSettingsWarningRow());

            foreach (var asset in _assets)
            {
                if (asset == null)
                    continue;

                var issues = _validator.Validate(asset, settings);

                rows.Add(new ValidationResultRow(null, asset, asset.name, false));

                foreach (var issue in issues)
                    rows.Add(new ValidationResultRow(issue, asset, issue.Message, true));
            }

            _resultRows.SetValueAndNotify(rows);
        }

        public void SelectRow(ValidationResultRow row)
        {
            if (!_rowSelected.DidDispose)
                _rowSelected.OnNext(row);
        }

        public void Dispose()
        {
            _rowSelected.Dispose();
            _resultRows.Dispose();
            _selectedSettings.Dispose();
        }

        private ValidationResultRow CreateSettingsWarningRow()
        {
            var (code, message) = AllSettings.Length == 0
                ? ("Validation.SettingsNotFound",
                    "AudioConductorSettings was not found. Category validation was skipped.")
                : ("Validation.SettingsNotSelected",
                    "AudioConductorSettings is not selected. Category validation was skipped.");
            return new ValidationResultRow(
                new ValidationIssue(ValidationSeverity.Warning, code, message, string.Empty, null),
                null,
                message,
                true);
        }
    }
}
