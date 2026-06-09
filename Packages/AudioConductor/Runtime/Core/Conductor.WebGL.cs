// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL
namespace AudioConductor.Core
{
    public sealed partial class Conductor
    {
        internal void OnSystemPause(bool pause)
        {
            foreach (var playback in _managedPlaybacks.Values)
            {
                if (pause)
                    playback.Player.PauseBySystem();
                else
                    playback.Player.ResumeBySystem();
            }

            foreach (var playback in _oneShotPlaybacks)
            {
                if (pause)
                    playback.Player.PauseBySystem();
                else
                    playback.Player.ResumeBySystem();
            }
        }
    }
}

#endif
