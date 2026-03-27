// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using Playback = AudioConductor.Core.Conductor.Playback;
using ManagedPlayback = AudioConductor.Core.Conductor.ManagedPlayback;
using OneShotPlayback = AudioConductor.Core.Conductor.OneShotPlayback;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class ThrottleResolverTests
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
                NullLifecycle.Instance
            );
        }

        private static AudioClipPlayer CreateStoppedPlayer()
        {
            return new AudioClipPlayer(
                new IAudioSourceWrapper[] { new SpyAudioSourceWrapper(), new SpyAudioSourceWrapper() },
                new StubDspClock(),
                NullLifecycle.Instance
            );
        }

        [Test]
        public void ResolveThrottle_LimitZero_ReturnsTrue()
        {
            var result = ThrottleResolver.ResolveThrottle(
                ThrottleType.FirstComeFirstServed, 0,
                10, 0, 0, null, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction.HasValue, Is.False);
        }

        [Test]
        public void ResolveThrottle_BelowLimit_ReturnsTrue()
        {
            var result = ThrottleResolver.ResolveThrottle(
                ThrottleType.FirstComeFirstServed, 3,
                2, 0, 0, null, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction.HasValue, Is.False);
        }

        [Test]
        public void ResolveThrottle_AtLimit_FirstComeFirstServed_ReturnsFalse()
        {
            var result = ThrottleResolver.ResolveThrottle(
                ThrottleType.FirstComeFirstServed, 2,
                2, 0, 0, null, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ResolveThrottle_AtLimit_PriorityOrder_WithCandidate_ReturnsTrue()
        {
            var player = CreatePlayingPlayer();
            var cue = CreateCue();
            var managed = new ManagedPlayback(1, 1, cue, player, 0);
            var result = ThrottleResolver.ResolveThrottle(
                ThrottleType.PriorityOrder, 1,
                1, 0, 0, managed.Core, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction!.Value.Id, Is.EqualTo(1u));
        }

        [Test]
        public void ResolveThrottle_AtLimit_MinPriorityHigherThanTrack_ReturnsFalse()
        {
            // All existing sounds have higher priority (value > trackPriority) → reject.
            var result = ThrottleResolver.ResolveThrottle(
                ThrottleType.PriorityOrder, 1,
                1, 5, 0, null, out _);
            Assert.That(result, Is.False);
        }

        [Test]
        public void ResolveThrottle_AtLimit_MinPriorityLowerThanTrack_ForcesEviction()
        {
            // Existing sounds have lower priority → force PriorityOrder eviction regardless of throttleType.
            var player = CreatePlayingPlayer();
            var cue = CreateCue();
            var managed = new ManagedPlayback(1, 1, cue, player, 0);
            var result = ThrottleResolver.ResolveThrottle(
                ThrottleType.FirstComeFirstServed, 1,
                1, 0, 5, managed.Core, out var eviction);
            Assert.That(result, Is.True);
            Assert.That(eviction!.Value.Id, Is.EqualTo(1u));
        }

        [Test]
        public void AccumulateAllScopes_Playback_CountsAndTracksOldest()
        {
            var cue = CreateCue(10);
            var player = CreatePlayingPlayer();
            var state = new ManagedPlayback(1, 100, cue, player, 5);

            int cueCount = 0, sheetCount = 0, catCount = 0, globalCount = 0;
            int cueMin = int.MaxValue, sheetMin = int.MaxValue, catMin = int.MaxValue, globalMin = int.MaxValue;
            Playback? cueOldest = null, sheetOldest = null, catOldest = null, globalOldest = null;

            ThrottleResolver.AccumulateAllScopes(state.Core, 100, cue, 10,
                ref cueCount, ref cueMin, ref cueOldest,
                ref sheetCount, ref sheetMin, ref sheetOldest,
                ref catCount, ref catMin, ref catOldest,
                ref globalCount, ref globalMin, ref globalOldest);

            Assert.That(globalCount, Is.EqualTo(1));
            Assert.That(sheetCount, Is.EqualTo(1));
            Assert.That(cueCount, Is.EqualTo(1));
            Assert.That(catCount, Is.EqualTo(1));
            Assert.That(globalOldest!.Value.Id, Is.EqualTo(1u));
        }

        [Test]
        public void AccumulateAllScopes_Playback_SkipsStoppedPlayer()
        {
            var cue = CreateCue();
            var player = CreateStoppedPlayer();
            var state = new ManagedPlayback(1, 100, cue, player, 0);

            int cueCount = 0, sheetCount = 0, catCount = 0, globalCount = 0;
            int cueMin = int.MaxValue, sheetMin = int.MaxValue, catMin = int.MaxValue, globalMin = int.MaxValue;
            Playback? cueOldest = null, sheetOldest = null, catOldest = null, globalOldest = null;

            ThrottleResolver.AccumulateAllScopes(state.Core, 100, cue, 0,
                ref cueCount, ref cueMin, ref cueOldest,
                ref sheetCount, ref sheetMin, ref sheetOldest,
                ref catCount, ref catMin, ref catOldest,
                ref globalCount, ref globalMin, ref globalOldest);

            Assert.That(globalCount, Is.EqualTo(0));
        }

        [Test]
        public void AccumulateAllScopes_Playback_DifferentSheet_CountsGlobalCategoryAndCue_SkipsSheet()
        {
            var cue = CreateCue(10);
            var player = CreatePlayingPlayer();
            var state = new ManagedPlayback(1, 200, cue, player, 0);

            int cueCount = 0, sheetCount = 0, catCount = 0, globalCount = 0;
            int cueMin = int.MaxValue, sheetMin = int.MaxValue, catMin = int.MaxValue, globalMin = int.MaxValue;
            Playback? cueOldest = null, sheetOldest = null, catOldest = null, globalOldest = null;

            // Target sheet is 100, but state's sheet is 200
            ThrottleResolver.AccumulateAllScopes(state.Core, 100, cue, 10,
                ref cueCount, ref cueMin, ref cueOldest,
                ref sheetCount, ref sheetMin, ref sheetOldest,
                ref catCount, ref catMin, ref catOldest,
                ref globalCount, ref globalMin, ref globalOldest);

            Assert.That(globalCount, Is.EqualTo(1));
            Assert.That(sheetCount, Is.EqualTo(0));
            Assert.That(cueCount, Is.EqualTo(1));
            Assert.That(catCount, Is.EqualTo(1));
        }

        [Test]
        public void AccumulateAllScopes_OneShot_CountsAndTracksOldest()
        {
            var cue = CreateCue(10);
            var player = CreatePlayingPlayer();
            var state = new OneShotPlayback(1, 100, cue, player, 5);

            int cueCount = 0, sheetCount = 0, catCount = 0, globalCount = 0;
            int cueMin = int.MaxValue, sheetMin = int.MaxValue, catMin = int.MaxValue, globalMin = int.MaxValue;
            Playback? cueOldest = null, sheetOldest = null, catOldest = null, globalOldest = null;

            ThrottleResolver.AccumulateAllScopes(state.Core, 100, cue, 10,
                ref cueCount, ref cueMin, ref cueOldest,
                ref sheetCount, ref sheetMin, ref sheetOldest,
                ref catCount, ref catMin, ref catOldest,
                ref globalCount, ref globalMin, ref globalOldest);

            Assert.That(globalCount, Is.EqualTo(1));
            Assert.That(sheetCount, Is.EqualTo(1));
            Assert.That(cueCount, Is.EqualTo(1));
            Assert.That(catCount, Is.EqualTo(1));
            Assert.That(globalOldest!.Value.Id, Is.EqualTo(1u));
        }

        [Test]
        public void AdjustCountsAfterEviction_ZeroId_NoChange()
        {
            Playback? eviction = null;
            var cue = CreateCue();
            int cueCount = 3, sheetCount = 3, catCount = 3, globalCount = 3;

            ThrottleResolver.AdjustCountsAfterEviction(eviction, 100, cue, 0,
                ref cueCount, ref sheetCount, ref catCount, ref globalCount);

            Assert.That(globalCount, Is.EqualTo(3));
        }

        [Test]
        public void AdjustCountsAfterEviction_MatchingScopes_DecrementsAll()
        {
            var cue = CreateCue(10);
            var eviction = new Playback(1, 100, cue, CreateStoppedPlayer(), 0);
            int cueCount = 3, sheetCount = 3, catCount = 3, globalCount = 3;

            ThrottleResolver.AdjustCountsAfterEviction(eviction, 100, cue, 10,
                ref cueCount, ref sheetCount, ref catCount, ref globalCount);

            Assert.That(globalCount, Is.EqualTo(2));
            Assert.That(sheetCount, Is.EqualTo(2));
            Assert.That(cueCount, Is.EqualTo(2));
            Assert.That(catCount, Is.EqualTo(2));
        }

        [Test]
        public void AdjustCountsAfterEviction_DifferentSheet_OnlyDecrementsGlobalAndCategory()
        {
            var cue = CreateCue(10);
            var eviction = new Playback(1, 200, cue, CreateStoppedPlayer(), 0);
            int cueCount = 3, sheetCount = 3, catCount = 3, globalCount = 3;

            ThrottleResolver.AdjustCountsAfterEviction(eviction, 100, cue, 10,
                ref cueCount, ref sheetCount, ref catCount, ref globalCount);

            Assert.That(globalCount, Is.EqualTo(2));
            Assert.That(sheetCount, Is.EqualTo(3));
            Assert.That(cueCount, Is.EqualTo(2));
            Assert.That(catCount, Is.EqualTo(2));
        }
    }
}
