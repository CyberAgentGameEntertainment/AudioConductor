// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
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
                MigrateCueIds(asset);
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
            ProcessDeletedAssets(deletedAssets, CueSheetAssets, CueSheetIds);
        }

        private static void MoveAssets(IReadOnlyList<string> movedAssets, IReadOnlyList<string> movedFromAssetPaths)
        {
            ProcessMovedAssets(movedAssets, movedFromAssetPaths, CueSheetAssets);
        }

        private static void ImportAssets(IReadOnlyCollection<string> importedAssets)
        {
            var modified = ProcessImportedAssets(importedAssets, CueSheetAssets, CueSheetIds,
                path => AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path));
            foreach (var asset in modified)
                EditorUtility.SetDirty(asset);
            if (modified.Count > 0)
                AssetDatabase.SaveAssets();
        }

        internal static void ProcessDeletedAssets(IReadOnlyCollection<string>? deletedAssets,
            Dictionary<string, CueSheetAsset> cueSheetAssets,
            HashSet<string> cueSheetIds)
        {
            if (deletedAssets == null || deletedAssets.Count == 0)
                return;

            foreach (var deletedAsset in deletedAssets)
            {
                if (!cueSheetAssets.TryGetValue(deletedAsset, out var asset))
                    continue;

                cueSheetAssets.Remove(deletedAsset);
                cueSheetIds.Remove(asset.cueSheet.Id);
            }
        }

        internal static void ProcessMovedAssets(IReadOnlyList<string>? movedAssets,
            IReadOnlyList<string>? movedFromAssetPaths,
            Dictionary<string, CueSheetAsset> cueSheetAssets)
        {
            if (movedAssets == null || movedAssets.Count == 0 || movedFromAssetPaths == null
                || movedFromAssetPaths.Count == 0)
                return;

            for (var i = 0; i < movedFromAssetPaths.Count; i++)
            {
                var originalAssetPath = movedFromAssetPaths[i];
                if (!cueSheetAssets.TryGetValue(originalAssetPath, out var asset))
                    continue;

                cueSheetAssets.Remove(originalAssetPath);
                cueSheetAssets.Add(movedAssets[i], asset);
            }
        }

        internal static IReadOnlyList<CueSheetAsset> ProcessImportedAssets(
            IReadOnlyCollection<string>? importedAssets,
            Dictionary<string, CueSheetAsset> cueSheetAssets,
            HashSet<string> cueSheetIds,
            Func<string, CueSheetAsset?> loadAsset)
        {
            var modifiedAssets = new List<CueSheetAsset>();

            if (importedAssets == null || importedAssets.Count == 0)
                return modifiedAssets;

            foreach (var importedAsset in importedAssets)
            {
                if (cueSheetAssets.ContainsKey(importedAsset))
                    continue; // reimport

                var asset = loadAsset(importedAsset);
                if (asset == null)
                    continue; // not CueSheetAsset

                if (ShouldDuplicateCueSheet(asset.cueSheet.Id, cueSheetIds))
                {
                    asset.cueSheet = asset.cueSheet.Duplicate()!;
                    modifiedAssets.Add(asset);
                }
                else if (NormalizeCueIdsIfNeeded(asset.cueSheet))
                {
                    modifiedAssets.Add(asset);
                }

                cueSheetAssets.Add(importedAsset, asset);
                cueSheetIds.Add(asset.cueSheet.Id);
            }

            return modifiedAssets;
        }

        internal static bool ShouldDuplicateCueSheet(string cueSheetId, IReadOnlyCollection<string> existingCueSheetIds)
        {
            return !string.IsNullOrEmpty(cueSheetId) && existingCueSheetIds.Contains(cueSheetId);
        }

        internal static bool NormalizeCueIdsIfNeeded(CueSheet cueSheet)
        {
            if (cueSheet == null || !CueIdAssigner.HasDuplicateCueIds(cueSheet.cueList))
                return false;

            // Reset all IDs and re-assign to ensure uniqueness even if non-zero duplicates exist.
            foreach (var cue in cueSheet.cueList)
                cue.cueId = 0;
            CueIdAssigner.AssignMissingCueIds(cueSheet.cueList);
            return true;
        }

        private static void MigrateCueIds(CueSheetAsset asset)
        {
            if (!NormalizeCueIdsIfNeeded(asset.cueSheet))
                return;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}
