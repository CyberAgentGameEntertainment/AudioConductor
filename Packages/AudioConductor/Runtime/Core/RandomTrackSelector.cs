// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class RandomTrackSelector : ITrackSelector
    {
        private IReadOnlyList<Track> _tracks;
        private int _trackWeightTotal;

        public void Setup(IReadOnlyList<Track> tracks)
        {
            Reset();
            _tracks = tracks;

            foreach (var track in _tracks)
                _trackWeightTotal += track.randomWeight;
        }

        public int NextTrackIndex()
        {
            if (_trackWeightTotal == 0)
                return Random.Range(0, _tracks.Count);

            var total = 0;
            var randomValue = Random.Range(0, _trackWeightTotal);
            for (var i = 0; i < _tracks.Count; ++i)
            {
                var track = _tracks[i];
                total += track.randomWeight;
                if (randomValue < total)
                    return i;
            }

            return 0;
        }

        public void Reset()
        {
            _trackWeightTotal = 0;
            _tracks = null;
        }
    }
}
