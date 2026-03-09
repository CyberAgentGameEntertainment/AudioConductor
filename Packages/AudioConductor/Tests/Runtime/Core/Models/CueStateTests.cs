// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Core.Tests.Models
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

        [Test]
        public void GetTrackByName_WithMatchingName_ReturnsTrack()
        {
            var expectedTrack = new Track { name = "TrackA" };
            var cue = new Cue
            {
                playType = CuePlayType.Sequential,
                trackList = new List<Track> { new() { name = "TrackB" }, expectedTrack }
            };
            var cueState = new CueState(1, cue);

            var track = cueState.GetTrack("TrackA");

            Assert.That(track, Is.EqualTo(expectedTrack));
        }

        [Test]
        public void GetTrackByName_WithNonMatchingName_ReturnsNull()
        {
            var cue = new Cue
            {
                playType = CuePlayType.Sequential,
                trackList = new List<Track> { new() { name = "TrackA" } }
            };
            var cueState = new CueState(1, cue);

            var track = cueState.GetTrack("NonExistent");

            Assert.That(track, Is.Null);
        }

        [Test]
        public void GetTrackByName_CalledMultipleTimes_ReturnsSameResult()
        {
            var expectedTrack = new Track { name = "TrackA" };
            var cue = new Cue
            {
                playType = CuePlayType.Sequential,
                trackList = new List<Track> { expectedTrack, new() { name = "TrackB" } }
            };
            var cueState = new CueState(1, cue);

            var track1 = cueState.GetTrack("TrackA");
            var track2 = cueState.GetTrack("TrackA");

            Assert.That(track1, Is.EqualTo(expectedTrack));
            Assert.That(track2, Is.EqualTo(expectedTrack));
        }
    }
}
