// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumCodeWriterTests
    {
        private const string RootFolder = "Assets/gen/CueEnumCodeWriterTests";
        private CueSheetAsset _asset = null!;

        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            Directory.CreateDirectory(RootFolder);

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
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder("Assets/gen"))
                AssetDatabase.DeleteAsset("Assets/gen");

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
        }

        [Test]
        public void Write_SameContent_DoesNotRewrite()
        {
            CueEnumCodeWriter.Write(_asset);

            var result = CueEnumCodeWriter.Write(_asset);

            Assert.That(result.Success, Is.True);
            Assert.That(result.WroteFile, Is.False);
        }
    }
}
