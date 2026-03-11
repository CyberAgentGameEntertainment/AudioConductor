// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.IO;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumDefaultsSettingsChangeProcessorTests
    {
        private const string RootFolder = "Assets/gen/CueEnumDefaultsSettingsChangeProcessorTests";
        private readonly List<CueSheetAsset> _assets = new();

        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            Directory.CreateDirectory(RootFolder);
            AssetDatabase.Refresh();
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder("Assets/gen"))
                AssetDatabase.DeleteAsset("Assets/gen");

            foreach (var asset in _assets)
                if (asset != null)
                    Object.DestroyImmediate(asset, true);

            _assets.Clear();
        }

        [Test]
        public void CollectAffectedCueSheets_FiltersToOnSaveAssetsUsingProjectDefaults()
        {
            var affected = CreateAsset("Affected", true, CueSheetCodeGenMode.OnSave, useDefaultNamespace: true);
            var manual = CreateAsset("Manual", true, CueSheetCodeGenMode.Manual, useDefaultNamespace: true);
            var explicitOnly = CreateAsset("ExplicitOnly", true, CueSheetCodeGenMode.OnSave,
                false, false, false);
            var disabled = CreateAsset("Disabled", false, CueSheetCodeGenMode.OnSave, useDefaultNamespace: true);

            var affectedAssets = CueEnumDefaultsSettingsChangeProcessor.CollectAffectedCueSheets(
                new[] { affected, manual, explicitOnly, disabled });

            Assert.That(affectedAssets, Has.Length.EqualTo(1));
            Assert.That(affectedAssets[0], Is.SameAs(affected));
        }

        private CueSheetAsset CreateAsset(string name, bool enabled, CueSheetCodeGenMode mode,
            bool useDefaultOutputPath = true, bool useDefaultNamespace = true, bool useDefaultClassSuffix = true)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.name = name;
            asset.codeGenEnabled = enabled;
            asset.codeGenMode = mode;
            asset.useDefaultCodeGenOutputPath = useDefaultOutputPath;
            asset.useDefaultCodeGenNamespace = useDefaultNamespace;
            asset.useDefaultCodeGenClassSuffix = useDefaultClassSuffix;
            asset.codeGenOutputPath = "Assets/TestGenerated";
            asset.cueSheet.cueList.Add(new Cue { name = "Title", cueId = 1 });
            AssetDatabase.CreateAsset(asset, $"{RootFolder}/{name}.asset");
            AssetDatabase.Refresh();
            _assets.Add(asset);
            return asset;
        }
    }
}
