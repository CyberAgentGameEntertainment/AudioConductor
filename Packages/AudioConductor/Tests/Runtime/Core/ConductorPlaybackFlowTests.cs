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
    internal sealed class ConductorPlaybackFlowTests
    {
        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _managedProvider = new FakePlayerProvider();
            _oneShotProvider = new FakePlayerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        private FakePlayerProvider _managedProvider = null!;
        private FakePlayerProvider _oneShotProvider = null!;
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
        public void Play_WithValidClip_CallsPlayerSetupAndPlay()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            Assert.That(player.SetupCount, Is.EqualTo(1));
            Assert.That(player.PlayCount, Is.EqualTo(1));
            Assert.That(player.IsPlaying, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_WithValidClip_PassesCorrectSetupParameters()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.categoryId = 3;
            var track = CreateTrack(clip);
            track.volume = 0.8f;
            track.pitch = 1.2f;
            cue.trackList.Add(track);
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            Assert.That(player.CategoryId, Is.EqualTo(3));
            Assert.That(player.SetupVolume, Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(player.SetupPitch, Is.EqualTo(1.2f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_WithValidClip_CallsSetMasterVolume()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            // Default _masterVolume is 1f.
            Assert.That(player.MasterVolume, Is.EqualTo(1f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_WithCustomMasterVolume_PlayerReceivesMasterVolume()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            conductor.SetMasterVolume(0.5f);
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            Assert.That(player.MasterVolume, Is.EqualTo(0.5f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_ReturnsValidHandle()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");

            Assert.That(handle.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_TwiceWithSameCue_RentsTwoPlayers()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");
            conductor.Play(sheet, "cue1");

            Assert.That(_managedProvider.Created.Count, Is.EqualTo(2));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
