// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using UnityEngine;
using CoreAudioConductor = AudioConductor.Runtime.Core.AudioConductor;
using AudioConductorSettings = AudioConductor.Runtime.Core.Models.AudioConductorSettings;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public class AudioConductorFadeTests
    {
        private CueSheetAsset _cueSheetAsset;
        private AudioConductorSettings _settings;

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
        public void Stop_WithFadeTime_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new CoreAudioConductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            Assert.DoesNotThrow(() => conductor.Stop(handle, 1.0f));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Stop_WithFadeTimeAndCustomFader_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new CoreAudioConductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            Assert.DoesNotThrow(() => conductor.Stop(handle, 1.0f, Faders.Linear));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Stop_WithFadeTime_HandleRemainsPlaying()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new CoreAudioConductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            // Fade-out starts: playback is still active until fade completes.
            conductor.Stop(handle, 1.0f);

            // IsPlaying remains true because the fade is ongoing.
            Assert.That(conductor.IsPlaying(handle), Is.True);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Stop_WithInvalidHandle_WithFadeTime_DoesNotThrow()
        {
            using var conductor = new CoreAudioConductor(_settings);

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

            using var conductor = new CoreAudioConductor(_settings);
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

            using var conductor = new CoreAudioConductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1",
                new PlayOptions { FadeTime = 0.5f, Fader = Faders.Linear });

            Object.DestroyImmediate(clip);
            Assert.That(handle.IsValid, Is.True);
        }
    }
}
