// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class ConductorOneShotTests
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
        public void PlayOneShot_WithInvalidSheetHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(default, "cue"));
        }

        [Test]
        public void PlayOneShot_WithUnregisteredHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(new CueSheetHandle(999), "cue"));
        }

        [Test]
        public void PlayOneShot_WithNonExistentCueName_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(sheetHandle, "nonexistent_cue"));
        }

        [Test]
        public void PlayOneShot_WithCueHavingNoTracks_DoesNotThrow()
        {
            var cue = new Cue { name = "empty_cue" };
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(sheetHandle, "empty_cue"));
        }

        [Test]
        public void PlayOneShot_WithTrackHavingNullClip_DoesNotThrow()
        {
            var track = new Track { name = "track1", audioClip = null };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(sheetHandle, "cue1"));
        }

        [Test]
        public void PlayOneShot_WithValidAudioClip_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(sheetHandle, "cue1"));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void PlayOneShot_ReturnsNoHandle_IsPlayingWithDefaultReturnsFalse()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            conductor.PlayOneShot(sheetHandle, "cue1");

            // PlayOneShot returns no handle; no managed playback is created.
            Assert.That(conductor.IsPlaying(default), Is.False);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Dispose_WithActivateOneShotPlaybacks_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            conductor.PlayOneShot(sheetHandle, "cue1");

            Assert.DoesNotThrow(() => conductor.Dispose());

            Object.DestroyImmediate(clip);
        }
    }
}
