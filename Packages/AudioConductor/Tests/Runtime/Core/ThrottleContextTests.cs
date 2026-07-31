// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using ManagedPlayback = AudioConductor.Core.Conductor.ManagedPlayback;
using OneShotPlayback = AudioConductor.Core.Conductor.OneShotPlayback;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class ThrottleContextTests
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
        public void Accumulate_Playback_UpdatesAllScopes()
        {
            var cue = CreateCue(10);
            var state = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 5);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Sheet), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Cue), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Category), Is.EqualTo(1));
        }

        [Test]
        public void Accumulate_StoppedPlayer_DoesNothing()
        {
            var cue = CreateCue();
            var state = new ManagedPlayback(1, 100, cue, CreateStoppedPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(0));
        }

        [Test]
        public void Accumulate_DifferentSheet_SkipsSheetScope()
        {
            var cue = CreateCue(10);
            var state = new ManagedPlayback(1, 200, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Sheet), Is.EqualTo(0));
            Assert.That(ctx.Count(ThrottleScopeKind.Cue), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Category), Is.EqualTo(1));
        }

        [Test]
        public void Accumulate_OneShot_UpdatesAllScopes()
        {
            var cue = CreateCue(10);
            var state = new OneShotPlayback(1, 100, cue, CreatePlayingPlayer(), 5);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Sheet), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Cue), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Category), Is.EqualTo(1));
        }

        [Test]
        public void Resolve_NoEviction_NoChange()
        {
            var cue = CreateCue(10);
            var state = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);
            ctx.Accumulate(state.Core);

            // Limit above current count: no eviction is produced, counts stay untouched.
            var result = ctx.Resolve(ThrottleScopeKind.Global,
                new ThrottleSetting(ThrottleType.FirstComeFirstServed, 5), 0);

            Assert.That(result, Is.True);
            Assert.That(ctx.PendingEviction(ThrottleScopeKind.Global).HasValue, Is.False);
            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Sheet), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Cue), Is.EqualTo(1));
            Assert.That(ctx.Count(ThrottleScopeKind.Category), Is.EqualTo(1));
        }

        [Test]
        public void Resolve_EvictionAtLimit_DecrementsAllMatchingScopes()
        {
            var cue = CreateCue(10);
            var state1 = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 0);
            var state2 = new ManagedPlayback(2, 100, cue, CreatePlayingPlayer(), 0);
            var state3 = new ManagedPlayback(3, 100, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);
            ctx.Accumulate(state1.Core);
            ctx.Accumulate(state2.Core);
            ctx.Accumulate(state3.Core);

            // Count == limit with equal priorities: the oldest (lowest Id) is evicted.
            var result = ctx.Resolve(ThrottleScopeKind.Global,
                new ThrottleSetting(ThrottleType.PriorityOrder, 3), 0);

            Assert.That(result, Is.True);
            Assert.That(ctx.PendingEviction(ThrottleScopeKind.Global)!.Value.Id, Is.EqualTo(1u));
            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(2));
            Assert.That(ctx.Count(ThrottleScopeKind.Sheet), Is.EqualTo(2));
            Assert.That(ctx.Count(ThrottleScopeKind.Cue), Is.EqualTo(2));
            Assert.That(ctx.Count(ThrottleScopeKind.Category), Is.EqualTo(2));
        }

        [Test]
        public void Resolve_EvictionFromDifferentSheet_KeepsSheetCount()
        {
            var cue = CreateCue(10);
            var stateA1 = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 10);
            var stateA2 = new ManagedPlayback(2, 100, cue, CreatePlayingPlayer(), 10);
            var stateB = new ManagedPlayback(3, 200, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);
            ctx.Accumulate(stateA1.Core);
            ctx.Accumulate(stateA2.Core);
            ctx.Accumulate(stateB.Core);

            // stateB has the lowest priority, so it becomes the eviction candidate
            // even though it belongs to a different sheet.
            var result = ctx.Resolve(ThrottleScopeKind.Global,
                new ThrottleSetting(ThrottleType.PriorityOrder, 3), 0);

            Assert.That(result, Is.True);
            Assert.That(ctx.PendingEviction(ThrottleScopeKind.Global)!.Value.Id, Is.EqualTo(3u));
            Assert.That(ctx.Count(ThrottleScopeKind.Global), Is.EqualTo(2));
            Assert.That(ctx.Count(ThrottleScopeKind.Sheet), Is.EqualTo(2));
            Assert.That(ctx.Count(ThrottleScopeKind.Cue), Is.EqualTo(2));
            Assert.That(ctx.Count(ThrottleScopeKind.Category), Is.EqualTo(2));
        }
    }
}
