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
    [TestFixture]
    internal sealed class ConductorFadeProgressionTests
    {
        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _managedProvider = new SpyPlayerProvider();
            _oneShotProvider = new SpyPlayerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        private SpyPlayerProvider _managedProvider = null!;
        private SpyPlayerProvider _oneShotProvider = null!;
        private AudioConductorSettings _settings = null!;

        private Conductor CreateConductor()
        {
            return new Conductor(_settings, _managedProvider, _oneShotProvider);
        }

        private static AudioClip CreateClip()
        {
            return AudioClip.Create("test", 44100, 1, 44100, false);
        }

        private static CueSheetAsset CreateSheetAsset(params Cue[] cues)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            foreach (var cue in cues)
                asset.cueSheet.cueList.Add(cue);
            return asset;
        }

        private static Cue CreateCue(string name)
        {
            return new Cue { name = name };
        }

        private static Track CreateTrack(AudioClip clip)
        {
            return new Track { name = "track", audioClip = clip };
        }

        [Test]
        public void Play_WithFadeIn_InitialVolumeFadeIsZero()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });

            var player = _managedProvider.Created[0];
            Assert.That(player.VolumeFade, Is.EqualTo(0f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_WithFadeIn_AfterUpdate_VolumeFadeIncreases()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });
            conductor.Update(0.5f);

            var player = _managedProvider.Created[0];
            Assert.That(player.VolumeFade, Is.GreaterThan(0f).And.LessThan(1f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_WithFadeIn_AfterFullDuration_VolumeFadeIsOne()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });
            conductor.Update(1.0f);

            var player = _managedProvider.Created[0];
            Assert.That(player.VolumeFade, Is.EqualTo(1f).Within(0.001f));
            Assert.That(player.IsFading, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Stop_WithFadeOut_VolumeFadeDecreases()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Stop(handle, 1f);
            conductor.Update(0.5f);

            var player = _managedProvider.Created[0];
            Assert.That(player.VolumeFade, Is.LessThan(1f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Stop_WithFadeOut_AfterCompletion_PlayerReturnedToPool()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Stop(handle, 0.1f);
            conductor.Update(0.2f);

            // After fade-out completes, player.Stop() is called then Return() returns it to pool.
            // A subsequent Play reuses the pooled player without creating a new instance.
            conductor.Play(sheet, "cue1");
            Assert.That(_managedProvider.Created.Count, Is.EqualTo(1));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
