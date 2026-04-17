// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class AudioConductorSettingsRepository : ScriptableSingleton<AudioConductorSettingsRepository>
    {
        private AudioConductorSettings[]? _allSettings; // null = cache invalidated

        /// <summary>
        ///     Returns all <see cref="AudioConductorSettings" /> assets found in the Assets folder.
        /// </summary>
        public AudioConductorSettings[] AllSettings
        {
            get
            {
                // Unity serialization converts null arrays to empty arrays on domain reload.
                // Check both to ensure re-querying when the cache was invalidated.
                if (_allSettings == null || _allSettings.Length == 0)
                    _allSettings = LoadAllSettings();

                return _allSettings;
            }
        }

        /// <summary>
        ///     Resolves an <see cref="AudioConductorSettings" /> asset by its GUID.
        /// </summary>
        public AudioConductorSettings? GetByGuid(string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return null;

            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(path);
        }

        private static AudioConductorSettings[] LoadAllSettings()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(AudioConductorSettings), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return Array.Empty<AudioConductorSettings>();

            var result = new AudioConductorSettings[guids.Length];
            for (var i = 0; i < guids.Length; i++)
                result[i] =
                    AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(AssetDatabase.GUIDToAssetPath(guids[i]));
            return result;
        }

        internal sealed class AssetPostProcessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                instance._allSettings = null;
            }

            public override int GetPostprocessOrder()
            {
                return int.MaxValue;
            }
        }
    }
}
