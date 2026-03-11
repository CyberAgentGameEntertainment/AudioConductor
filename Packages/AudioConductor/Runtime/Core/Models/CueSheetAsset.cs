// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using UnityEngine;

namespace AudioConductor.Core.Models
{
    /// <summary>
    ///     Cue-sheet asset.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCueSheet",
        menuName = "Audio Conductor/" + nameof(CueSheetAsset),
        order = 2)]
    public sealed class CueSheetAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        private const string DefaultCodeGenOutputPath = "Assets/Scripts/Generated/";

        public CueSheet cueSheet = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(codeGenOutputPath)) codeGenOutputPath = DefaultCodeGenOutputPath;
            codeGenNamespace ??= string.Empty;
            codeGenClassSuffix ??= string.Empty;
#endif
        }

#if UNITY_EDITOR
        /// <summary>
        ///     Enables enum code generation for this cue sheet.
        /// </summary>
        public bool codeGenEnabled;

        /// <summary>
        ///     Trigger mode used when enum code generation is enabled.
        /// </summary>
        public CueSheetCodeGenMode codeGenMode = CueSheetCodeGenMode.Manual;

        /// <summary>
        ///     Output directory for the generated file.
        /// </summary>
        public string? codeGenOutputPath = DefaultCodeGenOutputPath;

        /// <summary>
        ///     Namespace for the generated enum. Empty means no namespace.
        /// </summary>
        public string? codeGenNamespace = string.Empty;

        /// <summary>
        ///     Suffix appended to the CueSheet name to form the enum type name. Empty means no suffix.
        /// </summary>
        public string? codeGenClassSuffix = string.Empty;
#endif
    }
}
