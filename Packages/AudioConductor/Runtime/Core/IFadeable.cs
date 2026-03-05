// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal interface IFadeable
    {
        float VolumeFade { get; }
        void SetVolumeFade(float fade);
        void Stop();
    }
}
