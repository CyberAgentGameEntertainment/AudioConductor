// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Providers.Tests
{
    internal class ResourcesCueSheetProviderTests
    {
        private CueSheetAsset _asset = null!;

        private FakeAPIWrapper _fake = null!;
        private ResourcesCueSheetProvider _provider = null!;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _fake = new FakeAPIWrapper(_asset);
            _provider = new ResourcesCueSheetProvider(_fake);
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
        public void Load_ValidKey_ReturnsLoadInfo()
        {
            var result = _provider.Load("key");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.Asset, Is.SameAs(_asset));
            Assert.That(result.Value.LoadId, Is.GreaterThan(0u));
        }

        [Test]
        public void Load_InvalidKey_ReturnsNull()
        {
            using var provider = new ResourcesCueSheetProvider(new FakeAPIWrapper());

            var result = provider.Load("key");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Load_SameKey_CallsApiOnce()
        {
            _provider.Load("key");
            _provider.Load("key");

            Assert.That(_fake.LoadCount, Is.EqualTo(1));
        }

        [Test]
        public void Load_SameKey_ReturnsSameAsset()
        {
            var r1 = _provider.Load("key");
            var r2 = _provider.Load("key");

            Assert.That(r1!.Value.Asset, Is.SameAs(r2!.Value.Asset));
        }

        [Test]
        public void Load_SameKey_ReturnsDifferentLoadIds()
        {
            var r1 = _provider.Load("key");
            var r2 = _provider.Load("key");

            Assert.That(r1!.Value.LoadId, Is.Not.EqualTo(r2!.Value.LoadId));
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
            using var provider = new ResourcesCueSheetProvider(new FakeAPIWrapper());

            var result = await provider.LoadAsync("key");

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task LoadAsync_SameKey_CallsApiOnce()
        {
            await _provider.LoadAsync("key");
            await _provider.LoadAsync("key");

            Assert.That(_fake.LoadAsyncCount, Is.EqualTo(1));
        }

        [Test]
        public async Task LoadAsync_SameKey_ReturnsSameAsset()
        {
            var r1 = await _provider.LoadAsync("key");
            var r2 = await _provider.LoadAsync("key");

            Assert.That(r1!.Value.Asset, Is.SameAs(r2!.Value.Asset));
        }

        [Test]
        public async Task LoadAsync_SameKey_ReturnsDifferentLoadIds()
        {
            var r1 = await _provider.LoadAsync("key");
            var r2 = await _provider.LoadAsync("key");

            Assert.That(r1!.Value.LoadId, Is.Not.EqualTo(r2!.Value.LoadId));
        }

        [Test]
        public void Release_AfterLastReference_CallsUnloadAsset()
        {
            var r = _provider.Load("key");
            _provider.Release(r!.Value.LoadId);

            Assert.That(_fake.UnloadCount, Is.EqualTo(1));
        }

        [Test]
        public void Release_WithRemainingReferences_DoesNotCallUnloadAsset()
        {
            var r1 = _provider.Load("key");
            var r2 = _provider.Load("key");

            _provider.Release(r1!.Value.LoadId);

            Assert.That(_fake.UnloadCount, Is.EqualTo(0));

            _provider.Release(r2!.Value.LoadId);

            Assert.That(_fake.UnloadCount, Is.EqualTo(1));
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
        public void Release_SameLoadIdTwice_DoesNotThrow()
        {
            var r = _provider.Load("key");
            _provider.Release(r!.Value.LoadId);

            Assert.That(() => _provider.Release(r.Value.LoadId), Throws.Nothing);
        }

        [Test]
        public void Dispose_CallsUnloadForAllRemainingLoads()
        {
            _provider.Load("key");
            _provider.Load("key");

            _provider.Dispose();

            Assert.That(_fake.UnloadCount, Is.EqualTo(1));
        }

        private sealed class FakeAPIWrapper : ResourcesCueSheetProvider.IAPIWrapper
        {
            private readonly CueSheetAsset? _asset;

            internal FakeAPIWrapper(CueSheetAsset? asset = null)
            {
                _asset = asset;
            }

            internal int LoadCount { get; private set; }
            internal int LoadAsyncCount { get; private set; }
            internal int UnloadCount { get; private set; }

            public ResourcesCueSheetProvider.IResourceRequest LoadAsync<T>(string key) where T : Object
            {
                LoadAsyncCount++;
                return new FakeResourceRequest(_asset);
            }

            public T? Load<T>(string key) where T : Object
            {
                LoadCount++;
                return _asset as T;
            }

            public void UnloadAsset(Object asset)
            {
                UnloadCount++;
            }

            private sealed class FakeResourceRequest : ResourcesCueSheetProvider.IResourceRequest
            {
                internal FakeResourceRequest(Object? asset)
                {
                    this.asset = asset;
                }

                public Object? asset { get; }

                public event Action<ResourcesCueSheetProvider.IResourceRequest> completed
                {
                    add => value(this);
                    remove { }
                }
            }
        }
    }
}
