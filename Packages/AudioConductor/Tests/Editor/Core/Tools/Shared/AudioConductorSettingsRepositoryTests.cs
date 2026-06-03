// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tests;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal sealed class AudioConductorSettingsRepositoryTests
    {
        private string? _createdAssetPath;

        [TearDown]
        public void TearDown()
        {
            if (_createdAssetPath is not null)
            {
                AssetDatabase.DeleteAsset(_createdAssetPath);
                _createdAssetPath = null;
            }
        }

        [Test]
        public void GetGuid_ReturnsGuidResolvableByGetByGuid()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/SettingsRepoTest.asset";
            AssetDatabase.CreateAsset(settings, _createdAssetPath);

            var loaded = AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(_createdAssetPath);
            var repo = AudioConductorSettingsRepository.instance;

            var guid = repo.GetGuid(loaded);
            var resolved = repo.GetByGuid(guid);

            Assert.That(resolved, Is.SameAs(loaded));
        }

        [Test]
        public void AllSettings_ContainsCreatedSettings()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/SettingsAllTest.asset";
            AssetDatabase.CreateAsset(settings, _createdAssetPath);

            var loaded = AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(_createdAssetPath);
            var repo = AudioConductorSettingsRepository.instance;

            Assert.That(repo.AllSettings, Contains.Item(loaded));
        }

        [Test]
        public void AllSettings_ReturnsSameInstanceOnConsecutiveCalls()
        {
            var repo = AudioConductorSettingsRepository.instance;
            var first = repo.AllSettings;
            var second = repo.AllSettings;

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void AllSettings_IsInvalidatedAfterAssetCreated()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var repo = AudioConductorSettingsRepository.instance;
            var before = repo.AllSettings;

            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/SettingsInvalidateTest.asset";
            AssetDatabase.CreateAsset(settings, _createdAssetPath);

            var after = repo.AllSettings;

            Assert.That(after, Is.Not.SameAs(before));
        }
    }
}
