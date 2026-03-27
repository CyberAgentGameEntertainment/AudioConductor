// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class StubPlayerProvider : IPlayerProvider
    {
        private readonly Queue<AudioClipPlayer> _pool = new();
        public List<AudioClipPlayer> Created { get; } = new();

        public void Prewarm(int count)
        {
        }

        public AudioClipPlayer Rent()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            var player = new AudioClipPlayer(
                new IAudioSourceWrapper[] { new StubAudioSourceWrapper(), new StubAudioSourceWrapper() },
                new StubDspClock(),
                NullLifecycle.Instance
            );
            Created.Add(player);
            return player;
        }

        public void Return(AudioClipPlayer player)
        {
            player.ResetState();
            if (Created.Contains(player))
                _pool.Enqueue(player);
        }
    }
}
