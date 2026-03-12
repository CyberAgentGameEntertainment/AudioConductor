// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Automatically generates cue enums before a build starts.
    /// </summary>
    internal sealed class CueEnumBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var definition = CueEnumDefinitionRepository.instance.Definition;
            if (definition == null)
                return;

            var result = CueEnumPipeline.Execute(definition);
            if (!result.Success)
            {
                var errorMessage =
                    $"Cue enum generation failed with {result.Errors.Count} error(s). Check the console for details.";
                Debug.LogError(errorMessage);
                throw new BuildFailedException(errorMessage);
            }

            if (result.WrittenCount > 0)
                Debug.Log($"Cue enum generation: {result.WrittenCount} file(s)written before build.");
        }
    }
}
