// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AudioConductor.Core.Providers.Tests
{
    public class ResourcesCueSheetProviderTests
    {
        private const string ResourcesPath = "Assets/AudioConductorTestResources/Resources";
        private const string AssetFileName = "TestCueSheet.asset";

        private ResourcesCueSheetProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            _provider = new ResourcesCueSheetProvider();

            if (!AssetDatabase.IsValidFolder("Assets/AudioConductorTestResources"))
                AssetDatabase.CreateFolder("Assets", "AudioConductorTestResources");
            if (!AssetDatabase.IsValidFolder(ResourcesPath))
                AssetDatabase.CreateFolder("Assets/AudioConductorTestResources", "Resources");
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
            _provider = null!;

            if (AssetDatabase.IsValidFolder("Assets/AudioConductorTestResources"))
            {
                AssetDatabase.DeleteAsset("Assets/AudioConductorTestResources");
                AssetDatabase.Refresh();
            }
        }

        private string CreateTestAsset(string fileName = AssetFileName)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var assetPath = $"{ResourcesPath}/{fileName}";
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.Refresh();
            return assetPath;
        }

        [Test]
        public void Load_ValidKey_ReturnsLoadInfo()
        {
            CreateTestAsset();

            var result = _provider.Load("TestCueSheet");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.Asset, Is.Not.Null);
            Assert.That(result.Value.LoadId, Is.GreaterThan(0u));
        }

        [Test]
        public void Load_SameKey_ReturnsSameAsset()
        {
            CreateTestAsset();

            var result1 = _provider.Load("TestCueSheet");
            var result2 = _provider.Load("TestCueSheet");

            Assert.That(result1!.Value.Asset, Is.SameAs(result2!.Value.Asset));
        }

        [Test]
        public void Load_SameKey_ReturnsDifferentLoadIds()
        {
            CreateTestAsset();

            var result1 = _provider.Load("TestCueSheet");
            var result2 = _provider.Load("TestCueSheet");

            Assert.That(result1!.Value.LoadId, Is.Not.EqualTo(result2!.Value.LoadId));
        }

        [Test]
        public void Release_AfterSingleLoad_DoesNotThrow()
        {
            CreateTestAsset();

            var result = _provider.Load("TestCueSheet");
            _provider.Release(result!.Value.LoadId);

            Assert.Pass();
        }

        [Test]
        public void Release_MultipleLoads_DoesNotThrow()
        {
            CreateTestAsset();

            var result1 = _provider.Load("TestCueSheet");
            var result2 = _provider.Load("TestCueSheet");

            _provider.Release(result1!.Value.LoadId);

            Assert.That(result2!.Value.Asset, Is.Not.Null);

            _provider.Release(result2.Value.LoadId);

            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator LoadAsync_ReturnsLoadInfo()
        {
            CreateTestAsset();

            var task = _provider.LoadAsync("TestCueSheet");

            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.Result, Is.Not.Null);
            Assert.That(task.Result!.Value.Asset, Is.Not.Null);
            Assert.That(task.Result.Value.LoadId, Is.GreaterThan(0u));
        }

        [UnityTest]
        public IEnumerator LoadAsync_InvalidKey_ReturnsNull()
        {
            var task = _provider.LoadAsync("NonExistentKey");

            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.Result, Is.Null);
        }

        [Test]
        public void Release_ZeroLoadId_DoesNotThrow()
        {
            Assert.That(() => _provider.Release(0), Throws.Nothing);
        }

        [Test]
        public void Release_UnknownLoadId_DoesNotThrow()
        {
            Assert.That(() => _provider.Release(999), Throws.Nothing);
        }

        [Test]
        public void Load_InvalidKey_ReturnsNull()
        {
            var result = _provider.Load("NonExistentKey");

            Assert.That(result, Is.Null);
        }

        [UnityTest]
        public IEnumerator LoadAsync_ThenRelease_DoesNotThrow()
        {
            CreateTestAsset();

            var task = _provider.LoadAsync("TestCueSheet");

            while (!task.IsCompleted)
                yield return null;

            Assert.That(() => _provider.Release(task.Result!.Value.LoadId), Throws.Nothing);
        }

        [Test]
        public void Release_MoreTimesThanLoaded_DoesNotThrow()
        {
            CreateTestAsset();

            var result = _provider.Load("TestCueSheet");
            _provider.Release(result!.Value.LoadId);

            // Second release with same loadId is already removed, should be safe
            Assert.That(() => _provider.Release(result.Value.LoadId), Throws.Nothing);
        }
    }
}
