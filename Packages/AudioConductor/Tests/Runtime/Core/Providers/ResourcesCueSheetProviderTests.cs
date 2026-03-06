// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Providers;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace AudioConductor.Tests.Runtime.Core.Providers
{
    public class ResourcesCueSheetProviderTests
    {
        private const string ResourcesPath = "Assets/AudioConductorTestResources/Resources";
        private const string AssetFileName = "TestCueSheet.asset";

        private ResourcesCueSheetProvider _provider;

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
            _provider = null;

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
        public void Load_ValidKey_ReturnsAsset()
        {
            CreateTestAsset();

            var asset = _provider.Load("TestCueSheet");

            Assert.That(asset, Is.Not.Null);
        }

        [Test]
        public void Load_SameKey_ReturnsSameAsset()
        {
            CreateTestAsset();

            var asset1 = _provider.Load("TestCueSheet");
            var asset2 = _provider.Load("TestCueSheet");

            Assert.That(asset1, Is.SameAs(asset2));
        }

        [Test]
        public void Release_AfterSingleLoad_DoesNotThrow()
        {
            CreateTestAsset();

            var asset = _provider.Load("TestCueSheet");
            _provider.Release(asset);

            // After release with count 0, asset should be unloaded (no exception thrown)
            Assert.Pass();
        }

        [Test]
        public void Release_MultipleLoads_DoesNotThrow()
        {
            CreateTestAsset();

            var asset1 = _provider.Load("TestCueSheet");
            var asset2 = _provider.Load("TestCueSheet");

            // First release should not unload (count goes from 2 to 1)
            _provider.Release(asset1);

            // Asset should still be accessible
            Assert.That(asset2, Is.Not.Null);

            // Second release should unload (count goes from 1 to 0)
            _provider.Release(asset2);

            Assert.Pass();
        }

        [UnityTest]
        public IEnumerator LoadAsync_ReturnsAsset()
        {
            CreateTestAsset();

            var task = _provider.LoadAsync("TestCueSheet");

            while (!task.IsCompleted)
                yield return null;

            Assert.That(task.Result, Is.Not.Null);
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
        public void Release_UnloadedAsset_DoesNotThrow()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            Assert.That(() => _provider.Release(asset), Throws.Nothing);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Release_NullAsset_DoesNotThrow()
        {
            Assert.That(() => _provider.Release(null), Throws.Nothing);
        }

        [Test]
        public void Load_InvalidKey_ReturnsNull()
        {
            var asset = _provider.Load("NonExistentKey");

            Assert.That(asset, Is.Null);
        }

        [UnityTest]
        public IEnumerator LoadAsync_ThenRelease_DoesNotThrow()
        {
            CreateTestAsset();

            var task = _provider.LoadAsync("TestCueSheet");

            while (!task.IsCompleted)
                yield return null;

            Assert.That(() => _provider.Release(task.Result), Throws.Nothing);
        }

        [UnityTest]
        public IEnumerator Release_AfterLoadAsyncInvalidKey_DoesNotThrow()
        {
            var task = _provider.LoadAsync("NonExistentKey");

            while (!task.IsCompleted)
                yield return null;

            // null is returned on failure; Release(null) must be safe
            Assert.That(() => _provider.Release(task.Result), Throws.Nothing);
        }

        [Test]
        public void Release_MoreTimesThanLoaded_DoesNotThrow()
        {
            CreateTestAsset();

            var asset = _provider.Load("TestCueSheet");
            _provider.Release(asset);

            // Second release on already-removed asset must be ignored safely
            Assert.That(() => _provider.Release(asset), Throws.Nothing);
        }
    }
}
