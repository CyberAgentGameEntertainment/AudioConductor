// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Stateless strategy for selecting the next track index from a <see cref="TrackSelectionContext" />.
    /// </summary>
    public interface ITrackSelector
    {
        /// <summary>
        ///     Selects the next track index and updates <paramref name="context" />.
        /// </summary>
        /// <param name="context">The per-cue playback context. <see cref="TrackSelectionContext.CurrentIndex" /> is updated.</param>
        /// <returns>The selected track index, or -1 if no track is available.</returns>
        int SelectNext(TrackSelectionContext context);
    }
}
