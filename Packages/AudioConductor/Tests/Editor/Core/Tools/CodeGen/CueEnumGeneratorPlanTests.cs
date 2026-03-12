// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumGeneratorPlanTests
    {
        private readonly List<CueSheetAsset> _createdAssets = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var asset in _createdAssets)
                if (asset != null)
                    Object.DestroyImmediate(asset);
            _createdAssets.Clear();
        }

        [Test]
        public void GeneratePlan_SingleEntry_ProducesEnumAndExtensions()
        {
            var asset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 },
                new Cue { name = "battle", cueId = 2 });
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.Success, Is.True);
            Assert.That(result.SourceCode, Does.Contain("public enum BGM"));
            Assert.That(result.SourceCode, Does.Contain("Title = 1,"));
            Assert.That(result.SourceCode, Does.Contain("Battle = 2,"));
            Assert.That(result.SourceCode, Does.Contain("public static class BGMExtensions"));
        }

        [Test]
        public void GeneratePlan_MultipleEntries_ProducesAllEnumsInOneFile()
        {
            var bgmAsset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 });
            var seAsset = CreateAsset("SE", new Cue { name = "click", cueId = 1 });
            var plan = new GenerationPlan("Assets/Generated/AudioEnums.cs", "Game.Audio",
                new[]
                {
                    new EnumEntry("BGM", "BGM", bgmAsset),
                    new EnumEntry("SE", "SE", seAsset)
                });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.Success, Is.True);
            Assert.That(result.SourceCode, Does.Contain("namespace Game.Audio"));
            Assert.That(result.SourceCode, Does.Contain("public enum BGM"));
            Assert.That(result.SourceCode, Does.Contain("public enum SE"));
            Assert.That(result.SourceCode, Does.Contain("public static class BGMExtensions"));
            Assert.That(result.SourceCode, Does.Contain("public static class SEExtensions"));
        }

        [Test]
        public void GeneratePlan_WithNamespace_IndentsCorrectly()
        {
            var asset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 });
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "Game.Audio",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.SourceCode, Does.Contain("    public enum BGM"));
        }

        [Test]
        public void GeneratePlan_WithoutNamespace_NoIndent()
        {
            var asset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 });
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.SourceCode, Does.Not.Contain("namespace "));
            Assert.That(result.SourceCode, Does.Contain("public enum BGM"));
        }

        [Test]
        public void GeneratePlan_EmptyCueList_NoExtensions()
        {
            var asset = CreateAsset("BGM");
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.Success, Is.True);
            Assert.That(result.SourceCode, Does.Contain("public enum BGM"));
            Assert.That(result.SourceCode, Does.Not.Contain("Extensions"));
        }

        [Test]
        public void GeneratePlan_DuplicateEnumName_ReturnsFailure()
        {
            var asset1 = CreateAsset("BGM1", new Cue { name = "title", cueId = 1 });
            var asset2 = CreateAsset("BGM2", new Cue { name = "click", cueId = 1 });
            var plan = new GenerationPlan("Assets/Generated/Audio.cs", "",
                new[]
                {
                    new EnumEntry("BGM", "BGM1", asset1),
                    new EnumEntry("BGM", "BGM2", asset2)
                });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Has.Some.Contain("Duplicate EnumName"));
        }

        [Test]
        public void GeneratePlan_DuplicateCueName_ReturnsFailure()
        {
            var asset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 },
                new Cue { name = "title", cueId = 2 });
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Has.Some.Contain("Duplicate cue name"));
        }

        [Test]
        public void GeneratePlan_HasAutoGeneratedHeader()
        {
            var asset = CreateAsset("BGM");
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.SourceCode, Does.StartWith("// <auto-generated/>"));
        }

        [Test]
        public void GeneratePlan_WithCues_HasUsingSystem()
        {
            var asset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 });
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.SourceCode, Does.Contain("using System;"));
        }

        [Test]
        public void GeneratePlan_NoCues_NoUsingSystem()
        {
            var asset = CreateAsset("BGM");
            var plan = new GenerationPlan("Assets/Generated/BGM.cs", "",
                new[] { new EnumEntry("BGM", "BGM", asset) });

            var result = CueEnumGenerator.Generate(plan);

            Assert.That(result.SourceCode, Does.Not.Contain("using System;"));
        }

        private CueSheetAsset CreateAsset(string cueSheetName, params Cue[] cues)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.name = cueSheetName;
            foreach (var cue in cues)
                asset.cueSheet.cueList.Add(cue);
            _createdAssets.Add(asset);
            return asset;
        }
    }
}
