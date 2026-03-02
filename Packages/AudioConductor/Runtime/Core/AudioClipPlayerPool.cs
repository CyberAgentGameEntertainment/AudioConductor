// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Shared;

namespace AudioConductor.Runtime.Core
{
    internal sealed class AudioClipPlayerPool : ComponentPool<AudioClipPlayer>
    {
        protected override AudioClipPlayer CreateInstance()
        {
            return AudioClipPlayer.Create(GlobalGameObject.Instance.transform);
        }
    }
}
