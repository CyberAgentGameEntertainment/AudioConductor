// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Core.Enums;

namespace AudioConductor.Core
{
    internal interface IFadeable
    {
        uint ActiveFadeId { get; set; }
        FadeState FadeState { get; set; }
        float VolumeFade { get; }
        void SetVolumeFade(float fade);
    }
}
