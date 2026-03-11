// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.IO;
using System.Linq;
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
                var generated = false;
                foreach (var path in assetPaths)
                {
                    if (AssetDatabase.GetMainAssetTypeAtPath(path) != TargetType)
                        continue;
                    var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                    if (!asset.codeGenEnabled)
                        continue;

                    var result = CueEnumGenerator.Generate(asset);
                    if (!result.Success)
                        continue;

                    var fileName = result.EnumName + ".cs";
                    var outputPath = string.IsNullOrEmpty(asset.codeGenOutputPath)
                        ? (Path.GetDirectoryName(path) ?? ".") + "/" + fileName
                        : asset.codeGenOutputPath!.TrimEnd('/') + "/" + fileName;

                    // Skip write if content is identical
                    if (File.Exists(outputPath) && File.ReadAllText(outputPath) == result.SourceCode)
                        continue;

                    var dir = Path.GetDirectoryName(outputPath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    File.WriteAllText(outputPath, result.SourceCode);
                    generated = true;
                }

                if (generated)
                    AssetDatabase.Refresh();
            }
            finally
            {
                _isGenerating = false;
            }
        }
    }
}
