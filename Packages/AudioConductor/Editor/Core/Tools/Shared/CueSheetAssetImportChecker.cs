// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    [InitializeOnLoad]
    internal class CueSheetAssetImportChecker : AssetPostprocessor
    {
        private static readonly Dictionary<string, CueSheetAsset> CueSheetAssets;
        private static readonly HashSet<string> CueSheetIds;

        static CueSheetAssetImportChecker()
        {
            CueSheetAssets = new Dictionary<string, CueSheetAsset>(256);
            CueSheetIds = new HashSet<string>(256);

            var guids = AssetDatabase.FindAssets("t:" + nameof(CueSheetAsset), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                CueSheetAssets.Add(path, asset);
                CueSheetIds.Add(asset.cueSheet.Id);
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets,
                                                   string[] deletedAssets,
                                                   string[] movedAssets,
                                                   string[] movedFromAssetPaths)
        {
            DeleteAssets(deletedAssets);
            MoveAssets(movedAssets, movedFromAssetPaths);
            ImportAssets(importedAssets);
        }

        private static void DeleteAssets(IReadOnlyCollection<string> deletedAssets)
        {
            if (deletedAssets == null || deletedAssets.Count == 0)
                return;

            foreach (var deletedAsset in deletedAssets)
            {
                if (CueSheetAssets.TryGetValue(deletedAsset, out var asset) == false)
                    continue;

                CueSheetAssets.Remove(deletedAsset);
                CueSheetIds.Remove(asset.cueSheet.Id);
            }
        }

        private static void MoveAssets(IReadOnlyList<string> movedAssets, IReadOnlyList<string> movedFromAssetPaths)
        {
            if (movedAssets == null || movedAssets.Count == 0 || movedFromAssetPaths == null
                || movedFromAssetPaths.Count == 0)
                return;

            for (var i = 0; i < movedFromAssetPaths.Count; i++)
            {
                var originalAssetPath = movedFromAssetPaths[i];
                if (CueSheetAssets.TryGetValue(originalAssetPath, out var asset) == false)
                    continue;

                CueSheetAssets.Remove(originalAssetPath);
                CueSheetAssets.Add(movedAssets[i], asset);
            }
        }

        private static void ImportAssets(IReadOnlyCollection<string> importedAssets)
        {
            if (importedAssets == null || importedAssets.Count == 0)
                return;

            foreach (var importedAsset in importedAssets)
            {
                if (CueSheetAssets.TryGetValue(importedAsset, out var asset))
                    continue; // reimport

                asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(importedAsset);
                if (asset == null)
                    continue; // not CueSheetAsset

                if (CueSheetIds.Contains(asset.cueSheet.Id) == false)
                {
                    // create new
                    CueSheetAssets.Add(importedAsset, asset);
                    CueSheetIds.Add(asset.cueSheet.Id);
                    continue;
                }

                // duplicate
                asset.cueSheet = asset.cueSheet.Duplicate();
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                CueSheetAssets.Add(importedAsset, asset);
                CueSheetIds.Add(asset.cueSheet.Id);
            }
        }
    }
}
