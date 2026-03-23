// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class CueSheetProviderBaseTests
    {
        private CueSheetAsset _asset = null!;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_asset);
        }

        [Test]
        public void Load_ReturnsCueSheetLoadInfo()
        {
            using var provider = new TestProvider(_asset);

            var result = provider.Load("key");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Value.Asset, Is.SameAs(_asset));
            Assert.That(result.Value.LoadId, Is.GreaterThan(0u));
        }

        [Test]
        public void Load_WhenCoreReturnsNull_ReturnsNull()
        {
            using var provider = new TestProvider(null);

            var result = provider.Load("key");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void LoadAsync_ReturnsCueSheetLoadInfo()
        {
            using var provider = new TestProvider(_asset);

            var task = provider.LoadAsync("key");
            task.Wait();

            Assert.That(task.Result, Is.Not.Null);
            Assert.That(task.Result!.Value.Asset, Is.SameAs(_asset));
            Assert.That(task.Result.Value.LoadId, Is.GreaterThan(0u));
        }

        [Test]
        public void LoadAsync_WhenCoreReturnsNull_ReturnsNull()
        {
            using var provider = new TestProvider(null);

            var task = provider.LoadAsync("key");
            task.Wait();

            Assert.That(task.Result, Is.Null);
        }

        [Test]
        public void Load_EachCallAssignsDifferentLoadId()
        {
            using var provider = new TestProvider(_asset);

            var result1 = provider.Load("key1");
            var result2 = provider.Load("key2");

            Assert.That(result1!.Value.LoadId, Is.Not.EqualTo(result2!.Value.LoadId));
        }

        [Test]
        public void LoadAsync_EachCallAssignsDifferentLoadId()
        {
            using var provider = new TestProvider(_asset);

            var task1 = provider.LoadAsync("key1");
            task1.Wait();
            var task2 = provider.LoadAsync("key2");
            task2.Wait();

            Assert.That(task1.Result!.Value.LoadId, Is.Not.EqualTo(task2.Result!.Value.LoadId));
        }

        [Test]
        public void Load_And_LoadAsync_AssignDifferentLoadIds()
        {
            using var provider = new TestProvider(_asset);

            var syncResult = provider.Load("key1");
            var asyncTask = provider.LoadAsync("key2");
            asyncTask.Wait();

            Assert.That(syncResult!.Value.LoadId, Is.Not.EqualTo(asyncTask.Result!.Value.LoadId));
        }

        [Test]
        public void Release_CallsReleaseCoreWithCorrectState()
        {
            using var provider = new TestProvider(_asset);
            var result = provider.Load("my_key");

            provider.Release(result!.Value.LoadId);

            Assert.That(provider.ReleasedStates, Is.EqualTo(new[] { "my_key" }));
        }

        [Test]
        public void Release_UnknownLoadId_DoesNotThrow()
        {
            using var provider = new TestProvider(_asset);

            Assert.That(() => provider.Release(999), Throws.Nothing);
        }

        [Test]
        public void Release_ZeroLoadId_DoesNotThrow()
        {
            using var provider = new TestProvider(_asset);

            Assert.That(() => provider.Release(0), Throws.Nothing);
        }

        [Test]
        public void Release_ZeroLoadId_DoesNotCallReleaseCore()
        {
            using var provider = new TestProvider(_asset);

            provider.Release(0);

            Assert.That(provider.ReleasedStates, Is.Empty);
        }

        [Test]
        public void Dispose_ReleasesAllTrackedStates()
        {
            var provider = new TestProvider(_asset);
            provider.Load("key1");
            provider.Load("key2");

            provider.Dispose();

            Assert.That(provider.ReleasedStates, Has.Count.EqualTo(2));
            Assert.That(provider.ReleasedStates, Does.Contain("key1"));
            Assert.That(provider.ReleasedStates, Does.Contain("key2"));
        }

        [Test]
        public void Load_WhenLoadIdOverflows_SkipsZero()
        {
            using var provider = new TestProvider(_asset);
            var field = typeof(CueSheetProviderBase<string>).GetField(
                "_nextLoadId", BindingFlags.NonPublic | BindingFlags.Instance);
            field!.SetValue(provider, uint.MaxValue - 1u);

            var result1 = provider.Load("key1");
            var result2 = provider.Load("key2");

            Assert.That(result1!.Value.LoadId, Is.EqualTo(uint.MaxValue));
            Assert.That(result2!.Value.LoadId, Is.EqualTo(1u));
        }

        [Test]
        public void LoadCore_Default_ThrowsNotSupportedException()
        {
            using var provider = new AsyncOnlyProvider(_asset);

            Assert.That(() => provider.Load("key"), Throws.TypeOf<NotSupportedException>());
        }

        private sealed class TestProvider : CueSheetProviderBase<string>
        {
            private readonly CueSheetAsset? _assetToReturn;

            internal TestProvider(CueSheetAsset? assetToReturn)
            {
                _assetToReturn = assetToReturn;
            }

            internal List<string> ReleasedStates { get; } = new();

            protected override (CueSheetAsset asset, string state)? LoadCore(string key)
            {
                if (_assetToReturn == null)
                    return null;
                return (_assetToReturn, key);
            }

            protected override Task<(CueSheetAsset asset, string state)?> LoadCoreAsync(string key)
            {
                if (_assetToReturn == null)
                    return Task.FromResult<(CueSheetAsset asset, string state)?>(null);
                return Task.FromResult<(CueSheetAsset asset, string state)?>((_assetToReturn, key));
            }

            protected override void ReleaseCore(string state)
            {
                ReleasedStates.Add(state);
            }
        }

        private sealed class AsyncOnlyProvider : CueSheetProviderBase<string>
        {
            private readonly CueSheetAsset? _assetToReturn;

            internal AsyncOnlyProvider(CueSheetAsset? assetToReturn)
            {
                _assetToReturn = assetToReturn;
            }

            protected override Task<(CueSheetAsset asset, string state)?> LoadCoreAsync(string key)
            {
                if (_assetToReturn == null)
                    return Task.FromResult<(CueSheetAsset asset, string state)?>(null);
                return Task.FromResult<(CueSheetAsset asset, string state)?>((_assetToReturn, key));
            }

            protected override void ReleaseCore(string state)
            {
            }
        }
    }
}
