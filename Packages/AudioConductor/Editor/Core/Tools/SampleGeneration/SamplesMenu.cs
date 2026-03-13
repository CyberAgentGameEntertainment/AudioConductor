// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AudioConductor.Editor.SamplesDeployment;
using UnityEditor;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.SampleGeneration
{
    /// <summary>
    ///     Editor menu for generating, cleaning, and deploying samples.
    /// </summary>
    internal static class SamplesMenu
    {
        private const string PackagePath = "Packages/jp.co.cyberagent.audioconductor";
        private const string SamplesDestinationFolder = "Samples~";

        // --- Generate ---

#if AUDIOCONDUCTOR_DEVELOPER
        [MenuItem("Tools/Audio Conductor/Development/Samples/Generate")]
#endif
        private static void GenerateAudioConductorSample()
            => GenerateSample(SampleRegistry.FindByName("AudioConductorSample")!);

        // --- Clean ---

#if AUDIOCONDUCTOR_DEVELOPER
        [MenuItem("Tools/Audio Conductor/Development/Samples/Clean/Assets Sample")]
#endif
        private static void CleanAssetsSample()
        {
            if (!Directory.Exists(SampleRegistry.SamplesRootPath))
            {
                EditorUtility.DisplayDialog(
                    "Clean Samples",
                    "No samples folder found in Assets.",
                    "OK"
                );
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Clean Samples",
                    $"This will delete all contents in:\n{SampleRegistry.SamplesRootPath}\n\nContinue?",
                    "Clean",
                    "Cancel"))
                return;

            AssetDatabase.DeleteAsset(SampleRegistry.SamplesRootPath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Clean Samples - Success",
                "Successfully cleaned Assets samples.",
                "OK"
            );
        }

#if AUDIOCONDUCTOR_DEVELOPER
        [MenuItem("Tools/Audio Conductor/Development/Samples/Clean/Package Sample")]
#endif
        private static void CleanPackageSample()
        {
            var samplesDestinationPath = Path.Combine(PackagePath, SamplesDestinationFolder);

            if (!Directory.Exists(samplesDestinationPath))
            {
                EditorUtility.DisplayDialog(
                    "Clean Samples",
                    "No samples folder found in package.",
                    "OK"
                );
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Clean Samples",
                    $"This will delete all contents in:\n{samplesDestinationPath}\n\nContinue?",
                    "Clean",
                    "Cancel"))
                return;

            try
            {
                Directory.Delete(samplesDestinationPath, true);
                AssetDatabase.Refresh();

                EditorUtility.DisplayDialog(
                    "Clean Samples - Success",
                    "Successfully cleaned package samples.",
                    "OK"
                );
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Clean Samples - Error",
                    $"Failed to clean samples:\n{ex.Message}",
                    "OK"
                );
            }
        }

        // --- Deploy ---

#if AUDIOCONDUCTOR_DEVELOPER
        [MenuItem("Tools/Audio Conductor/Development/Samples/Deploy to Package")]
#endif
        private static void DeployToPackage()
        {
            var scanResult = SampleScanner.Scan(SampleRegistry.SamplesRootPath);

            if (!scanResult.IsValid)
            {
                var errorMessage = string.Join("\n", scanResult.Errors);
                EditorUtility.DisplayDialog(
                    "Deploy Samples - Error",
                    $"Validation failed:\n{errorMessage}",
                    "OK"
                );
                return;
            }

            var sampleNames = string.Join("\n", scanResult.SampleFolders.Select(f => $"  - {f.Name}"));
            var confirmMessage = $"The following samples will be deployed:\n{sampleNames}\n\nContinue?";

            if (!EditorUtility.DisplayDialog("Deploy Samples", confirmMessage, "Deploy", "Cancel"))
                return;

#if AUDIOCONDUCTOR_DEVELOPER
            if (!DeploySamples(scanResult))
                return;
#endif

            AssetDatabase.Refresh();
        }

        // --- Generate and Deploy ---

#if AUDIOCONDUCTOR_DEVELOPER
        [MenuItem("Tools/Audio Conductor/Development/Samples/Generate and Deploy")]
