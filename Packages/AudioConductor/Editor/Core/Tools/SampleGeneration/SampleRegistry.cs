// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;

namespace AudioConductor.Editor.Core.Tools.SampleGeneration
{
    /// <summary>
    ///     Provides search and enumeration of registered samples.
    /// </summary>
    internal static class SampleRegistry
    {
        internal const string SamplesRootPath = "Assets/Samples";

        private static readonly ISample[] Samples =
        {
            new AudioConductorSample()
        };

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
