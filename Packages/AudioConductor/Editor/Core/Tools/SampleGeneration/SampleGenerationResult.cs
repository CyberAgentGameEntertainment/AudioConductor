// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Editor.SampleGeneration
{
    /// <summary>
    ///     Holds the result of a sample generation operation.
    /// </summary>
    internal class SampleGenerationResult
    {
        /// <summary>
        ///     Whether the generation succeeded.
        /// </summary>
        internal bool IsSuccess { get; set; }

        /// <summary>
        ///     Path of the generated sample.
        /// </summary>
        internal string? SamplePath { get; set; }

        /// <summary>
        ///     List of created files.
        /// </summary>
        internal List<string> CreatedFiles { get; } = new();

        /// <summary>
        ///     List of error messages.
        /// </summary>
        internal List<string> Errors { get; } = new();
    }
}
