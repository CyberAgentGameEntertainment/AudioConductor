// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Core
{
    internal sealed class NullLifecycle : IAudioPlayerLifecycle
    {
        internal static readonly NullLifecycle Instance = new();

        public void SetActive(bool active)
        {
        }

        public void Destroy()
        {
        }
    }
}
