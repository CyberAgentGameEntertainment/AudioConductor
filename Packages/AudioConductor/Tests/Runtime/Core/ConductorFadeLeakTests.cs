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
    public class ConductorFadeLeakTests
    {
        private SpyPlayerProvider _managedProvider = null!;
        private SpyPlayerProvider _oneShotProvider = null!;
        private AudioConductorSettings _settings = null!;

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

        private static Track CreateTrack(AudioClip clip, int priority = 0)
        {
            return new Track { name = "track", audioClip = clip, priority = priority };
        }

        [Test]
        public void FadeIn_ThenImmediateStop_PlayerFadeCleared()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });

            var player = _managedProvider.Created[0];
            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingIn));

            conductor.Stop(handle);
            conductor.Update(0.016f);

            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.FadeState, Is.EqualTo(FadeState.None));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void FadeOut_ThenImmediateStop_PlayerFadeCleared()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1");

            var player = _managedProvider.Created[0];
            conductor.Stop(handle, 1f);
            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingOut));

            // Immediate stop via a new Stop call (no fade).
            conductor.Stop(handle);
            conductor.Update(0.016f);

            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.FadeState, Is.EqualTo(FadeState.None));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void FadeIn_ThenFadeOutStop_OldFadeInvalidated()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });

            var player = _managedProvider.Created[0];
            var fadeInId = player.ActiveFadeId;
            Assert.That(fadeInId, Is.Not.EqualTo(0u));

            // Start fade-out while fade-in is active.
            conductor.Stop(handle, 0.5f);

            // ActiveFadeId should now be a different (fade-out) id.
            Assert.That(player.ActiveFadeId, Is.Not.EqualTo(fadeInId));
            Assert.That(player.ActiveFadeId, Is.Not.EqualTo(0u));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void StopAll_DuringFade_ClearsAllFades()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });
            conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });

            Assert.That(_managedProvider.Created[0].FadeState, Is.EqualTo(FadeState.FadingIn));
            Assert.That(_managedProvider.Created[1].FadeState, Is.EqualTo(FadeState.FadingIn));

            conductor.StopAll();
            conductor.Update(0.016f);

            foreach (var player in _managedProvider.Created)
            {
                Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
                Assert.That(player.FadeState, Is.EqualTo(FadeState.None));
            }

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ImmediateStop_ThenUpdate_NoVolumeCorruption()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });

            var player = _managedProvider.Created[0];
            conductor.Stop(handle);

            // After stop, player is returned to pool and ResetState sets VolumeFade = 1.
            var volumeAfterStop = player.VolumeFade;
            conductor.Update(0.016f);

            Assert.That(player.VolumeFade, Is.EqualTo(volumeAfterStop));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void StopAll_DuringFadeOut_ClearsAllFades()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var h1 = conductor.Play(sheet, "cue1");
            var h2 = conductor.Play(sheet, "cue1");

            conductor.Stop(h1, 1f);
            conductor.Stop(h2, 1f);
            Assert.That(_managedProvider.Created[0].FadeState, Is.EqualTo(FadeState.FadingOut));
            Assert.That(_managedProvider.Created[1].FadeState, Is.EqualTo(FadeState.FadingOut));

            conductor.StopAll();
            conductor.Update(0.016f);

            foreach (var player in _managedProvider.Created)
            {
                Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
                Assert.That(player.FadeState, Is.EqualTo(FadeState.None));
            }

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Eviction_DuringFadeIn_ClearsFade()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.PriorityOrder;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);

            conductor.Play(sheet, "cue1", new PlayOptions { FadeTime = 1f });
            var evictedPlayer = _managedProvider.Created[0];
            Assert.That(evictedPlayer.FadeState, Is.EqualTo(FadeState.FadingIn));

            // Second play triggers eviction of the first.
            conductor.Play(sheet, "cue1");
            conductor.Update(0.016f);

            Assert.That(evictedPlayer.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(evictedPlayer.FadeState, Is.EqualTo(FadeState.None));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Eviction_DuringFadeOut_ClearsFade()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.PriorityOrder;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);

            var handle = conductor.Play(sheet, "cue1");
            var evictedPlayer = _managedProvider.Created[0];
            conductor.Stop(handle, 1f);
            Assert.That(evictedPlayer.FadeState, Is.EqualTo(FadeState.FadingOut));

            // Second play triggers eviction of the fading-out first.
            conductor.Play(sheet, "cue1");
            conductor.Update(0.016f);

            Assert.That(evictedPlayer.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(evictedPlayer.FadeState, Is.EqualTo(FadeState.None));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
