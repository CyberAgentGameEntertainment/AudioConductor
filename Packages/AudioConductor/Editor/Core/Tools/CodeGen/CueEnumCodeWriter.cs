// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.IO;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
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

            var settings = AudioConductorEditorSettingsRepository.instance.Settings;
            var resolvedSettings = CueEnumCodeGenSettingsResolver.Resolve(asset, settings);
            var fileName = generationResult.EnumName + ".cs";
            var outputPath = Path.Combine(resolvedSettings.OutputPath, fileName).Replace('\\', '/');

            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == generationResult.SourceCode)
                return new WriteResult(true, false, outputPath, generationResult.EnumName, generationResult.Errors);

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            WriteAllTextAtomically(outputPath, generationResult.SourceCode);
            AssetDatabase.Refresh();

            return new WriteResult(true, true, outputPath, generationResult.EnumName, generationResult.Errors);
        }

        /// <summary>
        ///     Writes source code to the specified path atomically.
        ///     Skips writing if the existing file content is identical.
        ///     Does NOT call AssetDatabase.Refresh.
        /// </summary>
        /// <returns>true if the file was written; false if the content was already up to date.</returns>
        internal static bool Write(string outputPath, string sourceCode)
        {
            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == sourceCode)
                return false;

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            WriteAllTextAtomically(outputPath, sourceCode);
            return true;
        }

        private static void WriteAllTextAtomically(string outputPath, string sourceCode)
        {
            var directory = Path.GetDirectoryName(outputPath);
            var tempPath = Path.Combine(directory ?? ".", Path.GetRandomFileName() + ".tmp");

            try
            {
                File.WriteAllText(tempPath, sourceCode);
                if (File.Exists(outputPath))
                    File.Replace(tempPath, outputPath, null);
                else
                    File.Move(tempPath, outputPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
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
