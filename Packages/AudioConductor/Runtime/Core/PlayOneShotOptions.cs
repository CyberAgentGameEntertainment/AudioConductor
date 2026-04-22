// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Options for fire-and-forget OneShot playback.
    /// </summary>
    public struct PlayOneShotOptions
    {
        /// <summary>
        ///     Called once when playback stops for any reason.
        /// </summary>
        public Action? OnStop;

        /// <summary>
        ///     Called once when non-looping playback reaches its natural end.
        ///     Not invoked on explicit stop or eviction.
        /// </summary>
        public Action? OnEnd;
    }
}
