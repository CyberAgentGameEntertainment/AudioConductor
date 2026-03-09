// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal interface IInternalPlayer : IAudioClipPlayer, IFadeable
    {
        void SetMasterVolume(float volume);
        void ManualUpdate(float deltaTime);
        void ResetState();
    }
}
