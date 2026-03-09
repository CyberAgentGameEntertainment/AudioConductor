// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using UnityEngine;
using AudioConductorSettings = AudioConductor.Runtime.Core.Models.AudioConductorSettings;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public class ConductorVolumeTests
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
        public void GetMasterVolume_DefaultValue_ReturnsOne()
        {
            using var conductor = new Conductor(_settings);

            Assert.That(conductor.GetMasterVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void SetMasterVolume_ThenGetMasterVolume_ReturnsSameValue()
        {
            using var conductor = new Conductor(_settings);

            conductor.SetMasterVolume(0.5f);

            Assert.That(conductor.GetMasterVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetMasterVolume_BelowZero_ClampedToZero()
        {
            using var conductor = new Conductor(_settings);

            conductor.SetMasterVolume(-0.5f);

            Assert.That(conductor.GetMasterVolume(), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void SetMasterVolume_AboveOne_ClampedToOne()
        {
            using var conductor = new Conductor(_settings);

            conductor.SetMasterVolume(1.5f);

            Assert.That(conductor.GetMasterVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void StopAll_WithNoPlayback_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.StopAll());
        }

        [Test]
        public void StopAll_WithActivePlayback_StopsAllPlaybacks()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle1 = conductor.Play(sheetHandle, "cue1");
            var handle2 = conductor.Play(sheetHandle, "cue1");

            conductor.StopAll();

            Object.DestroyImmediate(clip);
            Assert.That(conductor.IsPlaying(handle1), Is.False);
            Assert.That(conductor.IsPlaying(handle2), Is.False);
        }

        [Test]
        public void StopAll_WithFadeTime_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            conductor.Play(sheetHandle, "cue1");

            Assert.DoesNotThrow(() => conductor.StopAll(1.0f));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void StopAll_WithActiveOneShotPlayback_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            conductor.PlayOneShot(sheetHandle, "cue1");

            Assert.DoesNotThrow(() => conductor.StopAll());

            Object.DestroyImmediate(clip);
        }
    }
}
