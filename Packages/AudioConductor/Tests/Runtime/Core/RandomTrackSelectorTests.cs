// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    public class RandomTrackSelectorTests
    {
        private static TrackSelectionContext CreateContext(int count)
        {
            var tracks = new List<Track>();
            for (var i = 0; i < count; i++)
                tracks.Add(new Track { name = $"Track{i}" });
            return new TrackSelectionContext(tracks);
        }

        private static TrackSelectionContext CreateWeightedContext(int[] weights)
        {
            var tracks = new List<Track>();
            for (var i = 0; i < weights.Length; i++)
                tracks.Add(new Track { name = $"Track{i}", randomWeight = weights[i] });
            return new TrackSelectionContext(tracks);
        }

        [Test]
        public void SelectNext_WithEmptyTracks_ReturnsMinusOne()
        {
            var ctx = CreateContext(0);

            var index = TrackSelectors.Random.SelectNext(ctx);

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void SelectNext_ReturnsValidIndex()
        {
            var ctx = CreateContext(3);

            var index = TrackSelectors.Random.SelectNext(ctx);

            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            Assert.That(index, Is.LessThan(3));
        }

        [Test]
        public void SelectNext_WithSingleTrack_ReturnsZero()
        {
            var ctx = CreateContext(1);

            var index = TrackSelectors.Random.SelectNext(ctx);

            Assert.That(index, Is.EqualTo(0));
        }

        [Test]
        public void SelectNext_WithWeights_ReturnsValidIndex()
        {
            var ctx = CreateWeightedContext(new[] { 1, 2, 7 });

            var index = TrackSelectors.Random.SelectNext(ctx);

            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            Assert.That(index, Is.LessThan(3));
        }

        [Test]
        public void SelectNext_WithZeroWeightTrack_NeverSelectsZeroWeightTrack()
        {
            // Track0: weight=0, Track1: weight=1 → Track0 must never be selected in weighted path
            var ctx = CreateWeightedContext(new[] { 0, 1 });

            for (var i = 0; i < 50; i++)
            {
                ctx.CurrentIndex = -1;
                var index = TrackSelectors.Random.SelectNext(ctx);
                Assert.That(index, Is.EqualTo(1));
            }
        }

        [Test]
        public void SelectNext_WithWeights_UpdatesContextCurrentIndex()
        {
            var ctx = CreateWeightedContext(new[] { 1, 9 });

            TrackSelectors.Random.SelectNext(ctx);

            Assert.That(ctx.CurrentIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(ctx.CurrentIndex, Is.LessThan(2));
        }

        [Test]
        public void SelectNext_WithWeights_IncreasesPlayCount()
        {
            var ctx = CreateWeightedContext(new[] { 1, 1 });

            TrackSelectors.Random.SelectNext(ctx);

            Assert.That(ctx.PlayCount, Is.EqualTo(1));
        }
    }
}
