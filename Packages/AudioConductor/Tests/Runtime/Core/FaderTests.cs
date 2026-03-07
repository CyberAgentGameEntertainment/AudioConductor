// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Tests.Runtime.Core
{
    public class FaderTests
    {
        [Test]
        public void Linear_Evaluate_AtZero_ReturnsStartVolume()
        {
            Assert.That(Faders.Linear.Evaluate(0f, 0.2f, 0.8f), Is.EqualTo(0.2f).Within(0.0001f));
        }

        [Test]
        public void Linear_Evaluate_AtOne_ReturnsTargetVolume()
        {
            Assert.That(Faders.Linear.Evaluate(1f, 0.2f, 0.8f), Is.EqualTo(0.8f).Within(0.0001f));
        }

        [Test]
        public void Linear_Evaluate_AtHalf_ReturnsMidpoint()
        {
            Assert.That(Faders.Linear.Evaluate(0.5f, 0f, 1f), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void Linear_Evaluate_MatchesMathfLerp()
        {
            var t = 0.3f;
            var start = 0.1f;
            var target = 0.9f;

            Assert.That(Faders.Linear.Evaluate(t, start, target),
                Is.EqualTo(Mathf.Lerp(start, target, t)).Within(0.0001f));
        }

        [Test]
        public void Linear_IsSingletonInstance()
        {
            Assert.That(Faders.Linear, Is.SameAs(Faders.Linear));
        }
    }
}
