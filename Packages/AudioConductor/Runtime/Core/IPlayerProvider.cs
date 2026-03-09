// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core
{
    internal interface IPlayerProvider
    {
        void Prewarm(int count);
        IInternalPlayer Rent();
        void Return(IInternalPlayer player);
    }
}
