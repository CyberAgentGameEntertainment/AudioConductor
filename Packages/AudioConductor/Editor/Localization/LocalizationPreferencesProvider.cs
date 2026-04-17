// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Localization
{
    internal static class LocalizationPreferencesProvider
    {
        private const string PreferencesPath = "Preferences/AudioConductor";

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var provider = new SettingsProvider(PreferencesPath, SettingsScope.User)
            {
                keywords = new HashSet<string> { "AudioConductor", "Language" },
                activateHandler = OnActivate
            };
            return provider;
        }

        private static void OnActivate(string searchContext, VisualElement rootElement)
        {
            var languageField = new EnumField("Language", Localization.Language);
            languageField.RegisterValueChangedCallback(OnLanguageFieldChanged);
            rootElement.Add(languageField);
        }

        private static void OnLanguageFieldChanged(ChangeEvent<Enum> evt)
        {
            Localization.Language = (EditorLanguage)evt.newValue;
        }
    }
}
