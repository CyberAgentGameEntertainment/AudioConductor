// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.Validation.Models;
using NUnit.Framework;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Validation.Tests
{
    internal sealed class EditorPrefsValidationSettingsPreferencesTests
    {
        private const string ValidationKey = "AudioConductor.ValidationSelectedSettingsGuid";
        private const string EditorSettingsKey = "AudioConductor.SelectedSettingsGuid";
        private string? _savedEditorSettingsKey;

        private string? _savedValidationKey;

        [SetUp]
        public void SetUp()
        {
            _savedValidationKey = EditorPrefs.HasKey(ValidationKey)
                ? EditorPrefs.GetString(ValidationKey)
                : null;
            _savedEditorSettingsKey = EditorPrefs.HasKey(EditorSettingsKey)
                ? EditorPrefs.GetString(EditorSettingsKey)
                : null;

            EditorPrefs.DeleteKey(ValidationKey);
            EditorPrefs.DeleteKey(EditorSettingsKey);
        }

        [TearDown]
        public void TearDown()
        {
            EditorPrefs.DeleteKey(ValidationKey);
            EditorPrefs.DeleteKey(EditorSettingsKey);

            if (_savedValidationKey != null)
                EditorPrefs.SetString(ValidationKey, _savedValidationKey);
            if (_savedEditorSettingsKey != null)
                EditorPrefs.SetString(EditorSettingsKey, _savedEditorSettingsKey);
        }

        [Test]
        public void LoadSelectedGuid_UsesValidationKeyFirst()
        {
            EditorPrefs.SetString(ValidationKey, "validation-guid");
            EditorPrefs.SetString(EditorSettingsKey, "editor-guid");

            var prefs = new EditorPrefsValidationSettingsPreferences();

            Assert.That(prefs.LoadSelectedGuid(), Is.EqualTo("validation-guid"));
        }

        [Test]
        public void LoadSelectedGuid_FallsBackToEditorSettingsKey()
        {
            EditorPrefs.SetString(EditorSettingsKey, "editor-guid");

            var prefs = new EditorPrefsValidationSettingsPreferences();

            Assert.That(prefs.LoadSelectedGuid(), Is.EqualTo("editor-guid"));
        }

        [Test]
        public void LoadSelectedGuid_NoKeys_ReturnsEmpty()
        {
            var prefs = new EditorPrefsValidationSettingsPreferences();

            Assert.That(prefs.LoadSelectedGuid(), Is.Empty);
        }

        [Test]
        public void LoadSelectedGuid_ValidationKeyEmpty_IgnoresEditorSettingsKey()
        {
            EditorPrefs.SetString(ValidationKey, string.Empty);
            EditorPrefs.SetString(EditorSettingsKey, "editor-guid");

            var prefs = new EditorPrefsValidationSettingsPreferences();

            Assert.That(prefs.LoadSelectedGuid(), Is.Empty);
        }

        [Test]
        public void SaveSelectedGuid_WritesValidationKeyOnly()
        {
            EditorPrefs.SetString(EditorSettingsKey, "editor-guid");

            var prefs = new EditorPrefsValidationSettingsPreferences();
            prefs.SaveSelectedGuid("new-guid");

            Assert.That(EditorPrefs.GetString(ValidationKey), Is.EqualTo("new-guid"));
            Assert.That(EditorPrefs.GetString(EditorSettingsKey), Is.EqualTo("editor-guid"));
        }

        [Test]
        public void SaveSelectedGuid_WithEmptyString_WritesEmptyToValidationKey()
        {
            var prefs = new EditorPrefsValidationSettingsPreferences();
            prefs.SaveSelectedGuid(string.Empty);

            Assert.That(EditorPrefs.HasKey(ValidationKey), Is.True);
            Assert.That(EditorPrefs.GetString(ValidationKey), Is.Empty);
        }
    }
}
