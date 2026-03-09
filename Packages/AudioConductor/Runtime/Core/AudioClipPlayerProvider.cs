// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;

namespace AudioConductor.Core
{
    internal sealed class AudioClipPlayerProvider : IPlayerProvider
    {
        private readonly AudioClipPlayerPool _pool;

        internal AudioClipPlayerProvider(Transform parent, bool deactivateOnReturn)
        {
            _pool = new AudioClipPlayerPool(parent, deactivateOnReturn);
        }

        public void Prewarm(int count)
        {
            _pool.Prewarm(count);
        }

        public IInternalPlayer Rent()
        {
            return _pool.Rent();
        }

        public void Return(IInternalPlayer player)
        {
            if (player == null)
                return;

            var clipPlayer = (AudioClipPlayer)player;
            clipPlayer.ResetState();
            _pool.Return(clipPlayer);
        }
    }
}
