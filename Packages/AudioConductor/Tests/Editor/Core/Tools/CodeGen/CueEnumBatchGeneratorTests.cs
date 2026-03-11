// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.IO;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumBatchGeneratorTests
    {
        private const string RootFolder = "Assets/gen/CueEnumBatchGeneratorTests";
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
            {
                if (asset != null)
                    Object.DestroyImmediate(asset, true);
            }

            _assets.Clear();
        }

        [Test]
        public void Generate_EnabledOnly_ProcessesOnlyEnabledAssets()
        {
            var assets = new[]
            {
                CreateCueSheetAsset("01_Enabled", true, cueId: 1),
                CreateCueSheetAsset("02_Disabled", false, cueId: 2)
            };

            var result = CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, assets);

            Assert.That(result.Success, Is.True);
            Assert.That(result.TargetCount, Is.EqualTo(1));
            Assert.That(result.ProcessedCount, Is.EqualTo(1));
            Assert.That(result.WrittenCount, Is.EqualTo(1));
        }

        [Test]
        public void Generate_All_ProcessesEnabledAndDisabledAssets()
        {
            var assets = new[]
            {
                CreateCueSheetAsset("01_Enabled", true, cueId: 1),
                CreateCueSheetAsset("02_Disabled", false, cueId: 2)
            };

            var result = CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.All, assets);

            Assert.That(result.Success, Is.True);
            Assert.That(result.TargetCount, Is.EqualTo(2));
            Assert.That(result.WrittenCount, Is.EqualTo(2));
        }

        [Test]
        public void Generate_WhenNoTargets_ReturnsSuccess()
        {
            var result = CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.All,
                new CueSheetAsset?[] { });

            Assert.That(result.Success, Is.True);
            Assert.That(result.TargetCount, Is.EqualTo(0));
            Assert.That(result.ProcessedCount, Is.EqualTo(0));
            Assert.That(result.WrittenCount, Is.EqualTo(0));
            Assert.That(result.UpToDateCount, Is.EqualTo(0));
            Assert.That(result.Failures, Is.Empty);
        }

        [Test]
        public void Generate_WhenFailureOccurs_ContinuesAndCollectsFailure()
        {
            var assets = new[]
            {
                CreateCueSheetAsset("01_Valid", true, cueId: 1),
                CreateCueSheetAsset("02_Invalid", true, duplicateCueNames: true),
                CreateCueSheetAsset("03_Valid", true, cueId: 3)
            };

            var result = CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, assets);

            Assert.That(result.Success, Is.False);
            Assert.That(result.TargetCount, Is.EqualTo(3));
            Assert.That(result.ProcessedCount, Is.EqualTo(3));
            Assert.That(result.WrittenCount, Is.EqualTo(2));
            Assert.That(result.FailedCount, Is.EqualTo(1));
            Assert.That(result.Failures[0].AssetPath, Does.Contain("02_Invalid.asset"));
        }

        [Test]
        public void Generate_WhenFilesAlreadyExist_CountsAsUpToDate()
        {
            var assets = new[]
            {
                CreateCueSheetAsset("01_Enabled", true, cueId: 1)
            };
            CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, assets);

            var result = CueEnumBatchGenerator.Generate(CueEnumBatchGenerator.CueEnumBatchScope.EnabledOnly, assets);

            Assert.That(result.Success, Is.True);
            Assert.That(result.TargetCount, Is.EqualTo(1));
            Assert.That(result.WrittenCount, Is.EqualTo(0));
            Assert.That(result.UpToDateCount, Is.EqualTo(1));
        }

        private CueSheetAsset CreateCueSheetAsset(string name, bool codeGenEnabled, int cueId = 1,
            bool duplicateCueNames = false)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.name = name;
            asset.codeGenEnabled = codeGenEnabled;
            asset.codeGenOutputPath = RootFolder;
            asset.cueSheet.cueList.Add(new Cue
            {
                name = "Title",
                cueId = cueId
            });

            if (duplicateCueNames)
            {
                asset.cueSheet.cueList.Add(new Cue
                {
                    name = "Title",
                    cueId = cueId + 1
                });
            }

            AssetDatabase.CreateAsset(asset, $"{RootFolder}/{name}.asset");
            AssetDatabase.Refresh();
            _assets.Add(asset);
            return asset;
        }
    }
}
