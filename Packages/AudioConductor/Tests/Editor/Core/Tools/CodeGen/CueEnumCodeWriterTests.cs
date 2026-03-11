// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tests;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumCodeWriterTests
    {
        private const string RootFolder = "Assets/gen/" + nameof(CueEnumCodeWriterTests);
        private CueSheetAsset _asset = null!;
        private AudioConductorEditorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            Utility.CreateFolderRecursively(RootFolder);

            _settings = ScriptableObject.CreateInstance<AudioConductorEditorSettings>();
            _settings.defaultCodeGenOutputPath = RootFolder + "/ProjectDefault";
            AssetDatabase.CreateAsset(_settings, RootFolder + "/AudioConductorEditorSettings.asset");

            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _asset.cueSheet.name = "BGM";
            _asset.cueSheet.cueList.Add(new Cue
            {
                name = "Title",
                cueId = 7
            });
            _asset.codeGenOutputPath = RootFolder;
            AssetDatabase.CreateAsset(_asset, RootFolder + "/TestCueSheet.asset");
            AssetDatabase.Refresh();
            _asset.useDefaultCodeGenOutputPath = false;
            _asset.useDefaultCodeGenNamespace = false;
            _asset.useDefaultCodeGenClassSuffix = false;
            EditorUtility.SetDirty(_asset);
            AssetDatabase.SaveAssets();
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            if (_settings != null)
                Object.DestroyImmediate(_settings, true);
            if (_asset != null)
                Object.DestroyImmediate(_asset, true);
        }

        [Test]
        public void Write_FirstTime_WritesFile()
        {
            var result = CueEnumCodeWriter.Write(_asset);

            Assert.That(result.Success, Is.True);
            Assert.That(result.WroteFile, Is.True);
            Assert.That(File.Exists(Path.Combine(RootFolder, "BGM.cs")), Is.True);
            Assert.That(Directory.GetFiles(RootFolder, "*.tmp"), Is.Empty);
        }

        [Test]
        public void Write_SameContent_DoesNotRewrite()
        {
            CueEnumCodeWriter.Write(_asset);

            var result = CueEnumCodeWriter.Write(_asset);

            Assert.That(result.Success, Is.True);
            Assert.That(result.WroteFile, Is.False);
        }

        [Test]
        public void Write_WhenUpdatingExistingFile_ReplacesContentWithoutLeavingTempFile()
        {
            CueEnumCodeWriter.Write(_asset);
            _asset.cueSheet.cueList.Add(new Cue
            {
                name = "Battle",
                cueId = 9
            });

            var result = CueEnumCodeWriter.Write(_asset);
            var outputPath = Path.Combine(RootFolder, "BGM.cs");

            Assert.That(result.Success, Is.True);
            Assert.That(result.WroteFile, Is.True);
            Assert.That(File.ReadAllText(outputPath), Does.Contain("Battle = 9,"));
            Assert.That(Directory.GetFiles(RootFolder, "*.tmp"), Is.Empty);
        }

        [Test]
        public void Write_UseDefaultOutputPath_WritesToEditorSettingsPath()
        {
            _asset.useDefaultCodeGenOutputPath = true;

            var result = CueEnumCodeWriter.Write(_asset);

            Assert.That(result.Success, Is.True);
            Assert.That(result.OutputPath, Is.EqualTo(RootFolder + "/ProjectDefault/BGM.cs"));
            Assert.That(File.Exists(RootFolder + "/ProjectDefault/BGM.cs"), Is.True);
        }
    }
}
