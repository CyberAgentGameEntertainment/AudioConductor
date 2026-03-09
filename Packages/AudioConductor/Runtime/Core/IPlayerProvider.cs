// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal interface IPlayerProvider
    {
        void Prewarm(int count);
        IInternalPlayer Rent();
        void Return(IInternalPlayer player);
    }
}
