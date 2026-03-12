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
    internal sealed class ConductorUpdateTests
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
        public void Update_WhenPlayerStoppedExternally_RemovesPlayback()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            player.IsPlaying = false;

            conductor.Update(0.016f);

            // Player was returned to pool. Next Play reuses it instead of creating a new one.
            conductor.Play(sheet, "cue1");
            Assert.That(_managedProvider.Created.Count, Is.EqualTo(1));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Update_WhenPlayerPaused_DoesNotRemovePlayback()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Pause(handle);

            conductor.Update(0.016f);

            // Playback is retained: resume restores playing state.
            conductor.Resume(handle);
            Assert.That(conductor.IsPlaying(handle), Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Update_WhenPlayerFading_DoesNotRemovePlayback()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            player.IsPlaying = false;
            player.IsFading = true;

            conductor.Update(0.016f);

            // Playback is NOT removed because IsFading == true.
            // Player was not returned to pool; next Play creates a new instance.
            conductor.Play(sheet, "cue1");
            Assert.That(_managedProvider.Created.Count, Is.EqualTo(2));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Update_FadeOutCompleted_StopsAndRemovesPlayer()
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

            Assert.That(conductor.IsPlaying(handle), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Update_OneShot_WhenNotPlaying_ReturnsToPool()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1");

            Assert.That(_oneShotProvider.Created.Count, Is.EqualTo(1));

            var player = _oneShotProvider.Created[0];
            player.IsPlaying = false;

            conductor.Update(0.016f);

            // After Return(), the same player is back in the pool.
            // A subsequent PlayOneShot should reuse it without creating a new instance.
            conductor.PlayOneShot(sheet, "cue1");
            Assert.That(_oneShotProvider.Created.Count, Is.EqualTo(1));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
