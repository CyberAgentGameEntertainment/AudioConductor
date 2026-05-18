// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

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

            Assert.That(ctx.GlobalCount, Is.EqualTo(1));
            Assert.That(ctx.SheetCount, Is.EqualTo(1));
            Assert.That(ctx.CueCount, Is.EqualTo(1));
            Assert.That(ctx.CategoryCount, Is.EqualTo(1));
        }

        [Test]
        public void Accumulate_StoppedPlayer_DoesNothing()
        {
            var cue = CreateCue();
            var state = new ManagedPlayback(1, 100, cue, CreateStoppedPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.GlobalCount, Is.EqualTo(0));
        }

        [Test]
        public void Accumulate_DifferentSheet_SkipsSheetScope()
        {
            var cue = CreateCue(10);
            var state = new ManagedPlayback(1, 200, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.GlobalCount, Is.EqualTo(1));
            Assert.That(ctx.SheetCount, Is.EqualTo(0));
            Assert.That(ctx.CueCount, Is.EqualTo(1));
            Assert.That(ctx.CategoryCount, Is.EqualTo(1));
        }

        [Test]
        public void Accumulate_OneShot_UpdatesAllScopes()
        {
            var cue = CreateCue(10);
            var state = new OneShotPlayback(1, 100, cue, CreatePlayingPlayer(), 5);
            var ctx = new ThrottleContext(100, cue);

            ctx.Accumulate(state.Core);

            Assert.That(ctx.GlobalCount, Is.EqualTo(1));
            Assert.That(ctx.SheetCount, Is.EqualTo(1));
            Assert.That(ctx.CueCount, Is.EqualTo(1));
            Assert.That(ctx.CategoryCount, Is.EqualTo(1));
        }

        [Test]
        public void AdjustAfterEviction_Null_NoChange()
        {
            var cue = CreateCue();
            var state = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);
            ctx.Accumulate(state.Core);

            ctx.AdjustAfterEviction(null);

            Assert.That(ctx.GlobalCount, Is.EqualTo(1));
        }

        [Test]
        public void AdjustAfterEviction_MatchingScopes_DecrementsAll()
        {
            var cue = CreateCue(10);
            var state = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);
            ctx.Accumulate(state.Core);
            ctx.Accumulate(state.Core);
            ctx.Accumulate(state.Core);

            ctx.AdjustAfterEviction(state.Core);

            Assert.That(ctx.GlobalCount, Is.EqualTo(2));
            Assert.That(ctx.SheetCount, Is.EqualTo(2));
            Assert.That(ctx.CueCount, Is.EqualTo(2));
            Assert.That(ctx.CategoryCount, Is.EqualTo(2));
        }

        [Test]
        public void AdjustAfterEviction_DifferentSheet_KeepsSheet()
        {
            var cue = CreateCue(10);
            var stateA = new ManagedPlayback(1, 100, cue, CreatePlayingPlayer(), 0);
            var stateB = new ManagedPlayback(2, 200, cue, CreatePlayingPlayer(), 0);
            var ctx = new ThrottleContext(100, cue);
            ctx.Accumulate(stateA.Core);
            ctx.Accumulate(stateA.Core);
            ctx.Accumulate(stateB.Core);

            // evict a playback from a different sheet
            ctx.AdjustAfterEviction(stateB.Core);

            Assert.That(ctx.GlobalCount, Is.EqualTo(2));
            Assert.That(ctx.SheetCount, Is.EqualTo(2));
            Assert.That(ctx.CueCount, Is.EqualTo(2));
            Assert.That(ctx.CategoryCount, Is.EqualTo(2));
        }
    }
}
