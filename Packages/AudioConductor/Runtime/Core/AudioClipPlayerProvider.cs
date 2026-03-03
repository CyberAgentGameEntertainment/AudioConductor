// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class AudioClipPlayerProvider
    {
        private readonly AudioClipPlayerPool _pool;

        internal AudioClipPlayerProvider(Transform parent)
        {
            _pool = new AudioClipPlayerPool(parent);
        }

        public AudioClipPlayer Rent()
        {
            return _pool.Rent();
        }

        public void Return(AudioClipPlayer player)
        {
            if (player == null)
                return;

            player.ResetState();
            _pool.Return(player);
        }
    }
}
