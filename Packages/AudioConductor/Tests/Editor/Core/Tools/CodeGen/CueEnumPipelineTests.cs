// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.IO;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumPipelineTests
    {
        // Use the system temp directory instead of Assets/ to avoid triggering a domain reload
        // via Unity's file system watcher during interactive test execution.
        private static readonly string RootFolder =
            Path.Combine(Path.GetTempPath(), "AudioConductorTests", nameof(CueEnumPipelineTests));

        private readonly List<Object> _created = new();

        [SetUp]
        public void SetUp()
        {
            if (Directory.Exists(RootFolder))
                Directory.Delete(RootFolder, true);
            Directory.CreateDirectory(RootFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(RootFolder))
                Directory.Delete(RootFolder, true);

            foreach (var obj in _created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            _created.Clear();
        }

        [Test]
        public void Execute_SingleRootEntry_GeneratesFile()
        {
            var def = CreateDefinition();
            def.defaultOutputPath = RootFolder;
            var asset = CreateAsset("BGM", new Cue { name = "title", cueId = 1 });
            def.rootEntries.Add(asset);

            var result = CueEnumPipeline.Execute(def, false);

            Assert.That(result.Success, Is.True);
            Assert.That(result.GeneratedCount, Is.EqualTo(1));
            Assert.That(result.WrittenCount, Is.EqualTo(1));
            Assert.That(File.Exists(Path.Combine(RootFolder, "BGM.cs")), Is.True);
        }

        [Test]
        public void Execute_SameContentTwice_SecondRunUpToDate()
        {
            var def = CreateDefinition();
            def.defaultOutputPath = RootFolder;
            def.rootEntries.Add(CreateAsset("BGM", new Cue { name = "title", cueId = 1 }));

            CueEnumPipeline.Execute(def, false);
            var result = CueEnumPipeline.Execute(def, false);

            Assert.That(result.Success, Is.True);
            Assert.That(result.WrittenCount, Is.EqualTo(0));
            Assert.That(result.UpToDateCount, Is.EqualTo(1));
        }

        [Test]
        public void Execute_FileEntry_GeneratesMergedFile()
        {
            var def = CreateDefinition();
            def.defaultOutputPath = RootFolder;
            def.fileEntries.Add(new FileEntry
            {
                fileName = "AudioEnums",
                assets =
                {
                    CreateAsset("BGM", new Cue { name = "title", cueId = 1 }),
                    CreateAsset("SE", new Cue { name = "click", cueId = 1 })
                }
            });

            var result = CueEnumPipeline.Execute(def, false);

            Assert.That(result.Success, Is.True);
            Assert.That(result.WrittenCount, Is.EqualTo(1));
            var content = File.ReadAllText(Path.Combine(RootFolder, "AudioEnums.cs"));
            Assert.That(content, Does.Contain("public enum BGM"));
            Assert.That(content, Does.Contain("public enum SE"));
        }

        [Test]
        public void Execute_EmptyDefinition_SucceedsWithZeroCounts()
        {
            var def = CreateDefinition();

            var result = CueEnumPipeline.Execute(def, false);

            Assert.That(result.Success, Is.True);
            Assert.That(result.GeneratedCount, Is.EqualTo(0));
            Assert.That(result.WrittenCount, Is.EqualTo(0));
        }

        [Test]
        public void Execute_ValidationError_ReturnsFailure()
        {
            var def = CreateDefinition();
            def.defaultOutputPath = RootFolder;
            def.defaultClassSuffix = "";
            // Two root entries that produce the same output file path
            var asset1 = CreateAsset("BGM", new Cue { name = "title", cueId = 1 });
            var asset2 = CreateAsset("BGM", new Cue { name = "battle", cueId = 2 });
            def.rootEntries.Add(asset1);
            def.rootEntries.Add(asset2);

            var result = CueEnumPipeline.Execute(def, false);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Execute_GenerationError_ReturnsFailureWithErrors()
        {
            var def = CreateDefinition();
            def.defaultOutputPath = RootFolder;
            // Asset with duplicate cue names → generation failure
            var asset = CreateAsset("BGM", new Cue { name = "dup", cueId = 1 },
                new Cue { name = "dup", cueId = 2 });
            def.rootEntries.Add(asset);

            var result = CueEnumPipeline.Execute(def, false);

            Assert.That(result.Success, Is.False);
            Assert.That(result.Errors, Has.Some.Contain("Duplicate cue name"));
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
