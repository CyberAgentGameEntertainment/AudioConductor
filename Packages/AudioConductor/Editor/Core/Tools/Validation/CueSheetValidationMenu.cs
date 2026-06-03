// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    internal static class CueSheetValidationMenu
    {
        [MenuItem("Tools/Audio Conductor/Validate")]
        private static void ValidateAll()
        {
            CueSheetValidationWindow.Open(CueSheetAssetRepository.instance.GetAll(), ValidationScope.All);
        }

        [MenuItem("Assets/Audio Conductor/Validate CueSheet", validate = false)]
        private static void ValidateSelected()
        {
            CueSheetValidationWindow.Open(LoadSelectedCueSheetAssets(), ValidationScope.Selected);
        }

        [MenuItem("Assets/Audio Conductor/Validate CueSheet", validate = true)]
        private static bool ValidateSelectedEnabled()
        {
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(CueSheetAsset))
                    return true;
            }

            return false;
        }

        private static List<CueSheetAsset> LoadSelectedCueSheetAssets()
        {
            var assets = new List<CueSheetAsset>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                if (asset != null)
                    assets.Add(asset);
            }

            return assets;
        }
    }
}
