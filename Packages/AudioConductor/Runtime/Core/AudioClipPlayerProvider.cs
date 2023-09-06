// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal sealed class AudioClipPlayerProvider
    {
        private readonly AudioClipPlayerPool _pool = new();

        public AudioClipPlayer Rent() => _pool.Rent();

        public void Return(AudioClipPlayer player)
        {
            if (player == null)
                return;

            player.ResetState();
            _pool.Return(player);
        }
    }
}
