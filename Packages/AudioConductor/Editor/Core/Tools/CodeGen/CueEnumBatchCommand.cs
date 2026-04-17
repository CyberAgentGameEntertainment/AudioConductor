// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Linq;
using System.Text;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Provides menu and CLI entry points for cue enum generation via <see cref="CueEnumPipeline" />.
    /// </summary>
    public static class CueEnumBatchCommand
    {
        private const int MaxDialogFailureDetails = 5;

        [MenuItem("Tools/Audio Conductor/Generate Cue Enums")]
        private static void GenerateCueEnumsMenu()
        {
            var definition = CueEnumDefinitionRepository.instance.Definition;
            if (definition == null)
            {
                var ok = EditorUtility.DisplayDialog(
                    "Generate Cue Enums",
                    "CueEnumDefinition asset does not exist. A new one will be created.",
                    "OK",
                    "Cancel");

                if (!ok)
                    return;

                definition = CueEnumDefinitionRepository.instance.GetOrCreate();
            }

            Execute(definition, false);
        }

        /// <summary>
        ///     Generates cue enums from the CueEnumDefinition and exits with a process code for batchmode usage.
        /// </summary>
        public static void GenerateCueEnums()
        {
            var definition = CueEnumDefinitionRepository.instance.GetOrCreate();
            Execute(definition, true);
        }

        private static void Execute(CueEnumDefinition definition, bool exitWhenDone)
        {
            var result = CueEnumPipeline.Execute(definition);
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

        private static string GetTitle(CueEnumPipeline.PipelineResult result)
        {
            if (!result.Success)
                return "Cue enum generation finished with errors.";

            if (result.GeneratedCount == 0)
                return "Cue enum generation found no targets.";

            if (result.WrittenCount == 0)
                return "Cue enum generation found all files up to date.";

            return "Cue enum generation completed.";
        }

        private static string BuildMessage(CueEnumPipeline.PipelineResult result, int maxFailureDetails)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Generated: {result.GeneratedCount}");
            builder.AppendLine($"Written: {result.WrittenCount}");
            builder.AppendLine($"Up to date: {result.UpToDateCount}");

            if (result.Errors.Count == 0)
                return builder.ToString().TrimEnd();

            builder.AppendLine($"Errors: {result.Errors.Count}");
            builder.AppendLine();
            builder.AppendLine("Details:");

            foreach (var error in result.Errors.Take(maxFailureDetails))
                builder.AppendLine($"- {error}");

            if (result.Errors.Count > maxFailureDetails)
                builder.AppendLine($"... and {result.Errors.Count - maxFailureDetails} more.");

            return builder.ToString().TrimEnd();
        }
    }
}
