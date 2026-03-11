// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    [InitializeOnLoad]
    internal static class CueEnumDefaultsSettingsChangeProcessor
    {
        private static bool _isProcessing;

        static CueEnumDefaultsSettingsChangeProcessor()
        {
            AudioConductorEditorSettingsRepository.SettingsChanged += OnSettingsChanged;
        }

        private static void OnSettingsChanged()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            try
            {
                RegenerateAffectedCueEnums(LoadAllCueSheetAssets());
            }
            finally
            {
                _isProcessing = false;
            }
        }

        internal static CueEnumBatchGenerator.BatchResult RegenerateAffectedCueEnums(IEnumerable<CueSheetAsset?> assets)
        {
            var affectedAssets = CollectAffectedCueSheets(assets);
            return CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, affectedAssets);
        }

        internal static CueSheetAsset[] CollectAffectedCueSheets(IEnumerable<CueSheetAsset?> assets)
        {
            return assets.Where(IsAffectedByProjectDefaults).Cast<CueSheetAsset>().ToArray();
        }

        private static bool IsAffectedByProjectDefaults(CueSheetAsset? asset)
        {
            return asset != null
                   && asset is { codeGenEnabled: true, codeGenMode: CueSheetCodeGenMode.OnSave }
                   && (asset.useDefaultCodeGenOutputPath
                       || asset.useDefaultCodeGenNamespace
                       || asset.useDefaultCodeGenClassSuffix);
        }

        private static CueSheetAsset?[] LoadAllCueSheetAssets()
        {
            return AssetDatabase.FindAssets("t:" + nameof(CueSheetAsset), new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<CueSheetAsset>)
                .ToArray();
        }
    }
}
