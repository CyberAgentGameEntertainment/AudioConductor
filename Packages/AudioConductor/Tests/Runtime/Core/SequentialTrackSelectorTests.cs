// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace Tests.Runtime.Core
{
    public class SequentialTrackSelectorTests
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
            var selector = new SequentialTrackSelector();
            selector.Setup(CreateTracks(3));
            selector.Reset();

            var index = selector.NextTrackIndex();

            Assert.That(index, Is.EqualTo(-1));
        }

        [Test]
        public void NextTrackIndex_AfterSetup_ReturnsSequentialIndices()
        {
            var selector = new SequentialTrackSelector();
            selector.Setup(CreateTracks(3));

            Assert.That(selector.NextTrackIndex(), Is.EqualTo(0));
            Assert.That(selector.NextTrackIndex(), Is.EqualTo(1));
            Assert.That(selector.NextTrackIndex(), Is.EqualTo(2));
        }

        [Test]
        public void NextTrackIndex_AfterLastTrack_WrapsToFirst()
        {
            var selector = new SequentialTrackSelector();
            selector.Setup(CreateTracks(2));

            selector.NextTrackIndex(); // 0
            selector.NextTrackIndex(); // 1
            var index = selector.NextTrackIndex(); // wraps to 0

            Assert.That(index, Is.EqualTo(0));
        }
    }
}
