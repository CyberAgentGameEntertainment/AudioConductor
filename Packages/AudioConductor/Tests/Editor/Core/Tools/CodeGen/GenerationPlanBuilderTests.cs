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
    internal sealed class GenerationPlanBuilderTests
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
        public void Build_RootEntry_ProducesIndividualPlan()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("BGM");
            def.rootEntries.Add(asset);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans, Has.Count.EqualTo(1));
            Assert.That(plans[0].OutputFilePath, Is.EqualTo("Assets/Scripts/Generated/BGM.cs"));
            Assert.That(plans[0].Entries.Count, Is.EqualTo(1));
            Assert.That(plans[0].Entries[0].EnumName, Is.EqualTo("BGM"));
            Assert.That(plans[0].Entries[0].BaseName, Is.EqualTo("BGM"));
        }

        [Test]
        public void Build_RootEntry_UsesDefaultNamespace()
        {
            var def = CreateDefinition();
            def.defaultNamespace = "Game.Audio";
            def.rootEntries.Add(CreateAsset("BGM"));

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans[0].Namespace, Is.EqualTo("Game.Audio"));
        }

        [Test]
        public void Build_RootEntry_UsesDefaultClassSuffix()
        {
            var def = CreateDefinition();
            def.defaultClassSuffix = "Ids";
            def.rootEntries.Add(CreateAsset("BGM"));

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans[0].Entries[0].EnumName, Is.EqualTo("BGMIds"));
            Assert.That(plans[0].OutputFilePath, Does.EndWith("BGMIds.cs"));
        }

        [Test]
        public void Build_RootEntry_SkipsNullAsset()
        {
            var def = CreateDefinition();
            def.rootEntries.Add(null!);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans, Is.Empty);
        }

        [Test]
        public void Build_FileEntry_ProducesMergedPlan()
        {
            var def = CreateDefinition();
            var fe = new FileEntry
            {
                fileName = "AudioEnums",
                assets = { CreateAsset("BGM"), CreateAsset("SE") }
            };
            def.fileEntries.Add(fe);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans, Has.Count.EqualTo(1));
            Assert.That(plans[0].OutputFilePath, Is.EqualTo("Assets/Scripts/Generated/AudioEnums.cs"));
            Assert.That(plans[0].Entries.Count, Is.EqualTo(2));
            Assert.That(plans[0].Entries[0].EnumName, Is.EqualTo("BGM"));
            Assert.That(plans[0].Entries[1].EnumName, Is.EqualTo("SE"));
        }

        [Test]
        public void Build_FileEntry_UsesOwnSettings()
        {
            var def = CreateDefinition();
            def.defaultNamespace = "Default.NS";
            var fe = new FileEntry
            {
                fileName = "AudioEnums",
                useDefaultOutputPath = false,
                outputPath = "Assets/Custom/",
                useDefaultNamespace = false,
                @namespace = "Custom.NS",
                useDefaultClassSuffix = false,
                classSuffix = "Enum",
                assets = { CreateAsset("BGM") }
            };
            def.fileEntries.Add(fe);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans[0].OutputFilePath, Is.EqualTo("Assets/Custom/AudioEnums.cs"));
            Assert.That(plans[0].Namespace, Is.EqualTo("Custom.NS"));
            Assert.That(plans[0].Entries[0].EnumName, Is.EqualTo("BGMEnum"));
        }

        [Test]
        public void Build_FileEntry_UseDefaultFallsBackToDefinitionDefaults()
        {
            var def = CreateDefinition();
            def.defaultOutputPath = "Assets/Gen/";
            def.defaultNamespace = "My.NS";
            def.defaultClassSuffix = "Ids";
            var fe = new FileEntry
            {
                fileName = "AudioEnums",
                useDefaultOutputPath = true,
                useDefaultNamespace = true,
                useDefaultClassSuffix = true,
                assets = { CreateAsset("BGM") }
            };
            def.fileEntries.Add(fe);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans[0].OutputFilePath, Is.EqualTo("Assets/Gen/AudioEnums.cs"));
            Assert.That(plans[0].Namespace, Is.EqualTo("My.NS"));
            Assert.That(plans[0].Entries[0].EnumName, Is.EqualTo("BGMIds"));
        }

        [Test]
        public void Build_FileEntry_EmptyFileName_Skipped()
        {
            var def = CreateDefinition();
            var fe = new FileEntry
            {
                fileName = "",
                assets = { CreateAsset("BGM") }
            };
            def.fileEntries.Add(fe);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans, Is.Empty);
        }

        [Test]
        public void Build_FileEntry_SkipsNullAsset()
        {
            var def = CreateDefinition();
            var fe = new FileEntry
            {
                fileName = "AudioEnums",
                assets = { null!, CreateAsset("BGM") }
            };
            def.fileEntries.Add(fe);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans[0].Entries.Count, Is.EqualTo(1));
        }

        [Test]
        public void Build_FileEntry_AllNullAssets_NoPlan()
        {
            var def = CreateDefinition();
            var fe = new FileEntry
            {
                fileName = "AudioEnums",
                assets = { null! }
            };
            def.fileEntries.Add(fe);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans, Is.Empty);
        }

        [Test]
        public void Build_EmptyCueSheetName_FallsBackToAssetName()
        {
            var def = CreateDefinition();
            var asset = CreateAsset("");
            asset.name = "SampleSheet";
            def.rootEntries.Add(asset);

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans[0].Entries[0].EnumName, Is.EqualTo("SampleSheet"));
            Assert.That(plans[0].Entries[0].BaseName, Is.EqualTo("SampleSheet"));
        }

        [Test]
        public void Build_MixedRootAndFileEntries()
        {
            var def = CreateDefinition();
            def.rootEntries.Add(CreateAsset("BGM"));
            def.fileEntries.Add(new FileEntry
            {
                fileName = "SoundEffects",
                assets = { CreateAsset("SE_UI"), CreateAsset("SE_Battle") }
            });

            var plans = GenerationPlanBuilder.Build(def);

            Assert.That(plans, Has.Count.EqualTo(2));
            Assert.That(plans[0].Entries.Count, Is.EqualTo(1));
            Assert.That(plans[1].Entries.Count, Is.EqualTo(2));
        }

        [Test]
        public void Validate_NoDuplicates_ReturnsEmpty()
        {
            var plans = new[]
            {
                new GenerationPlan("Assets/A.cs", "", new[] { new EnumEntry("A", "A", CreateAsset("A")) }),
                new GenerationPlan("Assets/B.cs", "", new[] { new EnumEntry("B", "B", CreateAsset("B")) })
            };

            var errors = GenerationPlanBuilder.Validate(plans);

            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void Validate_DuplicateOutputPath_ReturnsError()
        {
            var plans = new[]
            {
                new GenerationPlan("Assets/Same.cs", "", new[] { new EnumEntry("A", "A", CreateAsset("A")) }),
                new GenerationPlan("Assets/Same.cs", "", new[] { new EnumEntry("B", "B", CreateAsset("B")) })
            };

            var errors = GenerationPlanBuilder.Validate(plans);

            Assert.That(errors, Has.Count.GreaterThan(0));
            Assert.That(errors[0], Does.Contain("Duplicate output file path"));
        }

        [Test]
        public void Validate_DuplicateEnumNameInMergedPlan_ReturnsError()
        {
            var plans = new[]
            {
                new GenerationPlan("Assets/Merged.cs", "",
                    new[]
                    {
                        new EnumEntry("Same", "A", CreateAsset("A")),
                        new EnumEntry("Same", "B", CreateAsset("B"))
                    })
            };

            var errors = GenerationPlanBuilder.Validate(plans);

            Assert.That(errors, Has.Count.GreaterThan(0));
            Assert.That(errors[0], Does.Contain("Duplicate EnumName"));
        }

        [Test]
        public void Validate_SingleEntryPlan_SkipsEnumNameCheck()
        {
            var plans = new[]
            {
                new GenerationPlan("Assets/A.cs", "", new[] { new EnumEntry("A", "A", CreateAsset("A")) })
            };

            var errors = GenerationPlanBuilder.Validate(plans);

            Assert.That(errors, Is.Empty);
        }

        private CueEnumDefinition CreateDefinition()
        {
            var def = ScriptableObject.CreateInstance<CueEnumDefinition>();
            _created.Add(def);
            return def;
        }

        private CueSheetAsset CreateAsset(string cueSheetName, params Cue[] cues)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.name = cueSheetName;
            foreach (var cue in cues)
                asset.cueSheet.cueList.Add(cue);
            _created.Add(asset);
            return asset;
        }
    }
}
