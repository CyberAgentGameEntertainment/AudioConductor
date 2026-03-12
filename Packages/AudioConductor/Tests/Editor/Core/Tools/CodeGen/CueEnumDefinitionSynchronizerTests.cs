// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumDefinitionSynchronizerTests
    {
        private readonly List<Object> _created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void IsContainedInDefinition_InRootEntries_ReturnsTrue()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("BGM");
            def.rootEntries.Add(asset);

            Assert.That(CueEnumDefinitionSynchronizer.IsContainedInDefinition(def, asset), Is.True);
        }

        [Test]
        public void IsContainedInDefinition_InFileEntry_ReturnsTrue()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("BGM");
            var fe = new FileEntry { fileName = "Audio", assets = { asset } };
            def.fileEntries.Add(fe);

            Assert.That(CueEnumDefinitionSynchronizer.IsContainedInDefinition(def, asset), Is.True);
        }

        [Test]
        public void IsContainedInDefinition_NotInDefinition_ReturnsFalse()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("BGM");

            Assert.That(CueEnumDefinitionSynchronizer.IsContainedInDefinition(def, asset), Is.False);
        }

        [Test]
        public void MatchesPathRule_SimpleWildcard()
        {
            Assert.That(CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Audio/BGM.asset", "Assets/Audio/*.asset"),
                Is.True);
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Other/BGM.asset", "Assets/Audio/*.asset"),
                Is.False);
        }

        [Test]
        public void MatchesPathRule_DoubleWildcard()
        {
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Audio/Sub/BGM.asset", "Assets/Audio/**/*.asset"),
                Is.True);
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Audio/BGM.asset", "Assets/Audio/**/*.asset"),
                Is.True);
        }

        [Test]
        public void MatchesPathRule_SingleWildcardDirectory()
        {
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Auto/NewCueSheet.asset", "*/Auto/*"),
                Is.True);
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Other/NewCueSheet.asset", "*/Auto/*"),
                Is.False);
        }

        [Test]
        public void MatchesPathRule_EmptyPattern_ReturnsFalse()
        {
            Assert.That(CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Audio/BGM.asset", ""), Is.False);
        }

        [Test]
        public void IsContainedInDefinition_InExcludedEntries_ReturnsFalse()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("Debug_SFX");
            def.excludedEntries.Add(asset);

            Assert.That(CueEnumDefinitionSynchronizer.IsContainedInDefinition(def, asset), Is.False);
        }

        [Test]
        public void ExcludedAsset_NotReaddedOnImport()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("Debug_SFX");
            def.excludedEntries.Add(asset);

            // Simulate: excludedEntries contains the asset, so it should not be added to rootEntries
            Assert.That(def.excludedEntries.Contains(asset), Is.True);
            Assert.That(def.rootEntries, Has.No.Member(asset));
            Assert.That(CueEnumDefinitionSynchronizer.IsContainedInDefinition(def, asset), Is.False);
        }

        [Test]
        public void ExcludePathRule_MatchingAsset_MatchesPathRuleReturnsTrue()
        {
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Debug/SFX.asset", "*/Debug/*"),
                Is.True);
        }

        [Test]
        public void ExcludePathRule_NonMatchingAsset_MatchesPathRuleReturnsFalse()
        {
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Audio/BGM.asset", "*/Debug/*"),
                Is.False);
        }

        [Test]
        public void ExcludePathRule_Empty_MatchesPathRuleReturnsFalse()
        {
            Assert.That(
                CueEnumDefinitionSynchronizer.MatchesPathRule("Assets/Debug/SFX.asset", ""),
                Is.False);
        }

        [Test]
        public void ExcludedEntries_NullCleanup()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("Test_BGM");
            def.excludedEntries.Add(asset);
            def.excludedEntries.Add(null!);

            var before = def.excludedEntries.Count;
            def.excludedEntries.RemoveAll(a => a == null);

            Assert.That(def.excludedEntries.Count, Is.EqualTo(before - 1));
            Assert.That(def.excludedEntries, Has.Member(asset));
        }

        private CueEnumDefinition CreateDefinition()
        {
            var def = ScriptableObject.CreateInstance<CueEnumDefinition>();
            _created.Add(def);
            return def;
        }

        private CueSheetAsset CreateAsset(string cueSheetName)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.name = cueSheetName;
            _created.Add(asset);
            return asset;
        }
    }
}