#endif
        private static void GenerateAndDeploy()
        {
            var samples = SampleRegistry.All;
            var totalCreatedFiles = 0;

            foreach (var sample in samples)
            {
                sample.Clean();

                var result = sample.Generate();

                if (!result.IsSuccess)
                {
                    var errorMessage = string.Join("\n", result.Errors);
                    EditorUtility.DisplayDialog(
                        "Generate and Deploy - Error",
                        $"Failed to generate '{sample.DisplayName}':\n{errorMessage}",
                        "OK"
                    );
                    return;
                }

                totalCreatedFiles += result.CreatedFiles.Count;
            }

            var scanResult = SampleScanner.Scan(SampleRegistry.SamplesRootPath);

            if (!scanResult.IsValid)
            {
                var errorMessage = string.Join("\n", scanResult.Errors);
                EditorUtility.DisplayDialog(
                    "Generate and Deploy - Error",
                    $"Validation failed:\n{errorMessage}",
                    "OK"
                );
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "Generate and Deploy",
                    "All samples generated successfully.\n" +
                    $"Files created: {totalCreatedFiles}\n\n" +
                    "Deploy to package?",
                    "Deploy",
                    "Skip"))
                return;

#if AUDIOCONDUCTOR_DEVELOPER
            if (!DeploySamples(scanResult))
                return;
#endif

            AssetDatabase.Refresh();
        }

        // --- Shared logic ---

        private static void GenerateSample(ISample sample)
        {
            var samplePath = Path.Combine(SampleRegistry.SamplesRootPath, sample.SampleName);

            if (Directory.Exists(samplePath))
            {
                if (!EditorUtility.DisplayDialog(
                        $"Generate {sample.DisplayName}",
                        $"Sample folder already exists:\n{samplePath}\n\nOverwrite?",
                        "Overwrite",
                        "Cancel"))
                    return;

                sample.Clean();
            }

            var result = sample.Generate();

            if (result.IsSuccess)
            {
                EditorUtility.DisplayDialog(
                    $"Generate {sample.DisplayName} - Success",
                    "Successfully generated sample.\n\n" +
                    $"Path: {result.SamplePath}\n" +
                    $"Files created: {result.CreatedFiles.Count}",
                    "OK"
                );

                var folder = AssetDatabase.LoadAssetAtPath<Object>(result.SamplePath);
                if (folder != null)
                {
                    Selection.activeObject = folder;
                    EditorGUIUtility.PingObject(folder);
                }
            }
            else
            {
                var errorMessage = string.Join("\n", result.Errors);
                EditorUtility.DisplayDialog(
                    $"Generate {sample.DisplayName} - Error",
                    $"Failed to generate sample:\n{errorMessage}",
                    "OK"
                );
            }
        }

#if AUDIOCONDUCTOR_DEVELOPER
        private static bool DeploySamples(SampleScanner.ScanResult scanResult)
        {
            var samplesDestinationPath = Path.Combine(PackagePath, SamplesDestinationFolder);
            var deployedSamples = new List<PackageManifestUpdater.SampleInfo>();
            var totalCopiedFiles = 0;

            foreach (var sampleFolder in scanResult.SampleFolders)
            {
                var destinationPath = Path.Combine(samplesDestinationPath, sampleFolder.Name);
                var deployResult = SampleDeployer.Deploy(sampleFolder.FullPath, destinationPath);

                if (!deployResult.IsSuccess)
                {
                    var errorMessage = string.Join("\n", deployResult.Errors);
                    EditorUtility.DisplayDialog(
                        "Deploy Samples - Error",
                        $"Failed to deploy '{sampleFolder.Name}':\n{errorMessage}",
                        "OK"
                    );
                    return false;
                }

                totalCopiedFiles += deployResult.CopiedFileCount;

                var displayName = FormatDisplayName(sampleFolder.Name);
                var description = $"Sample demonstrating {displayName} features";
                var path = $"Samples~/{sampleFolder.Name}";

                deployedSamples.Add(new PackageManifestUpdater.SampleInfo(displayName, description, path));
            }

            var packageJsonPath = Path.Combine(PackagePath, "package.json");
            var updateResult = PackageManifestUpdater.UpdateSamples(packageJsonPath, deployedSamples);

            if (!updateResult.IsSuccess)
            {
                var errorMessage = string.Join("\n", updateResult.Errors);
                EditorUtility.DisplayDialog(
                    "Deploy Samples - Error",
                    $"Failed to update package.json:\n{errorMessage}",
                    "OK"
                );
                return false;
            }

            EditorUtility.DisplayDialog(
                "Deploy Samples - Success",
                $"Successfully deployed {deployedSamples.Count} sample(s).\n" +
                $"Total files copied: {totalCopiedFiles}",
                "OK"
            );

            return true;
        }
#endif

        private static string FormatDisplayName(string folderName)
        {
            return Regex.Replace(folderName, "([a-z])([A-Z])", "$1 $2");
        }
    }
}
