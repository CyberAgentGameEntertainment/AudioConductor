// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Built-in <see cref="ITrackSelector" /> singletons.
    /// </summary>
    public static class TrackSelectors
    {
        /// <summary>Selects tracks in order, wrapping around to the first track after the last.</summary>
        public static readonly ITrackSelector Sequential = new SequentialTrackSelector();

        /// <summary>Selects a random track, weighted by <see cref="Models.Track.randomWeight" />.</summary>
        public static readonly ITrackSelector Random = new RandomTrackSelector();
    }
}
