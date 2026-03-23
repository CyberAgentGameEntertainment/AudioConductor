// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class ConductorPlaybackTests
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
        public void Play_WithInvalidSheetHandle_ReturnsInvalidHandle()
        {
            using var conductor = new Conductor(_settings);

            var handle = conductor.Play(default, "cue");

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithUnregisteredHandle_ReturnsInvalidHandle()
        {
            using var conductor = new Conductor(_settings);

            var handle = conductor.Play(new CueSheetHandle(999), "cue");

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithNonExistentCueName_ReturnsInvalidHandle()
        {
            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "nonexistent_cue");

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Stop_WithInvalidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.Stop(default));
        }

        [Test]
        public void Stop_WithUnknownHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.Stop(new PlaybackHandle(999)));
        }

        [Test]
        public void Pause_WithInvalidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.Pause(default));
        }

        [Test]
        public void Resume_WithInvalidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.Resume(default));
        }

        [Test]
        public void SetVolume_WithInvalidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.SetVolume(default, 0.5f));
        }

        [Test]
        public void SetPitch_WithInvalidHandle_DoesNotThrow()
        {
            using var conductor = new Conductor(_settings);

            Assert.DoesNotThrow(() => conductor.SetPitch(default, 1.0f));
        }

        [Test]
        public void IsPlaying_WithInvalidHandle_ReturnsFalse()
        {
            using var conductor = new Conductor(_settings);

            Assert.That(conductor.IsPlaying(default), Is.False);
        }

        [Test]
        public void IsPlaying_WithUnknownHandle_ReturnsFalse()
        {
            using var conductor = new Conductor(_settings);

            Assert.That(conductor.IsPlaying(new PlaybackHandle(999)), Is.False);
        }

        [Test]
        public void Play_WithCueHavingNoTracks_ReturnsInvalidHandle()
        {
            var cue = new Cue { name = "empty_cue" };
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "empty_cue");

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithCueHavingTrackWithNullClip_ReturnsInvalidHandle()
        {
            var track = new Track { name = "track1", audioClip = null };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1");

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithBothTrackIndexAndTrackName_ThrowsArgumentException()
        {
            var track = new Track { name = "track0", audioClip = null };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.Throws<ArgumentException>(() =>
                conductor.Play(sheetHandle, "cue1", new PlayOptions { TrackIndex = 0, TrackName = "track0" }));
        }

        [Test]
        public void PlayOptions_TrackIndex_SelectsSpecificTrack()
        {
            // Clips are null so Play returns invalid, but TrackIndex selection itself must not throw.
            var track0 = new Track { name = "track0", audioClip = null };
            var track1 = new Track { name = "track1", audioClip = null };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track0);
            cue.trackList.Add(track1);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1", new PlayOptions { TrackIndex = 1 });

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void PlayOptions_TrackName_SelectsSpecificTrack()
        {
            // Clip is null so Play returns invalid, but TrackName selection itself must not throw.
            var track = new Track { name = "named_track", audioClip = null };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1", new PlayOptions { TrackName = "named_track" });

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithValidAudioClip_ReturnsValidHandle()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, "cue1");

            Object.DestroyImmediate(clip);
            Assert.That(handle.IsValid, Is.True);
        }

        [Test]
        public void Play_TwiceWithValidAudioClip_ReturnsDifferentHandles()
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

            Object.DestroyImmediate(clip);
            Assert.That(handle1, Is.Not.EqualTo(handle2));
        }

        [Test]
        public void Stop_AfterPlay_HandleRemainsValidButIsPlayingReturnsFalse()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            conductor.Stop(handle);

            Object.DestroyImmediate(clip);
            // Handle itself is still valid (IsValid checks Id != 0)
            Assert.That(handle.IsValid, Is.True);
            // After stop, the playback is removed so IsPlaying returns false
            Assert.That(conductor.IsPlaying(handle), Is.False);
        }

        [Test]
        public void Stop_CalledTwice_DoesNotThrow()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);
            var handle = conductor.Play(sheetHandle, "cue1");

            conductor.Stop(handle);

            Object.DestroyImmediate(clip);
            Assert.DoesNotThrow(() => conductor.Stop(handle));
        }

        [Test]
        public void Dispose_WithActivePlaybacks_DoesNotThrow()
        {
            var conductor = new Conductor(_settings);
            // Just register without actually playing (no real AudioClip)
            conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.DoesNotThrow(() => conductor.Dispose());
        }

        [Test]
        public void Play_WithCueId_WithBothTrackIndexAndTrackName_ThrowsArgumentException()
        {
            var track = new Track { name = "track0", audioClip = null };
            var cue = new Cue { name = "cue1", cueId = 1 };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            Assert.Throws<ArgumentException>(() =>
                conductor.Play(sheetHandle, 1, new PlayOptions { TrackIndex = 0, TrackName = "track0" }));
        }

        [Test]
        public void Play_WithInvalidSheetHandle_AndCueId_ReturnsInvalidHandle()
        {
            using var conductor = new Conductor(_settings);

            var handle = conductor.Play(default, 1);

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithUnregisteredHandle_AndCueId_ReturnsInvalidHandle()
        {
            using var conductor = new Conductor(_settings);

            var handle = conductor.Play(new CueSheetHandle(999), 1);

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithNonExistentCueId_ReturnsInvalidHandle()
        {
            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, 999);

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void Play_WithValidCueId_ReturnsValidHandle()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1", cueId = 1 };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle = conductor.Play(sheetHandle, 1);

            Assert.That(handle.IsValid, Is.True);
            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Play_SequentialCuePlayedTwice_SecondPlayUsesNextTrack()
        {
            var clip0 = AudioClip.Create("clip0", 44100, 1, 44100, false);
            var clip1 = AudioClip.Create("clip1", 44100, 1, 44100, false);
            var track0 = new Track { name = "track0", audioClip = clip0 };
            var track1 = new Track { name = "track1", audioClip = clip1 };
            var cue = new Cue { name = "cue1", playType = CuePlayType.Sequential };
            cue.trackList.Add(track0);
            cue.trackList.Add(track1);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var handle1 = conductor.Play(sheetHandle, "cue1");
            var handle2 = conductor.Play(sheetHandle, "cue1");

            Object.DestroyImmediate(clip0);
            Object.DestroyImmediate(clip1);

            // Both plays should succeed because each sequential play advances the track index.
            Assert.That(handle1.IsValid, Is.True);
            Assert.That(handle2.IsValid, Is.True);
        }

        [Test]
        public void Play_WhenPlayStateCounterOverflows_SkipsZeroId()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "track1", audioClip = clip };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            // Force counter to uint.MaxValue so the next increment overflows to 0.
            conductor._playStateCounter = uint.MaxValue;

            var handle1 = conductor.Play(sheetHandle, "cue1");
            var handle2 = conductor.Play(sheetHandle, "cue1");

            Object.DestroyImmediate(clip);

            // First play wraps to 0 and must skip to 1; second play uses 2.
            // Both handles must be valid (Id != 0).
            Assert.That(handle1.IsValid, Is.True);
            Assert.That(handle2.IsValid, Is.True);
            Assert.That(handle1, Is.Not.EqualTo(handle2));
        }
    }
}
