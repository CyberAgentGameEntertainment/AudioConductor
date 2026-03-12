// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using UnityEngine;

namespace AudioConductor.Editor.Core.Models
{
    /// <summary>
    ///     Definition file for cue enum code generation.
    ///     Centralizes codegen settings that were previously scattered across CueSheetAsset fields.
    /// </summary>
    internal sealed class CueEnumDefinition : ScriptableObject
    {
        public string defaultOutputPath = "Assets/Scripts/Generated/";
        public string defaultNamespace = string.Empty;
        public string defaultClassSuffix = string.Empty;

        /// <summary>
        ///     CueSheetAssets placed directly at root. Each generates an individual file using default settings.
        /// </summary>
        public List<CueSheetAsset> rootEntries = new();

        /// <summary>
        ///     Grouped entries. Each FileEntry generates a single file containing multiple enums.
        /// </summary>
        public List<FileEntry> fileEntries = new();
    }
}
