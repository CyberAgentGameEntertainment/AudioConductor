// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace Tests.Runtime.Core
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
    }
}
