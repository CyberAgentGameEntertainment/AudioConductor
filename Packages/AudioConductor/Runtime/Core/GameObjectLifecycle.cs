// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core
{
    internal sealed class GameObjectLifecycle : IAudioPlayerLifecycle
    {
        private readonly GameObject _root;

        internal GameObjectLifecycle(GameObject root)
        {
            _root = root;
        }

        public void SetActive(bool active)
        {
            _root.SetActive(active);
        }

        public void Destroy()
        {
            if (_root == null)
                return;
            Object.Destroy(_root);
        }
    }
}
