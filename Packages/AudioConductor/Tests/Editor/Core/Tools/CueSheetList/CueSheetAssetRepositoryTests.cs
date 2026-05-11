// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tests;
using AudioConductor.Editor.Core.Tools.Shared;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Tests
{
    internal sealed class CueSheetAssetRepositoryTests
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
        public void GetAll_IncludesCreatedAsset()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/RepoTestSheet.asset";
            AssetDatabase.CreateAsset(asset, _createdAssetPath);

            var loaded = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(_createdAssetPath);
            var all = CueSheetAssetRepository.instance.GetAll();

            Assert.That(all, Has.Some.SameAs(loaded));
        }

        [Test]
        public void GetAll_ExcludesDeletedAsset()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/RepoDeleteTestSheet.asset";
            AssetDatabase.CreateAsset(asset, _createdAssetPath);

            var loaded = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(_createdAssetPath);
            AssetDatabase.DeleteAsset(_createdAssetPath);
            _createdAssetPath = null;

            var all = CueSheetAssetRepository.instance.GetAll();

            Assert.That(all, Has.None.SameAs(loaded));
        }

        [Test]
        public void Changed_FiresWhenAssetCreated()
        {
            var fired = false;
            Action handler = () => fired = true;
            CueSheetAssetRepository.instance.Changed += handler;

            try
            {
                Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
                var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
                _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/RepoChangedTestSheet.asset";
                AssetDatabase.CreateAsset(asset, _createdAssetPath);
            }
            finally
            {
                CueSheetAssetRepository.instance.Changed -= handler;
            }

            Assert.That(fired, Is.True);
        }

        [Test]
        public void Changed_FiresWhenAssetDeleted()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/RepoDeleteChangedTestSheet.asset";
            AssetDatabase.CreateAsset(asset, _createdAssetPath);

            var fired = false;
            Action handler = () => fired = true;
            CueSheetAssetRepository.instance.Changed += handler;

            try
            {
                AssetDatabase.DeleteAsset(_createdAssetPath);
                _createdAssetPath = null;
            }
            finally
            {
                CueSheetAssetRepository.instance.Changed -= handler;
            }

            Assert.That(fired, Is.True);
        }

        [Test]
        public void GetAll_ReturnsNewArrayAfterAssetCreated()
        {
            var before = CueSheetAssetRepository.instance.GetAll();

            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _createdAssetPath = $"{GlobalSetUpFixture.GenFolder}/RepoCacheTestSheet.asset";
            AssetDatabase.CreateAsset(asset, _createdAssetPath);

            var after = CueSheetAssetRepository.instance.GetAll();

            Assert.That(after, Is.Not.SameAs(before));
        }
    }
}
