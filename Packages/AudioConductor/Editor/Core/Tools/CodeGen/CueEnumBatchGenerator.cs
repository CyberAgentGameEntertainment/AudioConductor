// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class CueEnumBatchGenerator
    {
        internal static BatchResult Generate(CueEnumBatchScope scope)
        {
            var assets = AssetDatabase.FindAssets("t:" + nameof(CueSheetAsset), new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<CueSheetAsset>)
                .ToArray();

            return Generate(scope, assets);
        }

        internal static BatchResult Generate(CueEnumBatchScope scope, IEnumerable<CueSheetAsset?> assets)
        {
            var failures = new List<Failure>();

            var targetCount = 0;
            var processedCount = 0;
            var writtenCount = 0;
            var upToDateCount = 0;

            foreach (var asset in assets)
            {
                if (asset == null)
                {
                    failures.Add(new Failure(string.Empty, string.Empty,
                        new[] { "CueSheetAsset could not be loaded." }));
                    continue;
                }

                if (scope == CueEnumBatchScope.EnabledOnly && !asset.codeGenEnabled)
                    continue;

                var path = AssetDatabase.GetAssetPath(asset);
                targetCount++;
                processedCount++;

                var result = CueEnumCodeWriter.Write(asset);
                if (!result.Success)
                {
                    failures.Add(new Failure(path, asset.name, result.Errors));
                    continue;
                }

                if (result.WroteFile)
                    writtenCount++;
                else
                    upToDateCount++;
            }

            return new BatchResult(
                failures.Count == 0,
                scope,
                targetCount,
                processedCount,
                writtenCount,
                upToDateCount,
                failures);
        }

        internal enum CueEnumBatchScope
        {
            EnabledOnly,
            All
        }

        internal readonly struct BatchResult
        {
            internal bool Success { get; }
            internal CueEnumBatchScope Scope { get; }
            internal int TargetCount { get; }
            internal int ProcessedCount { get; }
            internal int WrittenCount { get; }
            internal int UpToDateCount { get; }
            internal int FailedCount => Failures.Count;
            internal IReadOnlyList<Failure> Failures { get; }

            internal BatchResult(
                bool success,
                CueEnumBatchScope scope,
                int targetCount,
                int processedCount,
                int writtenCount,
                int upToDateCount,
                IReadOnlyList<Failure> failures)
            {
                Success = success;
                Scope = scope;
                TargetCount = targetCount;
                ProcessedCount = processedCount;
                WrittenCount = writtenCount;
                UpToDateCount = upToDateCount;
                Failures = failures;
            }
        }

        internal readonly struct Failure
        {
            internal string AssetPath { get; }
            internal string CueSheetName { get; }
            internal IReadOnlyList<string> Errors { get; }

            internal Failure(string assetPath, string cueSheetName, IReadOnlyList<string> errors)
            {
                AssetPath = assetPath;
                CueSheetName = cueSheetName;
                Errors = errors;
            }
        }
    }
}
