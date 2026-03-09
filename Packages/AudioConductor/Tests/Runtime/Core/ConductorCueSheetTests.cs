// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Threading.Tasks;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using UnityEngine;
using AudioConductorSettings = AudioConductor.Runtime.Core.Models.AudioConductorSettings;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
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
            var provider = new FakeProvider();
            var conductor = new Conductor(_settings, provider);
            conductor.RegisterCueSheet(_cueSheetAsset);
            conductor.RegisterCueSheet(_cueSheetAsset);

            conductor.Dispose();

            Assert.That(provider.ReleaseCallCount, Is.EqualTo(2));
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
            var provider = new FakeProvider();
            using var conductor = new Conductor(_settings, provider);
            var handle = conductor.RegisterCueSheet(_cueSheetAsset);

            conductor.UnregisterCueSheet(handle);

            Assert.That(provider.ReleaseCallCount, Is.EqualTo(1));
        }

        private sealed class FakeProvider : ICueSheetProvider
        {
            internal CueSheetAsset AssetToReturn { get; set; } = null!;
            internal int ReleaseCallCount { get; private set; }

            public CueSheetAsset? Load(string key)
            {
                return AssetToReturn;
            }

            public Task<CueSheetAsset?> LoadAsync(string key)
            {
                return Task.FromResult<CueSheetAsset?>(AssetToReturn);
            }

            public void Release(CueSheetAsset? asset)
            {
                ReleaseCallCount++;
            }
        }
    }
}
