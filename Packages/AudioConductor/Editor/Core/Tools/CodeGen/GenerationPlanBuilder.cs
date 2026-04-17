// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Builds <see cref="GenerationPlan" /> instances from a <see cref="CueEnumDefinition" />.
    /// </summary>
    internal static class GenerationPlanBuilder
    {
        /// <summary>
        ///     Builds generation plans from the definition.
        ///     rootEntries produce individual plans (1 asset → 1 file).
        ///     fileEntries produce merged plans (N assets → 1 file).
        /// </summary>
        internal static IReadOnlyList<GenerationPlan> Build(CueEnumDefinition definition)
        {
            var plans = new List<GenerationPlan>();

            // rootEntries → individual plans
            foreach (var asset in definition.rootEntries)
            {
                if (asset == null)
                    continue;

                var baseName = GetBaseName(asset);
                var enumName = IdentifierConverter.ToPascalCase(baseName) +
                               NormalizeOptional(definition.defaultClassSuffix);
                var fileName = enumName + ".cs";
                var outputFilePath = Path.Combine(NormalizeOutputPath(definition.defaultOutputPath), fileName)
                    .Replace('\\', '/');

                var entry = new EnumEntry(enumName, baseName, asset);
                plans.Add(new GenerationPlan(outputFilePath, NormalizeOptional(definition.defaultNamespace),
                    new[] { entry }));
            }

            // fileEntries → merged plans
            foreach (var fileEntry in definition.fileEntries)
            {
                if (string.IsNullOrEmpty(fileEntry.fileName))
                    continue;

                var outputPath = fileEntry.useDefaultOutputPath
                    ? NormalizeOutputPath(definition.defaultOutputPath)
                    : NormalizeOutputPath(fileEntry.outputPath);
                var ns = fileEntry.useDefaultNamespace
                    ? NormalizeOptional(definition.defaultNamespace)
                    : NormalizeOptional(fileEntry.@namespace);
                var classSuffix = fileEntry.useDefaultClassSuffix
                    ? NormalizeOptional(definition.defaultClassSuffix)
                    : NormalizeOptional(fileEntry.classSuffix);

                var outputFilePath = Path.Combine(outputPath, fileEntry.fileName + ".cs").Replace('\\', '/');

                var entries = new List<EnumEntry>();
                foreach (var asset in fileEntry.assets)
                {
                    if (asset == null)
                        continue;

                    var baseName = GetBaseName(asset);
                    var enumName = IdentifierConverter.ToPascalCase(baseName) + classSuffix;
                    entries.Add(new EnumEntry(enumName, baseName, asset));
                }

                if (entries.Count > 0)
                    plans.Add(new GenerationPlan(outputFilePath, ns, entries));
            }

            return plans;
        }

        /// <summary>
        ///     Validates the built plans for conflicts.
        /// </summary>
        /// <returns>List of validation error messages. Empty if all valid.</returns>
        internal static IReadOnlyList<string> Validate(IReadOnlyList<GenerationPlan> plans)
        {
            var errors = new List<string>();

            // Check OutputFilePath uniqueness
            var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var plan in plans)
                if (!seenPaths.Add(plan.OutputFilePath))
                    errors.Add($"Duplicate output file path: \"{plan.OutputFilePath}\"");

            // Check EnumName uniqueness within each merged plan
            foreach (var plan in plans)
            {
                if (plan.Entries.Count <= 1)
                    continue;

                var seenNames = new HashSet<string>();
                foreach (var entry in plan.Entries)
                    if (!seenNames.Add(entry.EnumName))
                        errors.Add(
                            $"Duplicate EnumName \"{entry.EnumName}\" in file \"{plan.OutputFilePath}\"");
            }

            return errors;
        }

        private static string GetBaseName(CueSheetAsset asset)
        {
            return !string.IsNullOrEmpty(asset.cueSheet.name) ? asset.cueSheet.name : asset.name;
        }

        private static string NormalizeOutputPath(string? value)
        {
            return string.IsNullOrEmpty(value) ? "Assets/Scripts/Generated/" : value!;
        }

        private static string NormalizeOptional(string? value)
        {
            return value ?? string.Empty;
        }
    }
}
