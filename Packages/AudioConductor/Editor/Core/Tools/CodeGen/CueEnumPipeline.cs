// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Editor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Orchestrates the enum code generation pipeline:
    ///     Build → Validate → Generate → Write → optional Refresh.
    /// </summary>
    internal static class CueEnumPipeline
    {
        internal static PipelineResult Execute(CueEnumDefinition definition, bool refreshAssets = true)
        {
            // 1. Build plans
            var plans = GenerationPlanBuilder.Build(definition);

            // 2. Validate
            var validationErrors = GenerationPlanBuilder.Validate(plans);
            if (validationErrors.Count > 0)
                return new PipelineResult(false, 0, 0, 0, validationErrors);

            // 3-4. Generate and Write
            var errors = new List<string>();
            var generatedCount = 0;
            var writtenCount = 0;
            var upToDateCount = 0;

            foreach (var plan in plans)
            {
                var genResult = CueEnumGenerator.Generate(plan);
                if (!genResult.Success)
                {
                    foreach (var error in genResult.Errors)
                        errors.Add($"[{plan.OutputFilePath}] {error}");
                    continue;
                }

                generatedCount++;

                var wrote = CueEnumCodeWriter.Write(plan.OutputFilePath, genResult.SourceCode);
                if (wrote)
                    writtenCount++;
                else
                    upToDateCount++;
            }

            // 5. Refresh once
            if (refreshAssets && writtenCount > 0)
                AssetDatabase.Refresh();

            return new PipelineResult(errors.Count == 0, generatedCount, writtenCount, upToDateCount, errors);
        }

        internal readonly struct PipelineResult
        {
            internal bool Success { get; }
            internal int GeneratedCount { get; }
            internal int WrittenCount { get; }
            internal int UpToDateCount { get; }
            internal IReadOnlyList<string> Errors { get; }

            internal PipelineResult(bool success, int generatedCount, int writtenCount, int upToDateCount,
                IReadOnlyList<string> errors)
            {
                Success = success;
                GeneratedCount = generatedCount;
                WrittenCount = writtenCount;
                UpToDateCount = upToDateCount;
                Errors = errors;
            }
        }
    }
}
