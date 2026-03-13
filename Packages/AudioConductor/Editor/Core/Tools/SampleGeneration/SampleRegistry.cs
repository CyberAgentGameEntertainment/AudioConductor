// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;

namespace AudioConductor.Editor.SampleGeneration
{
    /// <summary>
    ///     Provides search and enumeration of registered samples.
    /// </summary>
    internal static class SampleRegistry
    {
        internal const string SamplesRootPath = "Assets/Samples";

        // AudioConductorSample will be registered in Commit 4
        private static readonly ISample[] Samples = Array.Empty<ISample>();

        /// <summary>
        ///     All registered samples.
        /// </summary>
        internal static IReadOnlyList<ISample> All => Samples;

        /// <summary>
        ///     Finds a sample by name.
        /// </summary>
        internal static ISample? FindByName(string sampleName)
        {
            return Array.Find(Samples, s => s.SampleName == sampleName);
        }
    }
}
