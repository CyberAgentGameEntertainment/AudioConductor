// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class AudioConductorSettingsRepository : ScriptableSingleton<AudioConductorSettingsRepository>,
        IAudioConductorSettingsProvider
    {
        [NonSerialized] private AudioConductorSettings[]? _allSettings; // null = cache not yet loaded

        /// <summary>
        ///     Returns all <see cref="AudioConductorSettings" /> assets found in the Assets folder.
        /// </summary>
        public AudioConductorSettings[] AllSettings
        {
            get
            {
                if (_allSettings is null)
                    _allSettings = LoadAllSettings();

                return _allSettings;
            }
        }

        /// <summary>
        ///     Returns the GUID of the given <see cref="AudioConductorSettings" /> asset.
        /// </summary>
        public string GetGuid(AudioConductorSettings settings)
        {
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(settings));
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

            var result = new List<AudioConductorSettings>(guids.Length);
            foreach (var guid in guids)
            {
                var settings =
                    AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(AssetDatabase.GUIDToAssetPath(guid));
                if (settings != null)
                    result.Add(settings);
            }

            return result.ToArray();
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
