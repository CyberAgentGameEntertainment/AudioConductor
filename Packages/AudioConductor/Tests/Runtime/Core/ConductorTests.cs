// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    internal sealed class ConductorTests
    {
        private AudioConductorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Constructor_CreatesRootGameObject()
        {
            using var conductor = new Conductor(_settings);

            var rootObject = GameObject.Find(nameof(Conductor));
            Assert.That(rootObject, Is.Not.Null);
        }

        [Test]
        public void Constructor_AttachesConductorBehaviour()
        {
            using var conductor = new Conductor(_settings);

            var rootObject = GameObject.Find(nameof(Conductor));
            Assert.That(rootObject, Is.Not.Null);
            var behaviour = rootObject.GetComponent<ConductorBehaviour>();
            Assert.That(behaviour, Is.Not.Null);
        }

        [Test]
        public void Dispose_DestroysRootGameObject()
        {
            GameObject rootObject;
            using (var conductor = new Conductor(_settings))
            {
                rootObject = GameObject.Find(nameof(Conductor));
                Assert.That(rootObject, Is.Not.Null);
            }

            // In EditMode, Object.DestroyImmediate is used, so the reference is immediately null-marked.
            Assert.That(rootObject == null, Is.True);
        }

        [Test]
        public void Dispose_DisconnectsBehaviourDelegate()
        {
            var conductor = new Conductor(_settings);
            var rootObject = GameObject.Find(nameof(Conductor));
            var behaviour = rootObject.GetComponent<ConductorBehaviour>();
            Assert.That(behaviour.Conductor, Is.Not.Null);

            conductor.Dispose();

            // After Dispose, the delegate is disconnected before the GameObject is destroyed.
            Assert.That(behaviour.Conductor, Is.Null);
        }

        [Test]
        public void GetAudioMixerGroup_WithUnknownCategoryId_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);

            var mixerGroup = conductor.GetAudioMixerGroup(999);

            Assert.That(mixerGroup, Is.Null);
        }

        [Test]
        public void GetCategoryVolume_WhenNotSet_ReturnsOne()
        {
            using var conductor = new Conductor(_settings, new StubPlayerProvider(), new StubPlayerProvider());

            Assert.That(conductor.GetCategoryVolume(1), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void SetCategoryVolume_ThenGetCategoryVolume_ReturnsSameValue()
        {
            using var conductor = new Conductor(_settings, new StubPlayerProvider(), new StubPlayerProvider());

            conductor.SetCategoryVolume(1, 0.5f);

            Assert.That(conductor.GetCategoryVolume(1), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetCategoryVolume_AppliedToManagedPlayerWithMatchingCategory()
        {
            var managedProvider = new StubPlayerProvider();
            var oneShotProvider = new StubPlayerProvider();
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var cue = new Cue { name = "cue1", categoryId = 1 };
            cue.trackList.Add(new Track { name = "track1", audioClip = clip });
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings, managedProvider, oneShotProvider);
            var sheetHandle = conductor.RegisterCueSheet(asset);
            conductor.Play(sheetHandle, "cue1");

            conductor.SetCategoryVolume(1, 0.4f);

            var player = managedProvider.Created[0];
            Assert.That(player.GetActualVolume(), Is.EqualTo(0.4f).Within(0.0001f));

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void SetCategoryVolume_AppliedToOneShotPlayerWithMatchingCategory()
        {
            var managedProvider = new StubPlayerProvider();
            var oneShotProvider = new StubPlayerProvider();
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var cue = new Cue { name = "cue1", categoryId = 2 };
            cue.trackList.Add(new Track { name = "track1", audioClip = clip });
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings, managedProvider, oneShotProvider);
            var sheetHandle = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheetHandle, "cue1");

            conductor.SetCategoryVolume(2, 0.3f);

            var player = oneShotProvider.Created[0];
            Assert.That(player.GetActualVolume(), Is.EqualTo(0.3f).Within(0.0001f));

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void SetCategoryVolume_NotAppliedToPlayerWithDifferentCategory()
        {
            var managedProvider = new StubPlayerProvider();
            var oneShotProvider = new StubPlayerProvider();
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var cue = new Cue { name = "cue1", categoryId = 10 };
            cue.trackList.Add(new Track { name = "track1", audioClip = clip });
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings, managedProvider, oneShotProvider);
            var sheetHandle = conductor.RegisterCueSheet(asset);
            conductor.Play(sheetHandle, "cue1");

            conductor.SetCategoryVolume(99, 0.1f);

            var player = managedProvider.Created[0];
            Assert.That(player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Play_AppliesCategoryVolumeAtStart()
        {
            var managedProvider = new StubPlayerProvider();
            var oneShotProvider = new StubPlayerProvider();
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var cue = new Cue { name = "cue1", categoryId = 5 };
            cue.trackList.Add(new Track { name = "track1", audioClip = clip });
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings, managedProvider, oneShotProvider);
            conductor.SetCategoryVolume(5, 0.7f);
            var sheetHandle = conductor.RegisterCueSheet(asset);
            conductor.Play(sheetHandle, "cue1");

            var player = managedProvider.Created[0];
            Assert.That(player.GetActualVolume(), Is.EqualTo(0.7f).Within(0.0001f));

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void PlayOneShot_AppliesCategoryVolumeAtStart()
        {
            var managedProvider = new StubPlayerProvider();
            var oneShotProvider = new StubPlayerProvider();
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var cue = new Cue { name = "cue1", categoryId = 5 };
            cue.trackList.Add(new Track { name = "track1", audioClip = clip });
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings, managedProvider, oneShotProvider);
            conductor.SetCategoryVolume(5, 0.7f);
            var sheetHandle = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheetHandle, "cue1");

            var player = oneShotProvider.Created[0];
            Assert.That(player.GetActualVolume(), Is.EqualTo(0.7f).Within(0.0001f));

            Object.DestroyImmediate(asset);
            Object.DestroyImmediate(clip);
        }
    }
}
