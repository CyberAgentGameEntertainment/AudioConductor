// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal class TrackExtensionsTests
    {
        [Test]
        public void Duplicate_NullTrack_ReturnsNull()
        {
            Track? track = null;

            Assert.That(track.Duplicate(), Is.Null);
        }

        [Test]
        public void Duplicate_CopiesAllFields()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track
            {
                name = "TestTrack",
                audioClip = clip,
                volume = 0.8f,
                volumeRange = 0.1f,
                pitch = 1.2f,
                pitchRange = 0.05f,
                pitchInvert = true,
                startSample = 100,
                endSample = 44000,
                loopStartSample = 200,
                isLoop = true,
                randomWeight = 5,
                priority = 3,
                fadeTime = 0.5f
            };

            var result = track.Duplicate()!;

            Assert.That(result.name, Is.EqualTo("TestTrack"));
            Assert.That(result.audioClip, Is.SameAs(clip));
            Assert.That(result.volume, Is.EqualTo(0.8f));
            Assert.That(result.volumeRange, Is.EqualTo(0.1f));
            Assert.That(result.pitch, Is.EqualTo(1.2f));
            Assert.That(result.pitchRange, Is.EqualTo(0.05f));
            Assert.That(result.pitchInvert, Is.True);
            Assert.That(result.startSample, Is.EqualTo(100));
            Assert.That(result.endSample, Is.EqualTo(44000));
            Assert.That(result.loopStartSample, Is.EqualTo(200));
            Assert.That(result.isLoop, Is.True);
            Assert.That(result.randomWeight, Is.EqualTo(5));
            Assert.That(result.priority, Is.EqualTo(3));
            Assert.That(result.fadeTime, Is.EqualTo(0.5f));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Duplicate_ReturnsNewInstance()
        {
            var track = new Track { name = "Original" };

            var result = track.Duplicate();

            Assert.That(result, Is.Not.SameAs(track));
        }

        [Test]
        public void Duplicate_AudioClipIsSharedReference()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            var track = new Track { name = "Track", audioClip = clip };

            var result = track.Duplicate()!;

            Assert.That(result.audioClip, Is.SameAs(clip));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Duplicate_EditorIdIsDifferent()
        {
            var track = new Track { name = "Track" };

            var result = track.Duplicate()!;

            Assert.That(result.Id, Is.Not.EqualTo(track.Id));
        }
    }
}
