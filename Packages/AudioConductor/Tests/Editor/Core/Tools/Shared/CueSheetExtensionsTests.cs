// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal class CueSheetExtensionsTests
    {
        [Test]
        public void Duplicate_NullCueSheet_ReturnsNull()
        {
            CueSheet? cueSheet = null;

            Assert.That(cueSheet.Duplicate(), Is.Null);
        }

        [Test]
        public void Duplicate_CopiesAllFields()
        {
            var cueSheet = new CueSheet
            {
                name = "TestSheet",
                throttleType = ThrottleType.PriorityOrder,
                throttleLimit = 3,
                volume = 0.9f,
                pitch = 1.5f,
                pitchInvert = true,
                cueList = { new Cue { name = "C1" } }
            };

            var result = cueSheet.Duplicate()!;

            Assert.That(result.name, Is.EqualTo("TestSheet"));
            Assert.That(result.throttleType, Is.EqualTo(ThrottleType.PriorityOrder));
            Assert.That(result.throttleLimit, Is.EqualTo(3));
            Assert.That(result.volume, Is.EqualTo(0.9f));
            Assert.That(result.pitch, Is.EqualTo(1.5f));
            Assert.That(result.pitchInvert, Is.True);
        }

        [Test]
        public void Duplicate_CueListIsDeepCopy()
        {
            var cueSheet = new CueSheet
            {
                name = "TestSheet",
                cueList =
                {
                    new Cue { name = "C1" },
                    new Cue { name = "C2" }
                }
            };

            var result = cueSheet.Duplicate()!;

            Assert.That(result.cueList.Count, Is.EqualTo(2));
            Assert.That(result.cueList[0], Is.Not.SameAs(cueSheet.cueList[0]));
            Assert.That(result.cueList[1], Is.Not.SameAs(cueSheet.cueList[1]));
            Assert.That(result.cueList[0].name, Is.EqualTo("C1"));
            Assert.That(result.cueList[1].name, Is.EqualTo("C2"));
        }

        [Test]
        public void Duplicate_ReturnsNewInstance()
        {
            var cueSheet = new CueSheet { name = "Original" };

            var result = cueSheet.Duplicate();

            Assert.That(result, Is.Not.SameAs(cueSheet));
        }

        [Test]
        public void Duplicate_EditorIdIsDifferent()
        {
            var cueSheet = new CueSheet { name = "TestSheet" };

            var result = cueSheet.Duplicate()!;

            Assert.That(result.Id, Is.Not.EqualTo(cueSheet.Id));
        }
    }
}
