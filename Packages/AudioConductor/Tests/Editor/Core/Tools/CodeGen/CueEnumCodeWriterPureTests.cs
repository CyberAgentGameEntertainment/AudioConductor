// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using AudioConductor.Editor.Core.Tests;
using NUnit.Framework;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal sealed class CueEnumCodeWriterPureTests
    {
        private const string RootFolder = "Assets/gen/" + nameof(CueEnumCodeWriterPureTests);

        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);
            Utility.CreateFolderRecursively(RootFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);
        }

        [Test]
        public void Write_NewFile_WritesAndReturnsTrue()
        {
            var outputPath = Path.Combine(RootFolder, "Test.cs");

            var wrote = CueEnumCodeWriter.Write(outputPath, "// test content");

            Assert.That(wrote, Is.True);
            Assert.That(File.Exists(outputPath), Is.True);
            Assert.That(File.ReadAllText(outputPath), Is.EqualTo("// test content"));
        }

        [Test]
        public void Write_SameContent_ReturnsFalse()
        {
            var outputPath = Path.Combine(RootFolder, "Test.cs");
            File.WriteAllText(outputPath, "// test content");

            var wrote = CueEnumCodeWriter.Write(outputPath, "// test content");

            Assert.That(wrote, Is.False);
        }

        [Test]
        public void Write_DifferentContent_OverwritesAndReturnsTrue()
        {
            var outputPath = Path.Combine(RootFolder, "Test.cs");
            File.WriteAllText(outputPath, "// old content");

            var wrote = CueEnumCodeWriter.Write(outputPath, "// new content");

            Assert.That(wrote, Is.True);
            Assert.That(File.ReadAllText(outputPath), Is.EqualTo("// new content"));
        }

        [Test]
        public void Write_CreatesDirectoryIfNotExists()
        {
            var outputPath = Path.Combine(RootFolder, "SubDir", "Test.cs");

            var wrote = CueEnumCodeWriter.Write(outputPath, "// test");

            Assert.That(wrote, Is.True);
            Assert.That(File.Exists(outputPath), Is.True);
        }

        [Test]
        public void Write_NoTempFileLeftBehind()
        {
            var outputPath = Path.Combine(RootFolder, "Test.cs");

            CueEnumCodeWriter.Write(outputPath, "// test");

            Assert.That(Directory.GetFiles(RootFolder, "*.tmp"), Is.Empty);
        }
    }
}
