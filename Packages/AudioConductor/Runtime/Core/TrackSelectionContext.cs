// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Carries per-cue state that a stateless <see cref="ITrackSelector" /> uses to select the next track.
    /// </summary>
    public sealed class TrackSelectionContext
    {
        public TrackSelectionContext(IReadOnlyList<Track> tracks)
        {
            Tracks = tracks;
            CurrentIndex = -1;
            PlayCount = 0;
        }

        /// <summary>The track list of the cue.</summary>
        public IReadOnlyList<Track> Tracks { get; }

        /// <summary>
        ///     Index of the most recently selected track. -1 means no track has been selected yet.
        ///     This property should only be written by <see cref="ITrackSelector.SelectNext" /> implementations.
        /// </summary>
        public int CurrentIndex { get; set; }

        /// <summary>
        ///     Number of times a track has been selected from this context.
        ///     This property should only be written by <see cref="ITrackSelector.SelectNext" /> implementations.
        /// </summary>
        public int PlayCount { get; set; }
    }
}
