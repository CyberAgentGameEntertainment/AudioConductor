// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.SampleGeneration;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.Tests
{
    internal sealed class AudioConductorSampleTests
    {
        [SetUp]
        public void SetUp()
        {
            AudioConductorSample.ClearPendingPhase2State();
        }

        [TearDown]
        public void TearDown()
        {
            AudioConductorSample.ClearPendingPhase2State();
        }

        [Test]
        public void SavePendingPhase2State_ThenConsume_ReturnsSavedValuesAndClearsState()
        {
            AudioConductorSample.SavePendingPhase2State("Assets/Samples/AudioConductorSample", true);

            var state = AudioConductorSample.ConsumePendingPhase2State();
            var afterConsume = AudioConductorSample.ConsumePendingPhase2State();

            Assert.That(state.HasPending, Is.True);
            Assert.That(state.SamplePath, Is.EqualTo("Assets/Samples/AudioConductorSample"));
            Assert.That(state.PostDeploy, Is.True);
            Assert.That(afterConsume.HasPending, Is.False);
        }

        [Test]
        public void ConsumePendingPhase2State_WithoutPendingFlag_ReturnsEmptyState()
        {
            var state = AudioConductorSample.ConsumePendingPhase2State();

            Assert.That(state.HasPending, Is.False);
            Assert.That(string.IsNullOrEmpty(state.SamplePath), Is.True);
            Assert.That(state.PostDeploy, Is.False);
        }

        [Test]
        public void SampleRegistry_FindByName_ReturnsRegisteredSample()
        {
            var sample = SampleRegistry.FindByName("AudioConductorSample");

            Assert.That(sample, Is.Not.Null);
            Assert.That(sample!.DisplayName, Is.EqualTo("Audio Conductor Sample"));
        }

        [Test]
        public void SampleRegistry_FindByName_WithUnknownName_ReturnsNull()
        {
            var sample = SampleRegistry.FindByName("UnknownSample");

            Assert.That(sample, Is.Null);
        }
    }
}
