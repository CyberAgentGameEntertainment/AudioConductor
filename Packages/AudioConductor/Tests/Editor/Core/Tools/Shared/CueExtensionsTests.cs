// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal class CueExtensionsTests
    {
        [Test]
        public void Duplicate_NullCue_ReturnsNull()
        {
            Cue? cue = null;

            Assert.That(cue.Duplicate(), Is.Null);
        }

        [Test]
        public void Duplicate_CopiesAllFields()
        {
            var cue = new Cue
            {
                name = "TestCue",
                categoryId = 2,
                throttleType = ThrottleType.PriorityOrder,
                throttleLimit = 5,
                volume = 0.7f,
                volumeRange = 0.2f,
                pitch = 1.1f,
                pitchRange = 0.3f,
                pitchInvert = true,
                playType = CuePlayType.Random,
                trackList = { new Track { name = "T1" } }
            };

            var result = cue.Duplicate()!;

            Assert.That(result.name, Is.EqualTo("TestCue"));
            Assert.That(result.categoryId, Is.EqualTo(2));
            Assert.That(result.throttleType, Is.EqualTo(ThrottleType.PriorityOrder));
            Assert.That(result.throttleLimit, Is.EqualTo(5));
            Assert.That(result.volume, Is.EqualTo(0.7f));
            Assert.That(result.volumeRange, Is.EqualTo(0.2f));
            Assert.That(result.pitch, Is.EqualTo(1.1f));
            Assert.That(result.pitchRange, Is.EqualTo(0.3f));
            Assert.That(result.pitchInvert, Is.True);
            Assert.That(result.playType, Is.EqualTo(CuePlayType.Random));
        }

        [Test]
        public void Duplicate_CueIdIsOmitted()
        {
            var cue = new Cue
            {
                name = "TestCue",
                cueId = 42,
                trackList = { new Track { name = "T1" } }
            };

            var result = cue.Duplicate()!;

            Assert.That(result.cueId, Is.EqualTo(0));
        }

        [Test]
        public void Duplicate_TrackListIsDeepCopy()
        {
            var cue = new Cue
            {
                name = "TestCue",
                trackList =
                {
                    new Track { name = "T1" },
                    new Track { name = "T2" }
                }
            };

            var result = cue.Duplicate()!;

            Assert.That(result.trackList.Count, Is.EqualTo(2));
            Assert.That(result.trackList[0], Is.Not.SameAs(cue.trackList[0]));
            Assert.That(result.trackList[1], Is.Not.SameAs(cue.trackList[1]));
            Assert.That(result.trackList[0].name, Is.EqualTo("T1"));
            Assert.That(result.trackList[1].name, Is.EqualTo("T2"));
        }

        [Test]
        public void Duplicate_ReturnsNewInstance()
        {
            var cue = new Cue { name = "Original" };

            var result = cue.Duplicate();

            Assert.That(result, Is.Not.SameAs(cue));
        }

        [Test]
        public void Duplicate_EditorIdIsDifferent()
        {
            var cue = new Cue { name = "TestCue" };

            var result = cue.Duplicate()!;

            Assert.That(result.Id, Is.Not.EqualTo(cue.Id));
        }
    }
}
