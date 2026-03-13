// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using AudioConductor.Editor.SamplesDeployment;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.SamplesDeployment.Tests
{
    internal class SampleScannerTests
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
        public void Scan_DetectsSampleFolderWithAsmdef()
        {
            var samplesRoot = Path.Combine(_tempDir, "Samples");
            var sampleDir = Path.Combine(samplesRoot, "MySample");
            Directory.CreateDirectory(sampleDir);
            File.WriteAllText(Path.Combine(sampleDir, "MySample.asmdef"), "{}");

            var result = SampleScanner.Scan(samplesRoot);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.SampleFolders, Has.Count.EqualTo(1));
            Assert.That(result.SampleFolders[0].Name, Is.EqualTo("MySample"));
        }

        [Test]
        public void Scan_ReturnsInvalid_WhenFolderHasNoAsmdef()
        {
            var samplesRoot = Path.Combine(_tempDir, "Samples");
            var sampleDir = Path.Combine(samplesRoot, "MySample");
            Directory.CreateDirectory(sampleDir);

            var result = SampleScanner.Scan(samplesRoot);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Scan_ReturnsInvalid_WhenSamplesRootDoesNotExist()
        {
            var samplesRoot = Path.Combine(_tempDir, "Nonexistent");

            var result = SampleScanner.Scan(samplesRoot);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Scan_ReturnsInvalid_WhenNoSubfoldersExist()
        {
            var samplesRoot = Path.Combine(_tempDir, "Samples");
            Directory.CreateDirectory(samplesRoot);

            var result = SampleScanner.Scan(samplesRoot);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void Scan_SetsFullPath_ForDetectedFolder()
        {
            var samplesRoot = Path.Combine(_tempDir, "Samples");
            var sampleDir = Path.Combine(samplesRoot, "MySample");
            Directory.CreateDirectory(sampleDir);
            File.WriteAllText(Path.Combine(sampleDir, "MySample.asmdef"), "{}");

            var result = SampleScanner.Scan(samplesRoot);

            Assert.That(result.SampleFolders[0].FullPath, Is.EqualTo(sampleDir));
        }

        [Test]
        public void Scan_DetectsMultipleSampleFolders()
        {
            var samplesRoot = Path.Combine(_tempDir, "Samples");
            foreach (var name in new[] { "SampleA", "SampleB" })
            {
                var dir = Path.Combine(samplesRoot, name);
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, $"{name}.asmdef"), "{}");
            }

            var result = SampleScanner.Scan(samplesRoot);

            Assert.That(result.IsValid, Is.True);
            Assert.That(result.SampleFolders, Has.Count.EqualTo(2));
        }
    }
}
