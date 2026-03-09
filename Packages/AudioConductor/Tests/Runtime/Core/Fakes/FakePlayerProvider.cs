// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Core.Tests.Fakes
{
    internal sealed class FakePlayerProvider : IPlayerProvider
    {
        private readonly Queue<FakePlayer> _pool = new();
        public List<FakePlayer> Created { get; } = new();

        public void Prewarm(int count)
        {
        }

        public IInternalPlayer Rent()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            var player = new FakePlayer();
            Created.Add(player);
            return player;
        }

        public void Return(IInternalPlayer player)
        {
            var fake = (FakePlayer)player;
            fake.ResetState();
            _pool.Enqueue(fake);
        }
    }
}
