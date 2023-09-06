// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class
        AudioConductorEditorSettingsRepository : ScriptableSingleton<AudioConductorEditorSettingsRepository>
    {
        private AudioConductorEditorSettings _settings;

        public AudioConductorEditorSettings Settings
        {
            get
            {
                if (_settings == null)
                    _settings = LoadSettings();

                return _settings;
            }
        }

        private static AudioConductorEditorSettings LoadSettings()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(AudioConductorEditorSettings), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<AudioConductorEditorSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
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
