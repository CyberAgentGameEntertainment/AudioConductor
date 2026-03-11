// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Provides menu and CLI entry points for batch cue enum generation.
    /// </summary>
    public static class CueEnumBatchCommand
    {
        private const string MenuRoot = "Tools/Audio Conductor/Generate Cue Enums/";
        private const int MaxDialogFailureDetails = 5;

        [MenuItem(MenuRoot + "Enabled CueSheets")]
        private static void GenerateEnabledCueEnumsMenu()
        {
            Execute(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, false);
        }

        [MenuItem(MenuRoot + "All CueSheets")]
        private static void GenerateAllCueEnumsMenu()
        {
            Execute(CueEnumBatchGenerator.CueEnumBatchScope.All, false);
        }

        /// <summary>
        ///     Generates cue enums for all enabled cue sheets and exits with a process code for batchmode usage.
        /// </summary>
        public static void GenerateEnabledCueEnums()
        {
            Execute(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, true);
        }

        /// <summary>
        ///     Generates cue enums for all cue sheets and exits with a process code for batchmode usage.
        /// </summary>
        public static void GenerateAllCueEnums()
        {
            Execute(CueEnumBatchGenerator.CueEnumBatchScope.All, true);
        }

        private static void Execute(CueEnumBatchGenerator.CueEnumBatchScope scope, bool exitWhenDone)
        {
            var result = CueEnumBatchGenerator.Generate(scope);
            var logMessage = BuildMessage(result, int.MaxValue);

            if (result.Success)
                Debug.Log(logMessage);
            else
                Debug.LogError(logMessage);

            if (exitWhenDone)
            {
                EditorApplication.Exit(result.Success ? 0 : 1);
                return;
            }

            EditorUtility.DisplayDialog(GetTitle(result), BuildMessage(result, MaxDialogFailureDetails), "OK");
        }

        private static string GetTitle(CueEnumBatchGenerator.BatchResult result)
        {
            if (!result.Success)
                return "Cue enum batch generation finished with errors.";

            if (result.TargetCount == 0)
                return "Cue enum batch generation found no targets.";

            if (result.WrittenCount == 0)
                return "Cue enum batch generation found all files up to date.";

            return "Cue enum batch generation completed.";
        }

        private static string BuildMessage(CueEnumBatchGenerator.BatchResult result, int maxFailureDetails)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Scope: {FormatScope(result.Scope)}");
            builder.AppendLine($"Targets: {result.TargetCount}");
            builder.AppendLine($"Processed: {result.ProcessedCount}");
            builder.AppendLine($"Written: {result.WrittenCount}");
            builder.AppendLine($"Up to date: {result.UpToDateCount}");
            builder.AppendLine($"Failed: {result.FailedCount}");

            if (result.Failures.Count == 0)
                return builder.ToString().TrimEnd();

            builder.AppendLine();
            builder.AppendLine("Failures:");

            foreach (var failure in result.Failures.Take(maxFailureDetails))
            {
                builder.Append("- ");
                builder.AppendLine(string.IsNullOrEmpty(failure.CueSheetName)
                    ? failure.AssetPath
                    : $"{failure.CueSheetName} ({failure.AssetPath})");

                foreach (var error in failure.Errors)
                    builder.AppendLine($"  {error}");
            }

            if (result.Failures.Count > maxFailureDetails)
                builder.AppendLine($"... and {result.Failures.Count - maxFailureDetails} more.");

            return builder.ToString().TrimEnd();
        }

        private static string FormatScope(CueEnumBatchGenerator.CueEnumBatchScope scope)
        {
            return scope == CueEnumBatchGenerator.CueEnumBatchScope.All ? "All CueSheets" : "Enabled CueSheets";
        }
    }
}
