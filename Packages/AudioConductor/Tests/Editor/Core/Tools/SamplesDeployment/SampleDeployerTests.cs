// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.SamplesDeployment.Tests
{
    internal class SampleDeployerTests
    {
        private string _tempDir = string.Empty;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [Test]
        public void Deploy_CopiesFilesToDestination()
        {
            var sourceDir = Path.Combine(_tempDir, "source");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "content");

            var destDir = Path.Combine(_tempDir, "dest");

            var result = SampleDeployer.Deploy(sourceDir, destDir);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(File.Exists(Path.Combine(destDir, "test.txt")), Is.True);
            Assert.That(result.CopiedFileCount, Is.EqualTo(1));
            Assert.That(result.CopiedFiles, Contains.Item("test.txt"));
        }

        [Test]
        public void Deploy_CopiesMetaFiles()
        {
            var sourceDir = Path.Combine(_tempDir, "source");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "content");
            File.WriteAllText(Path.Combine(sourceDir, "test.txt.meta"), "meta");

            var destDir = Path.Combine(_tempDir, "dest");

            var result = SampleDeployer.Deploy(sourceDir, destDir);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.CopiedFileCount, Is.EqualTo(2));
            Assert.That(File.Exists(Path.Combine(destDir, "test.txt.meta")), Is.True);
        }

        [Test]
        public void Deploy_ReturnsFailure_WhenSourceDoesNotExist()
        {
            var sourceDir = Path.Combine(_tempDir, "nonexistent");
            var destDir = Path.Combine(_tempDir, "dest");

            var result = SampleDeployer.Deploy(sourceDir, destDir);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Deploy_OverwritesExistingDestination()
        {
            var sourceDir = Path.Combine(_tempDir, "source");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "new.txt"), "new");

            var destDir = Path.Combine(_tempDir, "dest");
            Directory.CreateDirectory(destDir);
            File.WriteAllText(Path.Combine(destDir, "old.txt"), "old");

            var result = SampleDeployer.Deploy(sourceDir, destDir);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(File.Exists(Path.Combine(destDir, "new.txt")), Is.True);
            Assert.That(File.Exists(Path.Combine(destDir, "old.txt")), Is.False);
        }

        [Test]
        public void Deploy_CopiesSubdirectoryFiles()
        {
            var sourceDir = Path.Combine(_tempDir, "source");
            var subDir = Path.Combine(sourceDir, "sub");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "sub.txt"), "content");

            var destDir = Path.Combine(_tempDir, "dest");

            var result = SampleDeployer.Deploy(sourceDir, destDir);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(File.Exists(Path.Combine(destDir, "sub", "sub.txt")), Is.True);
        }

        [Test]
        public void CopiedFileCount_ReflectsCopiedFiles()
        {
            var sourceDir = Path.Combine(_tempDir, "source");
            Directory.CreateDirectory(sourceDir);
            File.WriteAllText(Path.Combine(sourceDir, "a.txt"), "a");
            File.WriteAllText(Path.Combine(sourceDir, "b.txt"), "b");
            File.WriteAllText(Path.Combine(sourceDir, "c.txt.meta"), "meta");

            var destDir = Path.Combine(_tempDir, "dest");

            var result = SampleDeployer.Deploy(sourceDir, destDir);

            Assert.That(result.CopiedFileCount, Is.EqualTo(result.CopiedFiles.Count));
            Assert.That(result.CopiedFileCount, Is.EqualTo(3));
        }
    }
}
