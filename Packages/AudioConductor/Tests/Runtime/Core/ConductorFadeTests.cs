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
    public class ConductorFadeTests
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
        public void Stop_WithFadeTime_StartsFade()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            conductor.Stop(handle, 1.0f);

            // Fade-out is in progress, so the playback remains tracked as playing.
            Object.DestroyImmediate(clip);
            Assert.That(conductor.IsPlaying(handle), Is.True);
        }

        [Test]
        public void Stop_WithFadeTimeAndCustomFader_StartsFade()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            conductor.Stop(handle, 1.0f, Faders.Linear);

            // Fade-out with custom fader is in progress, so the playback remains tracked as playing.
            Object.DestroyImmediate(clip);
            Assert.That(conductor.IsPlaying(handle), Is.True);
        }

        [Test]
        public void Stop_WithFadeTime_AfterFadeCompleted_IsNoLongerPlaying()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");
            conductor.Stop(handle, 1.0f);

            // Advance time past the fade duration to complete the fade-out.
            conductor.Update(1.1f);

            Object.DestroyImmediate(clip);
            Assert.That(conductor.IsPlaying(handle), Is.False);
        }

        [Test]
        public void Stop_WithInvalidHandle_WithFadeTime_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.Stop(default, 1.0f));
        }

        [Test]
        public void Play_WithFadeTimeInOptions_ReturnsValidHandle()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1", new PlayOptions { FadeTime = 1.0f });

            Object.DestroyImmediate(clip);
            Assert.That(handle.IsValid, Is.True);
        }

        [Test]
        public void Play_WithFadeTimeAndCustomFader_ReturnsValidHandle()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1",
                new PlayOptions { FadeTime = 0.5f, Fader = Faders.Linear });

            Object.DestroyImmediate(clip);
            Assert.That(handle.IsValid, Is.True);
        }
    }
}
