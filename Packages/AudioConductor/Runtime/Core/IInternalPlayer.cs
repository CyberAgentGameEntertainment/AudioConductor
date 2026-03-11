// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core
{
    internal interface IInternalPlayer : IAudioClipPlayer, IFadeable
    {
        void SetMasterVolume(float volume);
        void SetCategoryVolume(float volume);
        void ManualUpdate(float deltaTime);
        void ResetState();
    }
}
