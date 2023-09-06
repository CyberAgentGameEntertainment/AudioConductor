// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class AudioConductorSettingsRepository : ScriptableSingleton<AudioConductorSettingsRepository>
    {
        private AudioConductorSettings _settings;

        public AudioConductorSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = LoadSettings();

                return _settings;
            }
        }

        private static AudioConductorSettings LoadSettings()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(AudioConductorSettings), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        internal sealed class AssetPostProcessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets,
                                                       string[] deletedAssets,
                                                       string[] movedAssets,
                                                       string[] movedFromAssetPaths)
            {
                instance._settings = null;
            }

            public override int GetPostprocessOrder() => int.MaxValue;
        }
    }
}
