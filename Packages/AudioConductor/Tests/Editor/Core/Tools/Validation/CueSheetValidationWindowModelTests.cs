// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Core.Tools.Validation.Models;
using AudioConductor.Editor.Foundation.TinyRx;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.Validation.Tests
{
    internal sealed class CueSheetValidationWindowModelTests
    {
        private readonly List<CueSheetAsset> _createdAssets = new();
        private readonly List<AudioConductorSettings> _createdSettings = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var a in _createdAssets)
                if (a != null)
                    Object.DestroyImmediate(a);
            _createdAssets.Clear();

            foreach (var s in _createdSettings)
                if (s != null)
                    Object.DestroyImmediate(s);
            _createdSettings.Clear();
        }

        private CueSheetAsset MakeAsset()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _createdAssets.Add(asset);
            return asset;
        }

        private AudioConductorSettings MakeSettings()
        {
            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _createdSettings.Add(settings);
            return settings;
        }

        [Test]
        public void Constructor_SavedGuid_RestoresSelection()
        {
            var settings = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { settings });
            provider.RegisterGuid(settings, "guid-001");
            var preferences = new FakePreferences { Stored = "guid-001" };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());

            Assert.That(model.SelectedSettings.Value, Is.SameAs(settings));
        }

        [Test]
        public void Constructor_SavedGuidMissingWithSingleSettings_AutoSelects()
        {
            var settings = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { settings });
            var preferences = new FakePreferences { Stored = string.Empty };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());

            Assert.That(model.SelectedSettings.Value, Is.SameAs(settings));
        }

        [Test]
        public void Constructor_SavedGuidMissingWithMultipleSettings_NoSelection()
        {
            var s1 = MakeSettings();
            var s2 = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { s1, s2 });
            var preferences = new FakePreferences { Stored = string.Empty };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());

            Assert.That(model.SelectedSettings.Value, Is.Null);
        }

        [Test]
        public void Constructor_DoesNotSaveSelectedGuid()
        {
            var settings = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { settings });
            provider.RegisterGuid(settings, "guid-001");
            var preferences = new FakePreferences { Stored = "guid-001" };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());

            Assert.That(preferences.SaveCount, Is.Zero);
        }

        [Test]
        public void Constructor_LoadsSelectedGuidOnce()
        {
            var settings = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { settings });
            provider.RegisterGuid(settings, "guid-001");
            var preferences = new FakePreferences { Stored = "guid-001" };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());

            Assert.That(preferences.LoadCount, Is.EqualTo(1));
        }

        [Test]
        public void RunValidation_NoAssets_EmptyRows()
        {
            var settings = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { settings });
            var preferences = new FakePreferences();

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RunValidation();

            Assert.That(model.ResultRows.Value, Is.Empty);
        }

        [Test]
        public void RunValidation_SingleAssetWithIssues_PopulatesRowsWithHeaderAndIssues()
        {
            var asset = MakeAsset();
            var issue = new ValidationIssue(ValidationSeverity.Warning, "Test.Code", "msg", string.Empty, null);
            var validator = new FakeCueSheetValidator(issue);
            var provider = new FakeSettingsProvider(new[] { MakeSettings() });
            var preferences = new FakePreferences();

            using var model = new CueSheetValidationWindowModel(
                new[] { asset }, ValidationScope.None, provider, preferences, validator);
            model.RunValidation();

            var rows = model.ResultRows.Value.ToList();
            Assert.That(rows.Count, Is.EqualTo(2));
            Assert.That(rows[0].IsIssueRow, Is.False);
            Assert.That(rows[0].Asset, Is.SameAs(asset));
            Assert.That(rows[1].IsIssueRow, Is.True);
            Assert.That(rows[1].Issue, Is.SameAs(issue));
        }

        [Test]
        public void RunValidation_NullAssetInList_Skipped()
        {
            var asset = MakeAsset();
            var provider = new FakeSettingsProvider(new[] { MakeSettings() });
            var preferences = new FakePreferences();

            using var model = new CueSheetValidationWindowModel(
                new[] { null!, asset }, ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RunValidation();

            var rows = model.ResultRows.Value.ToList();
            Assert.That(rows.Count(r => !r.IsIssueRow), Is.EqualTo(1));
            Assert.That(rows[0].Asset, Is.SameAs(asset));
        }

        [Test]
        public void RunValidation_CalledTwice_DoesNotAccumulateRows()
        {
            var asset = MakeAsset();
            var provider = new FakeSettingsProvider(new[] { MakeSettings() });
            var preferences = new FakePreferences();

            using var model = new CueSheetValidationWindowModel(
                new[] { asset }, ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RunValidation();
            var countAfterFirst = model.ResultRows.Value.Count();

            model.RunValidation();

            Assert.That(model.ResultRows.Value.Count(), Is.EqualTo(countAfterFirst));
        }

        [Test]
        public void RunValidation_SettingsNotFound_AddsWarningRow()
        {
            var provider = new FakeSettingsProvider(Array.Empty<AudioConductorSettings>());
            var preferences = new FakePreferences();

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RunValidation();

            var rows = model.ResultRows.Value.ToList();
            Assert.That(rows.Count, Is.EqualTo(1));
            Assert.That(rows[0].IsIssueRow, Is.True);
            Assert.That(rows[0].Issue?.Code, Is.EqualTo("Validation.SettingsNotFound"));
        }

        [Test]
        public void RunValidation_SettingsNotSelected_AddsWarningRow()
        {
            var s1 = MakeSettings();
            var s2 = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { s1, s2 });
            var preferences = new FakePreferences { Stored = string.Empty };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RunValidation();

            var rows = model.ResultRows.Value.ToList();
            Assert.That(rows.Count, Is.EqualTo(1));
            Assert.That(rows[0].IsIssueRow, Is.True);
            Assert.That(rows[0].Issue?.Code, Is.EqualTo("Validation.SettingsNotSelected"));
        }

        [Test]
        public void RunValidation_PassesSelectedSettingsToValidator()
        {
            var asset = MakeAsset();
            var settings = MakeSettings();
            var validator = new FakeCueSheetValidator();
            var provider = new FakeSettingsProvider(new[] { settings });
            provider.RegisterGuid(settings, "guid-001");
            var preferences = new FakePreferences { Stored = "guid-001" };

            using var model = new CueSheetValidationWindowModel(
                new[] { asset }, ValidationScope.None, provider, preferences, validator);
            model.RunValidation();

            Assert.That(validator.LastCall?.Asset, Is.SameAs(asset));
            Assert.That(validator.LastCall?.Settings, Is.SameAs(settings));
        }

        [Test]
        public void SelectSettings_PersistsGuid()
        {
            var settings = MakeSettings();
            var provider = new FakeSettingsProvider(new[] { settings });
            provider.RegisterGuid(settings, "guid-001");
            var preferences = new FakePreferences();

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.SelectSettings(settings);

            Assert.That(preferences.Stored, Is.EqualTo("guid-001"));
            Assert.That(model.SelectedSettings.Value, Is.SameAs(settings));
        }

        [Test]
        public void SelectSettings_Null_PersistsEmptyString()
        {
            var provider = new FakeSettingsProvider(Array.Empty<AudioConductorSettings>());
            var preferences = new FakePreferences { Stored = "some-guid" };

            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.SelectSettings(null);

            Assert.That(preferences.Stored, Is.EqualTo(string.Empty));
            Assert.That(model.SelectedSettings.Value, Is.Null);
        }

        [Test]
        public void SelectRow_FiresRowSelected()
        {
            var asset = MakeAsset();
            var row = new ValidationResultRow(null, asset, "test", false);
            var provider = new FakeSettingsProvider(Array.Empty<AudioConductorSettings>());
            var preferences = new FakePreferences();

            ValidationResultRow? received = null;
            using var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RowSelected.Subscribe(r => received = r);
            model.SelectRow(row);

            Assert.That(received, Is.SameAs(row));
        }

        [Test]
        public void Dispose_AfterDispose_RowSelectedSubscriptionDoesNotFire()
        {
            var asset = MakeAsset();
            var row = new ValidationResultRow(null, asset, "test", false);
            var provider = new FakeSettingsProvider(Array.Empty<AudioConductorSettings>());
            var preferences = new FakePreferences();
            var fired = false;

            var model = new CueSheetValidationWindowModel(
                Array.Empty<CueSheetAsset>(), ValidationScope.None, provider, preferences,
                new FakeCueSheetValidator());
            model.RowSelected.Subscribe(_ => fired = true);
            model.Dispose();
            model.SelectRow(row);

            Assert.That(fired, Is.False);
        }

        private sealed class FakeCueSheetValidator : ICueSheetValidator
        {
            private readonly IReadOnlyList<ValidationIssue> _issues;

            internal FakeCueSheetValidator(params ValidationIssue[] issues)
            {
                _issues = issues;
            }

            internal (CueSheetAsset? Asset, AudioConductorSettings? Settings)? LastCall { get; private set; }

            public IReadOnlyList<ValidationIssue> Validate(CueSheetAsset asset, AudioConductorSettings? settings)
            {
                LastCall = (asset, settings);
                return _issues;
            }
        }

        private sealed class FakeSettingsProvider : IAudioConductorSettingsProvider
        {
            private readonly Dictionary<AudioConductorSettings, string> _guidMap = new();

            internal FakeSettingsProvider(AudioConductorSettings[] all)
            {
                AllSettings = all;
            }

            public AudioConductorSettings[] AllSettings { get; }

            public AudioConductorSettings? GetByGuid(string guid)
            {
                return AllSettings.FirstOrDefault(s => _guidMap.TryGetValue(s, out var g) && g == guid);
            }

            public string GetGuid(AudioConductorSettings settings)
            {
                return _guidMap.TryGetValue(settings, out var g) ? g : string.Empty;
            }

            internal void RegisterGuid(AudioConductorSettings s, string guid)
            {
                _guidMap[s] = guid;
            }
        }

        private sealed class FakePreferences : IValidationSettingsPreferences
        {
            internal string Stored { get; set; } = string.Empty;
            internal int SaveCount { get; private set; }
            internal int LoadCount { get; private set; }

            public string LoadSelectedGuid()
            {
                LoadCount++;
                return Stored;
            }

            public void SaveSelectedGuid(string guid)
            {
                Stored = guid;
                SaveCount++;
            }
        }
    }
}
