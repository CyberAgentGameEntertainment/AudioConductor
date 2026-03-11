// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class ConductorCueSheetTests
    {
        private CueSheetAsset _cueSheetAsset = null!;
        private AudioConductorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _cueSheetAsset = ScriptableObject.CreateInstance<CueSheetAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_cueSheetAsset);
        }

        [Test]
        public void RegisterCueSheet_ReturnsValidHandle()
        {
            using var conductor = new Conductor(_settings);

            var handle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.That(handle.IsValid, Is.True);
        }

        [Test]
        public void RegisterCueSheet_CalledTwiceWithSameAsset_ReturnsDifferentHandles()
        {
            using var conductor = new Conductor(_settings);

            var handle1 = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle2 = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.That(handle1, Is.Not.EqualTo(handle2));
        }

        [Test]
        public void RegisterCueSheet_MonotonicallyIncreasing()
        {
            using var conductor = new Conductor(_settings);

            var handle1 = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle2 = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.That(handle2.Id, Is.GreaterThan(handle1.Id));
        }

        [Test]
        public void UnregisterCueSheet_WithValidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);
            var handle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.UnregisterCueSheet(handle));
        }

        [Test]
        public void UnregisterCueSheet_WithInvalidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.UnregisterCueSheet(default));
        }

        [Test]
        public void UnregisterCueSheet_WithUnknownHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.UnregisterCueSheet(new CueSheetHandle(999)));
        }

        [Test]
        public void Dispose_WithoutProvider_DoesNotThrow()
        {
            var conductor = new Conductor(_settings);
            conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.Dispose());
        }

        [Test]
        public void Dispose_CallsProviderReleaseForEachRegistration()
        {
            var provider = new FakeProvider { AssetToReturn = _cueSheetAsset };
            var conductor = new Conductor(_settings, provider);

            var task1 = conductor.RegisterCueSheetAsync("key1");
            task1.Wait();
            var task2 = conductor.RegisterCueSheetAsync("key2");
            task2.Wait();

            conductor.Dispose();

            Assert.That(provider.ReleasedLoadIds, Has.Count.EqualTo(2));
        }

        [Test]
        public void RegisterCueSheetAsync_WithProvider_ReturnsValidHandle()
        {
            var provider = new FakeProvider { AssetToReturn = _cueSheetAsset };
            using var conductor = new Conductor(_settings, provider);

            var task = conductor.RegisterCueSheetAsync("test_key");
            task.Wait();

            Assert.That(task.Result.IsValid, Is.True);
        }

        [Test]
        public void RegisterCueSheetAsync_WithoutProvider_ThrowsInvalidOperationException()
        {
            using var conductor = new Conductor(_settings);

            var task = conductor.RegisterCueSheetAsync("test_key");

            var ex = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.That(ex.InnerException, Is.InstanceOf<InvalidOperationException>());
        }

        [Test]
        public void UnregisterCueSheet_WithProvider_CallsRelease()
        {
            var provider = new FakeProvider { AssetToReturn = _cueSheetAsset };
            using var conductor = new Conductor(_settings, provider);

            var task = conductor.RegisterCueSheetAsync("test_key");
            task.Wait();

            conductor.UnregisterCueSheet(task.Result);

            Assert.That(provider.ReleasedLoadIds, Has.Count.EqualTo(1));
        }

        [Test]
        public void Dispose_WithDirectRegistration_DoesNotCallProviderRelease()
        {
            var provider = new FakeProvider { AssetToReturn = _cueSheetAsset };
            var conductor = new Conductor(_settings, provider);
            conductor.RegisterCueSheet(_cueSheetAsset);

            conductor.Dispose();

            Assert.That(provider.ReleasedLoadIds, Is.Empty);
        }

        [Test]
        public void UnregisterCueSheet_WithDirectRegistration_DoesNotCallProviderRelease()
        {
            var provider = new FakeProvider { AssetToReturn = _cueSheetAsset };
            using var conductor = new Conductor(_settings, provider);
            var handle = conductor.RegisterCueSheet(_cueSheetAsset);

            conductor.UnregisterCueSheet(handle);

            Assert.That(provider.ReleasedLoadIds, Is.Empty);
        }

        private sealed class FakeProvider : ICueSheetProvider
        {
            private uint _loadIdCounter;
            internal CueSheetAsset AssetToReturn { get; set; } = null!;
            internal List<uint> ReleasedLoadIds { get; } = new();

            public CueSheetLoadInfo? Load(string key)
            {
                if (AssetToReturn == null)
                    return null;
                return new CueSheetLoadInfo(AssetToReturn, ++_loadIdCounter);
            }

            public Task<CueSheetLoadInfo?> LoadAsync(string key)
            {
                return Task.FromResult(Load(key));
            }

            public void Release(uint loadId)
            {
                ReleasedLoadIds.Add(loadId);
            }
        }
    }
}
