// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if AUDIOCONDUCTOR_ADDRESSABLES
using System;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Providers.Tests
{
    internal class AddressableCueSheetProviderTests
    {
        private CueSheetAsset _asset = null!;

        private FakeAPIWrapper _fake = null!;
        private AddressableCueSheetProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _fake = new FakeAPIWrapper(_asset);
            _provider = new AddressableCueSheetProvider(_fake);
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
            _provider = null!;
            Object.DestroyImmediate(_asset);
            _asset = null!;
        }

        [Test]
        public void Load_ThrowsNotSupportedException()
        {
            Assert.That(() => _provider.Load("key"), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public async Task LoadAsync_ValidKey_ReturnsLoadInfo()
        {
            var result = await _provider.LoadAsync("key");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.Asset, Is.SameAs(_asset));
            Assert.That(result.Value.LoadId, Is.GreaterThan(0u));
        }

        [Test]
        public async Task LoadAsync_InvalidKey_ReturnsNull()
        {
            using var provider = new AddressableCueSheetProvider(new FakeAPIWrapper());

            var result = await provider.LoadAsync("key");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoadAsync_SameKey_ReturnsDifferentLoadIds()
        {
            var r1 = await _provider.LoadAsync("key");
            var r2 = await _provider.LoadAsync("key");

            Assert.That(r1!.Value.LoadId, Is.Not.EqualTo(r2!.Value.LoadId));
        }

        [Test]
        public async Task Release_AfterLoadAsync_CallsRelease()
        {
            var result = await _provider.LoadAsync("key");
            _provider.Release(result!.Value.LoadId);

            Assert.That(_fake.ReleaseCount, Is.EqualTo(1));
        }

        [Test]
        public async Task Release_MultipleTimes_CallsReleaseForEach()
        {
            var r1 = await _provider.LoadAsync("key");
            var r2 = await _provider.LoadAsync("key");

            _provider.Release(r1!.Value.LoadId);
            _provider.Release(r2!.Value.LoadId);

            Assert.That(_fake.ReleaseCount, Is.EqualTo(2));
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
        public async Task Release_SameLoadIdTwice_DoesNotThrow()
        {
            var result = await _provider.LoadAsync("key");
            _provider.Release(result!.Value.LoadId);

            Assert.That(() => _provider.Release(result.Value.LoadId), Throws.Nothing);
        }

        [Test]
        public async Task Dispose_CallsReleaseForAllRemainingLoads()
        {
            await _provider.LoadAsync("key");
            await _provider.LoadAsync("key");

            _provider.Dispose();

            Assert.That(_fake.ReleaseCount, Is.EqualTo(2));
        }

        private sealed class FakeAPIWrapper : AddressableCueSheetProvider.IAPIWrapper
        {
            private readonly CueSheetAsset? _asset;

            internal FakeAPIWrapper(CueSheetAsset? asset = null)
            {
                _asset = asset;
            }

            internal int ReleaseCount { get; private set; }

            public AsyncOperationHandle<T> LoadAssetAsync<T>(string key)
            {
                if (_asset is T typedAsset)
                    return Addressables.ResourceManager.CreateCompletedOperation(typedAsset, string.Empty);
                return Addressables.ResourceManager.CreateCompletedOperationWithException<T>(
                    default!, new InvalidKeyException(key));
            }

            public void Release<T>(AsyncOperationHandle<T> handle)
            {
                Addressables.Release(handle);
                ReleaseCount++;
            }
        }
    }
}
#endif
