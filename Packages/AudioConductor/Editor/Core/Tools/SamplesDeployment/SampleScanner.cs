// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.IO;

namespace AudioConductor.Editor.Core.Tools.SamplesDeployment
{
    /// <summary>
    ///     Scans the Assets/Samples folder to detect samples.
    /// </summary>
    internal static class SampleScanner
    {
        /// <summary>
        ///     Scans the specified path to detect sample folders.
        /// </summary>
        /// <param name="samplesRootPath">The root path of samples (e.g. Assets/Samples).</param>
        /// <returns>The scan result.</returns>
        internal static ScanResult Scan(string samplesRootPath)
        {
            var result = new ScanResult();

            if (!Directory.Exists(samplesRootPath))
            {
                result.IsValid = false;
                result.Errors.Add($"Samples folder does not exist: {samplesRootPath}");
                return result;
            }

            var subFolders = Directory.GetDirectories(samplesRootPath);

            if (subFolders.Length == 0)
            {
                result.IsValid = false;
                result.Errors.Add("No sample folders found in Samples directory");
                return result;
            }

            foreach (var folder in subFolders)
            {
                var folderName = Path.GetFileName(folder);
                var asmdefFiles = Directory.GetFiles(folder, "*.asmdef");

                if (asmdefFiles.Length == 0)
                {
                    result.IsValid = false;
                    result.Errors.Add($"Sample folder '{folderName}' does not contain an .asmdef file");
                }
                else
                {
                    result.SampleFolders.Add(new SampleFolder(folder, folderName));
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        /// <summary>
        ///     Holds the result of a scan operation.
        /// </summary>
        internal class ScanResult
        {
            /// <summary>
            ///     Whether the scan is valid (no errors).
            /// </summary>
            internal bool IsValid { get; set; } = true;

            /// <summary>
            ///     List of detected sample folders.
            /// </summary>
            internal List<SampleFolder> SampleFolders { get; } = new();

            /// <summary>
            ///     List of error messages.
            /// </summary>
            internal List<string> Errors { get; } = new();
        }

        /// <summary>
        ///     Holds information about a sample folder.
        /// </summary>
        internal class SampleFolder
        {
            internal SampleFolder(string fullPath, string name)
            {
                FullPath = fullPath;
                Name = name;
            }

            /// <summary>
            ///     Full path of the folder.
            /// </summary>
            internal string FullPath { get; }

            /// <summary>
            ///     Folder name.
            /// </summary>
            internal string Name { get; }
        }
    }
}
