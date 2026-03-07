// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class AudioClipPlayerPool : ComponentPool<AudioClipPlayer>
    {
        private readonly bool _deactivateOnReturn;
        private readonly Transform _parent;

        internal AudioClipPlayerPool(Transform parent, bool deactivateOnReturn)
        {
            _parent = parent;
            _deactivateOnReturn = deactivateOnReturn;
        }

        protected override void OnBeforeRent(AudioClipPlayer instance)
        {
            if (_deactivateOnReturn)
                instance.gameObject.SetActive(true);
        }

        protected override void OnBeforeReturn(AudioClipPlayer instance)
        {
            if (_deactivateOnReturn)
                instance.gameObject.SetActive(false);
        }

        protected override AudioClipPlayer CreateInstance()
        {
            return AudioClipPlayer.Create(_parent);
        }
    }
}
