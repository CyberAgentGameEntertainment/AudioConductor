// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Shared;
using UnityEngine;

namespace AudioConductor.Core
{
    internal sealed class AudioClipPlayerPool : ObjectPool<AudioClipPlayer>
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
                instance.SetActive(true);
        }

        protected override void OnBeforeReturn(AudioClipPlayer instance)
        {
            instance.ResetState();

            if (_deactivateOnReturn)
                instance.SetActive(false);
        }

        protected override void OnClear(AudioClipPlayer instance)
        {
            instance.Destroy();
        }

        protected override AudioClipPlayer CreateInstance()
        {
            return AudioClipPlayer.Create(_parent);
        }
    }
}
