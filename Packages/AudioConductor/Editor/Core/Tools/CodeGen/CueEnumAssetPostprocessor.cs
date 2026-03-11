// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Linq;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal sealed class CueEnumAssetPostprocessor : AssetPostprocessor
    {
        private static readonly Type TargetType = typeof(CueSheetAsset);
        private static bool _isGenerating;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (_isGenerating)
                return;

            var assetPaths = importedAssets.Where(p => p.EndsWith(".asset")).ToArray();
            if (assetPaths.Length == 0)
                return;

            _isGenerating = true;
            try
            {
                foreach (var path in assetPaths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) != TargetType)
                        continue;
                    var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                    if (!asset.codeGenEnabled)
                        continue;
                    if (asset.codeGenMode != CueSheetCodeGenMode.OnSave)
                        continue;

                    CueEnumCodeWriter.Write(asset);
                }
            }
            finally
            {
                _isGenerating = false;
            }
        }
    }
}
