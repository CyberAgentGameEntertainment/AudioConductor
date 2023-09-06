// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Runtime.Core
{
    internal sealed class SequentialTrackSelector : ITrackSelector
    {
        private int _currentIndex = -1;
        private int _trackNum = -1;

        public void Setup(IReadOnlyList<Track> tracks)
        {
            Reset();
            _trackNum = tracks.Count;
        }

        public int NextTrackIndex()
        {
            _currentIndex++;
            if (_currentIndex >= _trackNum)
                _currentIndex = 0;

            return _currentIndex;
        }

        public void Reset()
        {
            _trackNum = -1;
            _currentIndex = -1;
        }
    }
}
