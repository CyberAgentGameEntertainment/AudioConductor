// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    public class TrackSelectorTests
    {
        private static TrackSelectionContext MakeContext(int trackCount, int currentIndex = -1)
        {
            var tracks = new List<Track>();
            for (var i = 0; i < trackCount; i++)
                tracks.Add(new Track { name = $"Track{i}", randomWeight = 1 });
            return new TrackSelectionContext(tracks) { CurrentIndex = currentIndex };
        }

        [Test]
        public void Sequential_EmptyTracks_ReturnsMinusOne()
        {
            var ctx = MakeContext(0);
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(-1));
        }

        [Test]
        public void Sequential_SingleTrack_AlwaysReturnsZero()
        {
            var ctx = MakeContext(1);
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(0));
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(0));
        }

        [Test]
        public void Sequential_MultipleTracksWraps()
        {
            var ctx = MakeContext(3);
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(0));
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(1));
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(2));
            // Wrap around
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(0));
        }

        [Test]
        public void Sequential_UpdatesContextCurrentIndex()
        {
            var ctx = MakeContext(3);
            TrackSelectors.Sequential.SelectNext(ctx);
            Assert.That(ctx.CurrentIndex, Is.EqualTo(0));
            TrackSelectors.Sequential.SelectNext(ctx);
            Assert.That(ctx.CurrentIndex, Is.EqualTo(1));
        }

        [Test]
        public void Sequential_IsSingletonInstance()
        {
            Assert.That(TrackSelectors.Sequential, Is.SameAs(TrackSelectors.Sequential));
        }

        [Test]
        public void Random_EmptyTracks_ReturnsMinusOne()
        {
            var ctx = MakeContext(0);
            Assert.That(TrackSelectors.Random.SelectNext(ctx), Is.EqualTo(-1));
        }

        [Test]
        public void Random_SingleTrack_AlwaysReturnsZero()
        {
            var ctx = MakeContext(1);
            for (var i = 0; i < 5; i++)
                Assert.That(TrackSelectors.Random.SelectNext(ctx), Is.EqualTo(0));
        }

        [Test]
        public void Random_MultipleTracksReturnsValidIndex()
        {
            var ctx = MakeContext(3);
            for (var i = 0; i < 20; i++)
            {
                var index = TrackSelectors.Random.SelectNext(ctx);
                Assert.That(index, Is.GreaterThanOrEqualTo(0).And.LessThan(3));
            }
        }

        [Test]
        public void Random_IsSingletonInstance()
        {
            Assert.That(TrackSelectors.Random, Is.SameAs(TrackSelectors.Random));
        }
    }
}
