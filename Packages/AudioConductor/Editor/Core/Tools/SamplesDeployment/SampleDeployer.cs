// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;

namespace AudioConductor.Editor.SamplesDeployment
{
    /// <summary>
    ///     Deploys a sample folder to the package's Samples~ folder.
    /// </summary>
    internal static class SampleDeployer
    {
        /// <summary>
        ///     Deploys samples from the source path to the destination path.
        /// </summary>
        /// <param name="sourcePath">The path to copy from.</param>
        /// <param name="destinationPath">The path to copy to.</param>
        /// <returns>The deploy result.</returns>
        internal static DeployResult Deploy(string sourcePath, string destinationPath)
        {
            var result = new DeployResult();

            if (!Directory.Exists(sourcePath))
            {
                result.IsSuccess = false;
                result.Errors.Add($"Source path does not exist: {sourcePath}");
                return result;
            }

            try
            {
                if (Directory.Exists(destinationPath))
                    Directory.Delete(destinationPath, true);

                Directory.CreateDirectory(destinationPath);

                var allFiles = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);

                foreach (var sourceFile in allFiles)
                {
                    var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
                    var destFile = Path.Combine(destinationPath, relativePath);

                    var destDir = Path.GetDirectoryName(destFile);
                    if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                        Directory.CreateDirectory(destDir);

                    File.Copy(sourceFile, destFile, true);
                    result.CopiedFiles.Add(relativePath);
                }

                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Errors.Add($"Error during deployment: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        ///     Holds the result of a deploy operation.
        /// </summary>
        internal class DeployResult
        {
            /// <summary>
            ///     Whether the deploy succeeded.
            /// </summary>
            internal bool IsSuccess { get; set; }

            /// <summary>
            ///     List of copied files (relative paths).
            /// </summary>
            internal List<string> CopiedFiles { get; } = new();

            /// <summary>
            ///     Number of copied files.
            /// </summary>
            internal int CopiedFileCount => CopiedFiles.Count;

            /// <summary>
            ///     List of error messages.
            /// </summary>
            internal List<string> Errors { get; } = new();
        }
    }
}
