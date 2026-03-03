// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class AudioClipPlayerPool : ComponentPool<AudioClipPlayer>
    {
        private readonly Transform _parent;

        internal AudioClipPlayerPool(Transform parent)
        {
            _parent = parent;
        }

        protected override AudioClipPlayer CreateInstance()
        {
            return AudioClipPlayer.Create(_parent);
        }
    }
}
