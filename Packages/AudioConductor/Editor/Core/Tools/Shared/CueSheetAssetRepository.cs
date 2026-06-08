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
    internal sealed class CueSheetAssetRepository : ScriptableSingleton<CueSheetAssetRepository>,
        ICueSheetAssetRepository
    {
        private CueSheetAsset[]? _all;

        public event Action? Changed;

        /// <summary>
        ///     Returns all <see cref="CueSheetAsset" /> assets found in the Assets folder.
        /// </summary>
        public CueSheetAsset[] GetAll()
        {
            if (_all is null || _all.Length == 0)
                _all = LoadAll();
            return _all;
        }

        private static CueSheetAsset[] LoadAll()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(CueSheetAsset), new[] { "Assets" });
            if (guids is null || guids.Length == 0)
                return Array.Empty<CueSheetAsset>();

            var result = new List<CueSheetAsset>(guids.Length);
            foreach (var guid in guids)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                    result.Add(asset);
            }

            return result.ToArray();
        }

        internal sealed class AssetPostProcessor : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (!HasAssetFileChanges(importedAssets, deletedAssets, movedAssets))
                    return;

                instance._all = null;
                instance.Changed?.Invoke();
            }

            private static bool HasAssetFileChanges(string[] imported, string[] deleted, string[] moved)
            {
                return ContainsAssetFile(imported) || ContainsAssetFile(deleted) || ContainsAssetFile(moved);
            }

            private static bool ContainsAssetFile(string[] paths)
            {
                if (paths is null)
                    return false;
                foreach (var path in paths)
                    if (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                        return true;
                return false;
            }

            public override int GetPostprocessOrder()
            {
                return int.MaxValue;
            }
        }
    }
}
