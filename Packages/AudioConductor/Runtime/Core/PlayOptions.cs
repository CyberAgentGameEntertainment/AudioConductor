// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
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
        public string TrackName;
    }
}
