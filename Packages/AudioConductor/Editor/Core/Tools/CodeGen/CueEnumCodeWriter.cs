// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.IO;
using AudioConductor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class CueEnumCodeWriter
    {
        internal static WriteResult Write(CueSheetAsset asset)
        {
            var generationResult = CueEnumGenerator.Generate(asset);
            if (!generationResult.Success)
                return new WriteResult(false, false, string.Empty, string.Empty, generationResult.Errors);

            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(assetPath))
                return new WriteResult(false, false, string.Empty, generationResult.EnumName,
                    new[] { "CueSheetAsset path could not be resolved." });

            var fileName = generationResult.EnumName + ".cs";
            var outputPath = string.IsNullOrEmpty(asset.codeGenOutputPath)
                ? (Path.GetDirectoryName(assetPath) ?? ".") + "/" + fileName
                : asset.codeGenOutputPath!.TrimEnd('/') + "/" + fileName;

            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == generationResult.SourceCode)
                return new WriteResult(true, false, outputPath, generationResult.EnumName, generationResult.Errors);

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(outputPath, generationResult.SourceCode);
            AssetDatabase.Refresh();

            return new WriteResult(true, true, outputPath, generationResult.EnumName, generationResult.Errors);
        }

        internal readonly struct WriteResult
        {
            internal bool Success { get; }
            internal bool WroteFile { get; }
            internal string OutputPath { get; }
            internal string EnumName { get; }
            internal IReadOnlyList<string> Errors { get; }

            internal WriteResult(bool success, bool wroteFile, string outputPath, string enumName,
                IReadOnlyList<string> errors)
            {
                Success = success;
                WroteFile = wroteFile;
                OutputPath = outputPath;
                EnumName = enumName;
                Errors = errors;
            }
        }
    }
}
