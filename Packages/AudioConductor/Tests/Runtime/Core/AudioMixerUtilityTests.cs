// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    public class AudioMixerUtilityTests
    {
        [Test]
        public void ToDecibel_WithZeroInput_ReturnsMinus80()
        {
            var result = AudioMixerUtility.ToDecibel(0f);
            Assert.That(result, Is.EqualTo(-80f));
        }

        [Test]
        public void ToDecibel_WithNegativeInput_ReturnsMinus80()
        {
            var result = AudioMixerUtility.ToDecibel(-1f);
            Assert.That(result, Is.EqualTo(-80f));
        }

        [Test]
        public void ToDecibel_WithLinear1_Returns0dB()
        {
            var result = AudioMixerUtility.ToDecibel(1f);
            Assert.That(result, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void ToDecibel_WithLinear0point5_ReturnsApproxMinus6dB()
        {
            var result = AudioMixerUtility.ToDecibel(0.5f);
            Assert.That(result, Is.EqualTo(-6.0206f).Within(0.01f));
        }

        [Test]
        public void ToLinear_WithMinus80dB_Returns0()
        {
            var result = AudioMixerUtility.ToLinear(-80f);
            Assert.That(result, Is.EqualTo(0f));
        }

        [Test]
        public void ToLinear_WithBelowMinus80dB_Returns0()
        {
            var result = AudioMixerUtility.ToLinear(-100f);
            Assert.That(result, Is.EqualTo(0f));
        }

        [Test]
        public void ToLinear_With0dB_Returns1()
        {
            var result = AudioMixerUtility.ToLinear(0f);
            Assert.That(result, Is.EqualTo(1f).Within(0.001f));
        }

        [Test]
        public void ToLinear_WithMinus6dB_ReturnsApprox0point5()
        {
            var result = AudioMixerUtility.ToLinear(-6f);
            Assert.That(result, Is.EqualTo(0.5f).Within(0.01f));
        }

        [TestCase(0.1f)]
        [TestCase(0.5f)]
        [TestCase(1.0f)]
        [TestCase(0.75f)]
        public void RoundTrip_ToLinearOfToDecibel_ReturnsOriginalValue(float linear)
        {
            var roundTrip = AudioMixerUtility.ToLinear(AudioMixerUtility.ToDecibel(linear));
            Assert.That(roundTrip, Is.EqualTo(linear).Within(0.0001f));
        }
    }
}
