// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Runtime.Core
{
    public class SequentialTrackSelectorTests
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

            var index = TrackSelectors.Sequential.SelectNext(ctx);

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void SelectNext_ReturnsSequentialIndices()
        {
            var ctx = CreateContext(3);

            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(0));
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(1));
            Assert.That(TrackSelectors.Sequential.SelectNext(ctx), Is.EqualTo(2));
        }

        [Test]
        public void SelectNext_AfterLastTrack_WrapsToFirst()
        {
            var ctx = CreateContext(2);

            TrackSelectors.Sequential.SelectNext(ctx); // 0
            TrackSelectors.Sequential.SelectNext(ctx); // 1
            var index = TrackSelectors.Sequential.SelectNext(ctx); // wraps to 0

            Assert.That(index, Is.EqualTo(0));
        }
    }
}
