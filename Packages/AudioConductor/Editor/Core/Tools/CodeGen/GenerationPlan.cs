// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Intermediate representation for enum code generation.
    ///     Decouples EnumName resolution from the Generator.
    /// </summary>
    internal readonly struct GenerationPlan
    {
        /// <summary>
        ///     Full output file path (directory + fileName + ".cs").
        /// </summary>
        internal string OutputFilePath { get; }

        /// <summary>
        ///     Namespace for the generated file. Empty means no namespace.
        /// </summary>
        internal string Namespace { get; }

        /// <summary>
        ///     One or more enum definitions to generate in a single file.
        /// </summary>
        internal IReadOnlyList<EnumEntry> Entries { get; }

        internal GenerationPlan(string outputFilePath, string @namespace, IReadOnlyList<EnumEntry> entries)
        {
            OutputFilePath = outputFilePath;
            Namespace = @namespace;
            Entries = entries;
        }
    }

    /// <summary>
    ///     A single enum definition within a <see cref="GenerationPlan" />.
    /// </summary>
    internal readonly struct EnumEntry
    {
        /// <summary>
        ///     PascalCase enum type name (e.g. "BGMAudioIds").
        /// </summary>
        internal string EnumName { get; }

        /// <summary>
        ///     Original CueSheet name for the Extensions class (e.g. "BGM").
        /// </summary>
        internal string BaseName { get; }

        /// <summary>
        ///     Source CueSheetAsset.
        /// </summary>
        internal CueSheetAsset Asset { get; }

        internal EnumEntry(string enumName, string baseName, CueSheetAsset asset)
        {
            EnumName = enumName;
            BaseName = baseName;
            Asset = asset;
        }
    }
}
