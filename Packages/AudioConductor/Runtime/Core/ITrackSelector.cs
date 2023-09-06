// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Runtime.Core
{
    internal interface ITrackSelector
    {
        void Setup(IReadOnlyList<Track> trackList);
        int NextTrackIndex();
        void Reset();
    }
}
