// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class SpyPlayerProvider : IPlayerProvider
    {
        private readonly Queue<SpyPlayer> _pool = new();
        public List<SpyPlayer> Created { get; } = new();

        public void Prewarm(int count)
        {
        }

        public IInternalPlayer Rent()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            var player = new SpyPlayer();
            Created.Add(player);
            return player;
        }

        public void Return(IInternalPlayer player)
        {
            var spy = (SpyPlayer)player;
            spy.ResetState();
            _pool.Enqueue(spy);
        }
    }
}
