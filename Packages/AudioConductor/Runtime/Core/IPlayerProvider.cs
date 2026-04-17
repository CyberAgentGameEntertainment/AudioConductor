// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core
{
    internal interface IPlayerProvider
    {
        void Prewarm(int count);
        AudioClipPlayer Rent();
        void Return(AudioClipPlayer player);
    }
}
