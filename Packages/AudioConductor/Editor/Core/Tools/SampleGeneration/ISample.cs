// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.SampleGeneration
{
    /// <summary>
    ///     Defines a sample. Manages its own generation and deletion.
    /// </summary>
    internal interface ISample
    {
        /// <summary>
        ///     Sample name (used as folder name).
        /// </summary>
        string SampleName { get; }

        /// <summary>
        ///     Display name shown in menus.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     Generates the sample assets.
        /// </summary>
        SampleGenerationResult Generate();

        /// <summary>
        ///     Deletes the generated sample assets.
        /// </summary>
        void Clean();
    }
}
