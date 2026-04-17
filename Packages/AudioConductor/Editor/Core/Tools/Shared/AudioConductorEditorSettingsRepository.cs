// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Editor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class
        AudioConductorEditorSettingsRepository : ScriptableSingleton<AudioConductorEditorSettingsRepository>
    {
        private AudioConductorEditorSettings? _settings;

        public AudioConductorEditorSettings? Settings
        {
            get
            {
                if (_settings == null)
                    _settings = LoadSettings();

                return _settings;
            }
        }

        internal static event Action? SettingsChanged;

        private static AudioConductorEditorSettings? LoadSettings()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(AudioConductorEditorSettings), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<AudioConductorEditorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        internal static void NotifySettingsChanged()
        {
            instance._settings = null;
            SettingsChanged?.Invoke();
        }

        internal sealed class AssetPostProcessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (!HasSettingsAssetChange(importedAssets)
                    && !HasSettingsAssetChange(deletedAssets)
                    && !HasSettingsAssetChange(movedAssets)
                    && !HasSettingsAssetChange(movedFromAssetPaths))
                    return;

                NotifySettingsChanged();
            }

            public override int GetPostprocessOrder()
            {
                return int.MaxValue;
            }

            private static bool HasSettingsAssetChange(string[]? assetPaths)
            {
                if (assetPaths == null || assetPaths.Length == 0)
                    return false;

                var cachedSettingsPath = GetCachedSettingsPath();
                var existingSettingsPaths = GetExistingSettingsPaths();

                foreach (var assetPath in assetPaths)
                    if (assetPath == cachedSettingsPath || existingSettingsPaths.Contains(assetPath))
                        return true;

                return false;
            }

            private static string? GetCachedSettingsPath()
            {
                return instance._settings == null ? null : AssetDatabase.GetAssetPath(instance._settings);
            }

            private static HashSet<string> GetExistingSettingsPaths()
            {
                return AssetDatabase.FindAssets("t:" + nameof(AudioConductorEditorSettings), new[] { "Assets" })
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .ToHashSet();
            }
        }
    }
}
