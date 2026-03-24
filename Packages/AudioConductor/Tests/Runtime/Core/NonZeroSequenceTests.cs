// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    public class NonZeroSequenceTests
    {
        [Test]
        public void Next_StartingFromZero_ReturnsOne()
        {
            var seq = new NonZeroSequence(0);

            var result = seq.Next();

            Assert.That(result, Is.EqualTo(1u));
        }

        [Test]
        public void Next_CalledTwice_ReturnsConsecutiveValues()
        {
            var seq = new NonZeroSequence(0);

            var first = seq.Next();
            var second = seq.Next();

            Assert.That(first, Is.EqualTo(1u));
            Assert.That(second, Is.EqualTo(2u));
        }

        [Test]
        public void Next_WhenAtMaxValue_SkipsZeroAndReturnsOne()
        {
            var seq = new NonZeroSequence(uint.MaxValue);

            var result = seq.Next();

            Assert.That(result, Is.EqualTo(1u));
        }

        [Test]
        public void Next_WhenAtMaxValueMinusOne_ReturnsMaxValue()
        {
            var seq = new NonZeroSequence(uint.MaxValue - 1u);

            var result = seq.Next();

            Assert.That(result, Is.EqualTo(uint.MaxValue));
        }
    }
}
