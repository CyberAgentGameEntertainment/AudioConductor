// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class AudioClipPlayerPool : ComponentPool<AudioClipPlayer>
    {
        private AudioClipPlayer _prefab;

        protected override AudioClipPlayer CreateInstance()
        {
            if (_prefab == null)
                _prefab = Resources.Load<AudioClipPlayer>("AudioClipPlayer");

            return Object.Instantiate(_prefab, GlobalGameObject.Instance.transform);
        }
    }
}
