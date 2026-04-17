// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#if AUDIOCONDUCTOR_DEVELOPER && AUDIOCONDUCTOR_NEWTONSOFT_JSON

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudioConductor.Editor.Core.Tools.SamplesDeployment
{
    /// <summary>
    ///     Updates the samples array in package.json.
    /// </summary>
    internal static class PackageManifestUpdater
    {
        /// <summary>
        ///     Updates the samples array in package.json.
        /// </summary>
        /// <param name="packageJsonPath">Path to package.json.</param>
        /// <param name="samples">List of sample information.</param>
        /// <returns>The update result.</returns>
        internal static UpdateResult UpdateSamples(string packageJsonPath, IReadOnlyList<SampleInfo> samples)
        {
            var result = new UpdateResult();

            if (!File.Exists(packageJsonPath))
            {
                result.IsSuccess = false;
                result.Errors.Add($"package.json does not exist: {packageJsonPath}");
                return result;
            }

            try
            {
                var jsonText = File.ReadAllText(packageJsonPath);
                var packageJson = JObject.Parse(jsonText);

                var samplesArray = new JArray();
                foreach (var sample in samples)
                {
                    var sampleObject = new JObject
                    {
                        ["displayName"] = sample.DisplayName,
                        ["description"] = sample.Description,
                        ["path"] = sample.Path
                    };
                    samplesArray.Add(sampleObject);
                }

                packageJson["samples"] = samplesArray;

                var formattedJson = packageJson.ToString(Formatting.Indented);
                File.WriteAllText(packageJsonPath, formattedJson);

                result.IsSuccess = true;
                result.UpdatedSamplesCount = samples.Count;
            }
            catch (JsonException ex)
            {
                result.IsSuccess = false;
                result.Errors.Add($"Invalid JSON format: {ex.Message}");
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Errors.Add($"Error updating package.json: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        ///     Holds the result of an update operation.
        /// </summary>
        internal class UpdateResult
        {
            /// <summary>
            ///     Whether the update succeeded.
            /// </summary>
            internal bool IsSuccess { get; set; }

            /// <summary>
            ///     Number of samples updated.
            /// </summary>
            internal int UpdatedSamplesCount { get; set; }

            /// <summary>
            ///     List of error messages.
            /// </summary>
            internal List<string> Errors { get; } = new();
        }

        /// <summary>
        ///     Holds information about a sample.
        /// </summary>
        internal class SampleInfo
        {
            internal SampleInfo(string displayName, string description, string path)
            {
                DisplayName = displayName;
                Description = description;
                Path = path;
            }

            /// <summary>
            ///     Display name shown in Package Manager.
            /// </summary>
            internal string DisplayName { get; }

            /// <summary>
            ///     Description of the sample.
            /// </summary>
            internal string Description { get; }

            /// <summary>
            ///     Path in Samples~/[sample name] format.
            /// </summary>
            internal string Path { get; }
        }
    }
}

#endif
