// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace Tests.Runtime.Core.Models
{
    public class FadeStateTests
    {
        [Test]
        public void Elapsed_WhenFadeTimeIsZero_SetsTargetVolumeImmediately()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState(fadeable);
            fadeState.Setup(0f, 1f, 0f, false);

            fadeState.Elapsed(0f);

            Assert.That(fadeable.Volume, Is.EqualTo(1f));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZero_SetsIsFinishedToTrue()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState(fadeable);
            fadeState.Setup(0f, 1f, 0f, false);

            var result = fadeState.Elapsed(0f);

            Assert.That(result, Is.True);
            Assert.That(fadeState.IsFinished, Is.True);
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZeroAndIsStopTarget_CallsStop()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState(fadeable);
            fadeState.Setup(0f, 0f, 0f, true);

            fadeState.Elapsed(0f);

            Assert.That(fadeable.StopCallCount, Is.EqualTo(1));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZeroAndIsNotStopTarget_DoesNotCallStop()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState(fadeable);
            fadeState.Setup(0f, 1f, 0f, false);

            fadeState.Elapsed(0f);

            Assert.That(fadeable.StopCallCount, Is.EqualTo(0));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsPositive_InterpolatesVolume()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState(fadeable);
            fadeState.Setup(0f, 1f, 1f, false);

            fadeState.Elapsed(0.5f);

            Assert.That(fadeable.Volume, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(fadeState.IsFinished, Is.False);
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsPositiveAndElapsed_ReturnsTrue()
        {
            var fadeable = new FakeIFadeable();
            var fadeState = new FadeState(fadeable);
            fadeState.Setup(0f, 1f, 1f, false);

            var result = fadeState.Elapsed(1f);

            Assert.That(result, Is.True);
            Assert.That(fadeState.IsFinished, Is.True);
            Assert.That(fadeable.Volume, Is.EqualTo(1f).Within(0.0001f));
        }

        private sealed class FakeIFadeable : IFadeable
        {
            public float Volume { get; private set; }
            public int StopCallCount { get; private set; }

            public void SetVolumeInternal(float volume)
            {
                Volume = volume;
            }

            public void Stop()
            {
                StopCallCount++;
            }
        }
    }
}
