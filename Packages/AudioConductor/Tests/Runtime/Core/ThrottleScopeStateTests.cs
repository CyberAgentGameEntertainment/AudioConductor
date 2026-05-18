// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using ManagedPlayback = AudioConductor.Core.Conductor.ManagedPlayback;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class ThrottleScopeStateTests
    {
        private static Cue CreateCue(int categoryId = 0)
        {
            return new Cue { categoryId = categoryId };
        }

        private static AudioClipPlayer CreatePlayingPlayer()
        {
            var source = new SpyAudioSourceWrapper { IsPlaying = true };
            return new AudioClipPlayer(
                new IAudioSourceWrapper[] { source, new SpyAudioSourceWrapper() },
                new StubDspClock(),
                NullLifecycle.Instance);
        }

        private static AudioClipPlayer CreateStoppedPlayer()
        {
            return new AudioClipPlayer(
                new IAudioSourceWrapper[] { new SpyAudioSourceWrapper(), new SpyAudioSourceWrapper() },
                new StubDspClock(),
                NullLifecycle.Instance);
        }

        [Test]
        public void Resolve_LimitZero_ReturnsTrue()
        {
            var state = default(ThrottleScopeState);
            var result = state.Resolve(ThrottleType.FirstComeFirstServed, 0, 0, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction.HasValue, Is.False);
        }

        [Test]
        public void Resolve_BelowLimit_ReturnsTrue()
        {
            var state = default(ThrottleScopeState);
            state.Count = 2;
            var result = state.Resolve(ThrottleType.FirstComeFirstServed, 3, 0, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction.HasValue, Is.False);
        }

        [Test]
        public void Resolve_AtLimit_FirstComeFirstServed_ReturnsFalse()
        {
            var state = default(ThrottleScopeState);
            state.Count = 2;
            var result = state.Resolve(ThrottleType.FirstComeFirstServed, 2, 0, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void Resolve_AtLimit_PriorityOrder_WithCandidate_ReturnsTrue()
        {
            var cue = CreateCue();
            var managed = new ManagedPlayback(1, 1, cue, CreatePlayingPlayer(), 0);
            var state = default(ThrottleScopeState);
            state.Count = 1;
            state.Oldest = managed.Core;
            var result = state.Resolve(ThrottleType.PriorityOrder, 1, 0, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction!.Value.Id, Is.EqualTo(1u));
        }

        [Test]
        public void Resolve_AtLimit_MinPriorityHigherThanTrack_ReturnsFalse()
        {
            var state = default(ThrottleScopeState);
            state.Count = 1;
            state.Min = 5;
            var result = state.Resolve(ThrottleType.PriorityOrder, 1, 0, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void Resolve_AtLimit_MinPriorityLowerThanTrack_ForcesEviction()
        {
            var cue = CreateCue();
            var managed = new ManagedPlayback(1, 1, cue, CreatePlayingPlayer(), 0);
            var state = default(ThrottleScopeState);
            state.Count = 1;
            state.Oldest = managed.Core;
            var result = state.Resolve(ThrottleType.FirstComeFirstServed, 1, 5, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction!.Value.Id, Is.EqualTo(1u));
        }

        [Test]
        public void Accumulate_DefaultState_SetsOldest()
        {
            var cue = CreateCue();
            var managed = new ManagedPlayback(1, 1, cue, CreatePlayingPlayer(), 3);
            var state = default(ThrottleScopeState);
            state.Accumulate(managed.Core);
            Assert.That(state.Oldest.HasValue, Is.True);
            Assert.That(state.Oldest!.Value.Id, Is.EqualTo(1u));
        }
    }
}
