// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class ConductorOneShotFlowTests
    {
        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _managedProvider = new StubPlayerProvider();
            _oneShotProvider = new StubPlayerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        private StubPlayerProvider _managedProvider = null!;
        private StubPlayerProvider _oneShotProvider = null!;
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
        public void PlayOneShot_WithValidClip_CallsSetupAndPlay()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1");

            var player = _oneShotProvider.Created[0];
            Assert.That(player.ClipSamples, Is.GreaterThan(0));
            Assert.That(player.State, Is.EqualTo(PlayerState.Playing));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void PlayOneShot_WithValidClip_CallsSetMasterVolume()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1");

            var player = _oneShotProvider.Created[0];
            // Default _masterVolume is 1f; GetActualVolume = VolumeAsset(1) * 1 * 1 * master(1) * cat(1) = 1.
            Assert.That(player.GetActualVolume(), Is.EqualTo(1f).Within(0.001f));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void PlayOneShot_AfterPlaybackEnds_Update_ReturnsToPool()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1");

            var oneShotPlayer = _oneShotProvider.Created[0];
            oneShotPlayer.Stop();

            conductor.Update(0.016f);

            // Player was returned to pool. Next PlayOneShot reuses it without creating a new instance.
            conductor.PlayOneShot(sheet, "cue1");
            Assert.That(_oneShotProvider.Created.Count, Is.EqualTo(1));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void StopAll_WithActiveOneShot_ReturnsToPool()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1");

            conductor.StopAll();

            // Player was returned to pool by StopAll. Next PlayOneShot reuses it without creating a new instance.
            conductor.PlayOneShot(sheet, "cue1");
            Assert.That(_oneShotProvider.Created.Count, Is.EqualTo(1));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
