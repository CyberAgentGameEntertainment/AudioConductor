// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Options for customizing managed playback behavior.
    /// </summary>
    public struct PlayOptions
    {
        /// <summary>
        ///     When true, overrides the track's loop setting and forces looping.
        /// </summary>
        public bool IsLoop;

        /// <summary>
        ///     Specifies the track to play by index.
        ///     Mutually exclusive with <see cref="TrackName" />.
        /// </summary>
        public int? TrackIndex;

        /// <summary>
        ///     Specifies the track to play by name.
        ///     Mutually exclusive with <see cref="TrackIndex" />.
        /// </summary>
        public string? TrackName;

        /// <summary>
        ///     Fade-in duration in seconds. When null, no fade-in is applied.
        /// </summary>
        public float? FadeTime;

        /// <summary>
        ///     Custom fader curve. When null, <see cref="Faders.Linear" /> is used.
        /// </summary>
        public IFader? Fader;

        /// <summary>
        ///     Custom track selector. When null, the cue's configured selector is used.
        ///     Mutually exclusive with <see cref="TrackIndex" /> and <see cref="TrackName" />.
        /// </summary>
        public ITrackSelector? Selector;

        /// <summary>
        ///     Called once when playback stops for any reason (explicit stop, fade-out completion, eviction, or natural end).
        /// </summary>
        public Action? OnStop;

        /// <summary>
        ///     Called once when non-looping playback reaches its natural end.
        ///     Not invoked on explicit stop, fade-out completion, or eviction.
        /// </summary>
        public Action? OnEnd;
    }
}
