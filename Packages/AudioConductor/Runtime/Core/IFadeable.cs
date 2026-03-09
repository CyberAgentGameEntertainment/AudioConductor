// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core
{
    internal interface IFadeable
    {
        uint ActiveFadeId { get; set; }
        bool IsFading { get; set; }
        float VolumeFade { get; }
        void SetVolumeFade(float fade);
    }
}
