// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Localization
{
    internal enum EditorLanguage
    {
        Auto,
        English,
        Japanese
    }

    internal static class Localization
    {
        private const string LanguagePrefKey = "AudioConductor.EditorLanguage";

        internal static Dictionary<string, string> EnglishTable => EnglishTranslations.Table;
        internal static Dictionary<string, string> JapaneseTable => JapaneseTranslations.Table;

        public static EditorLanguage Language
        {
            get => (EditorLanguage)EditorPrefs.GetInt(LanguagePrefKey, (int)EditorLanguage.Auto);
            set
            {
                EditorPrefs.SetInt(LanguagePrefKey, (int)value);
                LanguageChanged?.Invoke();
            }
        }

        public static event Action? LanguageChanged;

        public static string Tr(string key)
        {
            var table = ResolveLanguage() == EditorLanguage.Japanese ? JapaneseTable : EnglishTable;
            return table.TryGetValue(key, out var text) ? text : key;
        }

        private static EditorLanguage ResolveLanguage()
        {
            var language = Language;
            if (language != EditorLanguage.Auto)
                return language;

            return DetectUnityLanguage();
        }

        private static EditorLanguage DetectUnityLanguage()
        {
            try
            {
                var type = Type.GetType("UnityEditor.LocalizationDatabase, UnityEditor");
                var property = type?.GetProperty("currentEditorLanguage",
                    BindingFlags.Public | BindingFlags.Static);
                var value = property?.GetValue(null);
                if (value != null)
                {
                    // SystemLanguage enum value for Japanese
                    var lang = (SystemLanguage)(int)value;
                    return lang == SystemLanguage.Japanese ? EditorLanguage.Japanese : EditorLanguage.English;
                }
            }
            catch (Exception)
            {
                // Fall through to Application.systemLanguage fallback
            }

            return Application.systemLanguage == SystemLanguage.Japanese
                ? EditorLanguage.Japanese
                : EditorLanguage.English;
        }
    }
}
