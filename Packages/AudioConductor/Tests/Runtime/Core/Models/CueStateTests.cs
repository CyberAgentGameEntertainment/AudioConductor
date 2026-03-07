// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Runtime.Core.Models
{
    public class CueStateTests
    {
        [Test]
        public void NextTrack_WithEmptyTrackList_ReturnsNull()
        {
            var cue = new Cue { playType = CuePlayType.Sequential, trackList = new List<Track>() };
            var cueState = new CueState(1, cue);

            var track = cueState.NextTrack();

            Assert.That(track, Is.Null);
        }

        [Test]
        public void NextTrack_WithNonEmptyTrackList_ReturnsTrack()
        {
            var expectedTrack = new Track { name = "Track0" };
            var cue = new Cue
            {
                playType = CuePlayType.Sequential,
                trackList = new List<Track> { expectedTrack }
            };
            var cueState = new CueState(1, cue);

            var track = cueState.NextTrack();

            Assert.That(track, Is.EqualTo(expectedTrack));
        }

        [Test]
        public void NextTrack_RandomWithEmptyTrackList_ReturnsNull()
        {
            var cue = new Cue { playType = CuePlayType.Random, trackList = new List<Track>() };
            var cueState = new CueState(1, cue);

            var track = cueState.NextTrack();

            Assert.That(track, Is.Null);
        }
    }
}
