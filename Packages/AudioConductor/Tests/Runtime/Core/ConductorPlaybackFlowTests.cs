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

        [Test]
        public void Stop_AfterPlay_PlayerBecomesInactive()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            Assert.That(player.IsPlaying, Is.True);

            conductor.Stop(handle);

            // StopPlayback() calls player.Stop() then Return() which calls ResetState(),
            // so StopCount is reset to 0. Verify the inactive state instead.
            Assert.That(player.IsPlaying, Is.False);
            Assert.That(player.IsPaused, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Pause_AfterPlay_IsPaused()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Pause(handle);

            var player = _managedProvider.Created[0];
            Assert.That(player.IsPaused, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Resume_AfterPause_CallsSpyPlayerResume()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Pause(handle);
            conductor.Resume(handle);

            var player = _managedProvider.Created[0];
            Assert.That(player.ResumeCount, Is.EqualTo(1));
            Assert.That(player.IsPlaying, Is.True);
            Assert.That(player.IsPaused, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void IsPlaying_AfterPlay_ReturnsTrue()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");

            Assert.That(conductor.IsPlaying(handle), Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void IsPlaying_AfterStop_ReturnsFalse()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Stop(handle);

            Assert.That(conductor.IsPlaying(handle), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void IsPlaying_AfterPause_ReturnsFalse()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.Pause(handle);

            Assert.That(conductor.IsPlaying(handle), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void SetVolume_AfterPlay_CallsSpyPlayerSetVolume()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.SetVolume(handle, 0.6f);

            var player = _managedProvider.Created[0];
            Assert.That(player.Volume, Is.EqualTo(0.6f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void SetPitch_AfterPlay_CallsSpyPlayerSetPitch()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");
            conductor.SetPitch(handle, 1.5f);

            var player = _managedProvider.Created[0];
            Assert.That(player.Pitch, Is.EqualTo(1.5f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Update_WithActivePlayback_CallsManualUpdate()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");

            conductor.Update(0.016f);

            var player = _managedProvider.Created[0];
            Assert.That(player.ManualUpdateCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(player.LastDeltaTime, Is.EqualTo(0.016f).Within(0.0001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void SetMasterVolume_WithActivePlayback_PropagatesToPlayer()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1");
            conductor.SetMasterVolume(0.7f);

            var player = _managedProvider.Created[0];
            Assert.That(player.MasterVolume, Is.EqualTo(0.7f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void SetMasterVolume_WithActiveOneShot_PropagatesToPlayer()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1");
            conductor.SetMasterVolume(0.3f);

            var player = _oneShotProvider.Created[0];
            Assert.That(player.MasterVolume, Is.EqualTo(0.3f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
