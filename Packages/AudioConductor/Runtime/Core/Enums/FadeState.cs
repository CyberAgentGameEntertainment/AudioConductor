// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core.Enums
{
    /// <summary>
    ///     Fade direction state of the player.
    /// </summary>
    public enum FadeState
    {
        None,
        FadingIn,
        FadingOut,

        // Fade-out animation completed; Conductor.Update will stop and return the player.
        FadingOutComplete
    }
}
