// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core;
using NUnit.Framework;

namespace AudioConductor.Tests.Runtime.Core
{
    public class CueSheetHandleTests
    {
        [Test]
        public void IsValid_WithDefaultHandle_ReturnsFalse()
        {
            var handle = default(CueSheetHandle);

            Assert.That(handle.IsValid, Is.False);
        }

        [Test]
        public void IsValid_WithNonZeroId_ReturnsTrue()
        {
            var handle = new CueSheetHandle(1);

            Assert.That(handle.IsValid, Is.True);
        }

        [Test]
        public void Equals_WithSameId_ReturnsTrue()
        {
            var a = new CueSheetHandle(1);
            var b = new CueSheetHandle(1);

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void Equals_WithDifferentId_ReturnsFalse()
        {
            var a = new CueSheetHandle(1);
            var b = new CueSheetHandle(2);

            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void EqualityOperator_WithSameId_ReturnsTrue()
        {
            var a = new CueSheetHandle(1);
            var b = new CueSheetHandle(1);

            Assert.That(a == b, Is.True);
        }

        [Test]
        public void InequalityOperator_WithDifferentId_ReturnsTrue()
        {
            var a = new CueSheetHandle(1);
            var b = new CueSheetHandle(2);

            Assert.That(a != b, Is.True);
        }

        [Test]
        public void GetHashCode_WithSameId_ReturnsSameValue()
        {
            var a = new CueSheetHandle(42);
            var b = new CueSheetHandle(42);

            Assert.That(a.GetHashCode(), Is.EqualTo(b.GetHashCode()));
        }
    }
}
