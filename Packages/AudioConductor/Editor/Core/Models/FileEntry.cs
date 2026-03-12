// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Models
{
    /// <summary>
    ///     Groups multiple CueSheetAssets into a single generated file.
    /// </summary>
    [Serializable]
    internal sealed class FileEntry
    {
        public string fileName = string.Empty;
        public string outputPath = string.Empty;
        public string @namespace = string.Empty;
        public string classSuffix = string.Empty;
        public bool useDefaultOutputPath = true;
        public bool useDefaultNamespace = true;
        public bool useDefaultClassSuffix = true;

        /// <summary>
        ///     Glob pattern for automatic CueSheetAsset collection. Empty means manual assignment only.
        /// </summary>
        public string pathRule = string.Empty;

        /// <summary>
        ///     CueSheetAssets assigned to this group (manually and/or via path rule).
        /// </summary>
        public List<CueSheetAsset> assets = new();
    }
}
