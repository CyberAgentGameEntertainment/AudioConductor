// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class CueEnumCodeGenSettingsResolver
    {
        internal const string LegacyDefaultOutputPath = "Assets/Scripts/Generated/";

        internal static ResolvedSettings Resolve(CueSheetAsset asset, AudioConductorEditorSettings? settings)
        {
            var outputPath = asset.useDefaultCodeGenOutputPath
                ? NormalizeOutputPath(settings?.defaultCodeGenOutputPath)
                : NormalizeOutputPath(asset.codeGenOutputPath);
            var codeGenNamespace = asset.useDefaultCodeGenNamespace
                ? NormalizeOptional(settings?.defaultCodeGenNamespace)
                : NormalizeOptional(asset.codeGenNamespace);
            var classSuffix = asset.useDefaultCodeGenClassSuffix
                ? NormalizeOptional(settings?.defaultCodeGenClassSuffix)
                : NormalizeOptional(asset.codeGenClassSuffix);

            return new ResolvedSettings(outputPath, codeGenNamespace, classSuffix);
        }

        private static string NormalizeOutputPath(string? value)
        {
            return string.IsNullOrEmpty(value) ? LegacyDefaultOutputPath : value!;
        }

        private static string NormalizeOptional(string? value)
        {
            return value ?? string.Empty;
        }

        internal readonly struct ResolvedSettings
        {
            internal string OutputPath { get; }
            internal string Namespace { get; }
            internal string ClassSuffix { get; }

            internal ResolvedSettings(string outputPath, string @namespace, string classSuffix)
            {
                OutputPath = outputPath;
                Namespace = @namespace;
                ClassSuffix = classSuffix;
            }
        }
    }
}
