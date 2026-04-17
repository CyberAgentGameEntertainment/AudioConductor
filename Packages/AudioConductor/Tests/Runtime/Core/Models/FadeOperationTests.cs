// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using NUnit.Framework;

namespace AudioConductor.Core.Models.Tests
{
    public class FadeOperationTests
    {
        [Test]
        public void Elapsed_WhenFadeTimeIsZero_SetsTargetVolumeImmediately()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable, Faders.Linear, 0f, 1f, 0f);

            fadeOperation.Elapsed(0f);

            Assert.That(fadeable.VolumeFade, Is.EqualTo(1f));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsZero_SetsIsFinishedToTrue()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable, Faders.Linear, 0f, 1f, 0f);

            var result = fadeOperation.Elapsed(0f);

            Assert.That(result, Is.True);
            Assert.That(fadeOperation.IsFinished, Is.True);
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsPositive_InterpolatesVolume()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable, Faders.Linear, 0f, 1f, 1f);

            fadeOperation.Elapsed(0.5f);

            Assert.That(fadeable.VolumeFade, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(fadeOperation.IsFinished, Is.False);
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsPositiveAndElapsed_ReturnsTrue()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable, Faders.Linear, 0f, 1f, 1f);

            var result = fadeOperation.Elapsed(1f);

            Assert.That(result, Is.True);
            Assert.That(fadeOperation.IsFinished, Is.True);
            Assert.That(fadeable.VolumeFade, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Setup_SetsId()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(42, fadeable, Faders.Linear, 0f, 1f, 1f);

            Assert.That(fadeOperation.Id, Is.EqualTo(42u));
        }

        [Test]
        public void Elapsed_WhenFadeTimeIsNegative_SetsTargetVolumeImmediately()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable, Faders.Linear, 0f, 1f, -1f);

            var result = fadeOperation.Elapsed(0f);

            Assert.That(result, Is.True);
            Assert.That(fadeOperation.IsFinished, Is.True);
            Assert.That(fadeable.VolumeFade, Is.EqualTo(1f));
        }

        [Test]
        public void Elapsed_WhenDeltaTimeExceedsFadeTime_CompletesAndClampsToTargetVolume()
        {
            var fadeable = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable, Faders.Linear, 0f, 1f, 0.5f);

            var result = fadeOperation.Elapsed(1f);

            Assert.That(result, Is.True);
            Assert.That(fadeOperation.IsFinished, Is.True);
            Assert.That(fadeable.VolumeFade, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void Setup_CanBeCalledTwice_ResetsState()
        {
            var fadeable1 = new FakeIFadeable();
            var fadeable2 = new FakeIFadeable();
            var fadeOperation = new FadeOperation();
            fadeOperation.Setup(1, fadeable1, Faders.Linear, 0f, 1f, 1f);
            fadeOperation.Elapsed(0.5f);

            fadeOperation.Setup(2, fadeable2, Faders.Linear, 0f, 0.5f, 2f);

            Assert.That(fadeOperation.Fadeable, Is.SameAs(fadeable2));
            Assert.That(fadeOperation.ElapsedTime, Is.EqualTo(0f));
            Assert.That(fadeOperation.IsFinished, Is.False);
        }

        private sealed class FakeIFadeable : IFadeable
        {
            public uint ActiveFadeId { get; set; }
            public FadeState FadeState { get; set; }
            public float VolumeFade { get; private set; } = 1f;

            public void SetVolumeFade(float fade)
            {
                VolumeFade = fade;
            }
        }
    }
}
