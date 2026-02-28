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
        private static IReadOnlyList<Track> CreateTracks(int count)
        {
            var tracks = new List<Track>();
            for (var i = 0; i < count; i++)
                tracks.Add(new Track { name = $"Track{i}" });
            return tracks;
        }

        [Test]
        public void NextTrackIndex_AfterReset_ReturnsMinusOne()
        {
            var selector = new RandomTrackSelector();
            selector.Setup(CreateTracks(3));
            selector.Reset();

            var index = selector.NextTrackIndex();

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void NextTrackIndex_AfterSetup_ReturnsValidIndex()
        {
            var selector = new RandomTrackSelector();
            var tracks = CreateTracks(3);
            selector.Setup(tracks);

            var index = selector.NextTrackIndex();

            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            Assert.That(index, Is.LessThan(tracks.Count));
        }

        [Test]
        public void NextTrackIndex_WithSingleTrack_ReturnsZero()
        {
            var selector = new RandomTrackSelector();
            selector.Setup(CreateTracks(1));

            var index = selector.NextTrackIndex();

            Assert.That(index, Is.EqualTo(0));
        }
    }
}
