// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal interface IFadeable
    {
        void SetVolumeInternal(float volume);
        void Stop();
    }
}
