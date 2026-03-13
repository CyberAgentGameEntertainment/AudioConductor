// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#if AUDIOCONDUCTOR_DEVELOPER && AUDIOCONDUCTOR_NEWTONSOFT_JSON

#nullable enable

using System.Collections.Generic;
using System.IO;
using AudioConductor.Editor.SamplesDeployment;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.SamplesDeployment.Tests
{
    internal class PackageManifestUpdaterTests
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
        public void UpdateSamples_WritesSamplesArrayToPackageJson()
        {
            var packageJsonPath = Path.Combine(_tempDir, "package.json");
            File.WriteAllText(packageJsonPath, "{\"name\": \"com.example.package\"}");

            var samples = new List<PackageManifestUpdater.SampleInfo>
            {
                new("My Sample", "A sample", "Samples~/MySample")
            };

            var result = PackageManifestUpdater.UpdateSamples(packageJsonPath, samples);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.UpdatedSamplesCount, Is.EqualTo(1));

            var json = File.ReadAllText(packageJsonPath);
            Assert.That(json, Does.Contain("My Sample"));
            Assert.That(json, Does.Contain("Samples~/MySample"));
        }

        [Test]
        public void UpdateSamples_ReturnsFailure_WhenPackageJsonDoesNotExist()
        {
            var packageJsonPath = Path.Combine(_tempDir, "nonexistent.json");

            var result = PackageManifestUpdater.UpdateSamples(packageJsonPath, new List<PackageManifestUpdater.SampleInfo>());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void UpdateSamples_ReturnsFailure_WhenJsonIsInvalid()
        {
            var packageJsonPath = Path.Combine(_tempDir, "package.json");
            File.WriteAllText(packageJsonPath, "not valid json");

            var result = PackageManifestUpdater.UpdateSamples(packageJsonPath, new List<PackageManifestUpdater.SampleInfo>());

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Errors, Has.Count.GreaterThan(0));
        }

        [Test]
        public void UpdateSamples_UpdatesExistingSamplesArray()
        {
            var packageJsonPath = Path.Combine(_tempDir, "package.json");
            File.WriteAllText(packageJsonPath,
                "{\"name\": \"com.example\", \"samples\": [{\"displayName\": \"Old\", \"description\": \"\", \"path\": \"Samples~/Old\"}]}");

            var samples = new List<PackageManifestUpdater.SampleInfo>
            {
                new("New Sample", "New", "Samples~/NewSample")
            };

            var result = PackageManifestUpdater.UpdateSamples(packageJsonPath, samples);

            Assert.That(result.IsSuccess, Is.True);
            var json = File.ReadAllText(packageJsonPath);
            Assert.That(json, Does.Contain("New Sample"));
            Assert.That(json, Does.Not.Contain("\"Old\""));
        }

        [Test]
        public void UpdateSamples_SetsCorrectUpdatedSamplesCount()
        {
            var packageJsonPath = Path.Combine(_tempDir, "package.json");
            File.WriteAllText(packageJsonPath, "{\"name\": \"com.example\"}");

            var samples = new List<PackageManifestUpdater.SampleInfo>
            {
                new("Sample A", "A", "Samples~/SampleA"),
                new("Sample B", "B", "Samples~/SampleB")
            };

            var result = PackageManifestUpdater.UpdateSamples(packageJsonPath, samples);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.UpdatedSamplesCount, Is.EqualTo(2));
        }
    }
}

#endif
