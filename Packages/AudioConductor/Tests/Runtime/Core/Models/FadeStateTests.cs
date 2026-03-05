// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Runtime.Core.Models
{
    public class FadeStateTests
    {
        [Test]
        public void Elapsed_WhenFadeTimeIsZero_SetsTargetVolumeImmediately()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable, Faders.Linear, 0f, 1f, 0f, false);

            fadeState.Elapsed(0f);

            Assert.That(fadeable.Volume, Is.EqualTo(1f));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZero_SetsIsFinishedToTrue()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable, Faders.Linear, 0f, 1f, 0f, false);

            var result = fadeState.Elapsed(0f);

            Assert.That(result, Is.True);
            Assert.That(fadeState.IsFinished, Is.True);
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZeroAndIsStopTarget_CallsStop()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable, Faders.Linear, 0f, 0f, 0f, true);

            fadeState.Elapsed(0f);

            Assert.That(fadeable.StopCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZeroAndIsNotStopTarget_DoesNotCallStop()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable, Faders.Linear, 0f, 1f, 0f, false);

            fadeState.Elapsed(0f);

            Assert.That(fadeable.StopCallCount, Is.EqualTo(0));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsPositive_InterpolatesVolume()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable, Faders.Linear, 0f, 1f, 1f, false);

            fadeState.Elapsed(0.5f);

            Assert.That(fadeable.Volume, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(fadeState.IsFinished, Is.False);
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsPositiveAndElapsed_ReturnsTrue()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable, Faders.Linear, 0f, 1f, 1f, false);

            var result = fadeState.Elapsed(1f);

            Assert.That(result, Is.True);
            Assert.That(fadeState.IsFinished, Is.True);
            Assert.That(fadeable.Volume, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Setup_CanBeCalledTwice_ResetsState()
        {
            var fadeable1 = new FakeIFadeable();
            var fadeable2 = new FakeIFadeable();
            var fadeState = new FadeState();
            fadeState.Setup(fadeable1, Faders.Linear, 0f, 1f, 1f, false);
            fadeState.Elapsed(0.5f);

            fadeState.Setup(fadeable2, Faders.Linear, 0f, 0.5f, 2f, true);

            Assert.That(fadeState.Fadeable, Is.SameAs(fadeable2));
            Assert.That(fadeState.ElapsedTime, Is.EqualTo(0f));
            Assert.That(fadeState.IsFinished, Is.False);
        }

        private sealed class FakeIFadeable : IFadeable
        {
            public float Volume { get; private set; }
            public int StopCallCount { get; private set; }

            public float VolumeFade { get; private set; } = 1f;

            public void SetVolumeFade(float fade)
            {
                VolumeFade = fade;
                Volume = fade;
            }

            public void Stop()
            {
                StopCallCount++;
            }
        }
    }
}
