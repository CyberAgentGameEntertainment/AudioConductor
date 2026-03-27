// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core
{
    internal interface IAudioPlayerLifecycle
    {
        void SetActive(bool active);
        void Destroy();
    }
}
