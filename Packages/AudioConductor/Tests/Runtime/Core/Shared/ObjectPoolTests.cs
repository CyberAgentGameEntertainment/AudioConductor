// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using NUnit.Framework;

namespace AudioConductor.Core.Shared.Tests
{
    public class ObjectPoolTests
    {
        [Test]
        public void RentAndReturn_NormalCycle_DoesNotThrow()
        {
            using var pool = new IntPool();
            var instance = pool.Rent();

            Assert.DoesNotThrow(() => pool.Return(instance));
        }

        [Test]
        public void Return_SameInstanceTwice_ThrowsInvalidOperationException()
        {
            using var pool = new IntPool();
            var instance = pool.Rent();
            pool.Return(instance);

            Assert.Throws<InvalidOperationException>(() => pool.Return(instance));
        }

        [Test]
        public void Return_InstanceNotRented_ThrowsInvalidOperationException()
        {
            using var pool = new IntPool();
            var notRented = new int[1];

            Assert.Throws<InvalidOperationException>(() => pool.Return(notRented));
        }

        [Test]
        public void Rent_AfterDispose_ThrowsObjectDisposedException()
        {
            var pool = new IntPool();
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Rent());
        }

        [Test]
        public void Return_AfterDispose_ThrowsObjectDisposedException()
        {
            var pool = new IntPool();
            var instance = pool.Rent();
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Return(instance));
        }

        [Test]
        public void Prewarm_WithCount3_PoolCountIs3()
        {
            using var pool = new IntPool();

            pool.Prewarm(3);

            Assert.That(pool.Count, Is.EqualTo(3));
        }

        [Test]
        public void Prewarm_WithCount0_PoolCountIs0()
        {
            using var pool = new IntPool();

            pool.Prewarm(0);

            Assert.That(pool.Count, Is.EqualTo(0));
        }

        [Test]
        public void Prewarm_AfterRent_PoolCountIsPrewarmedCountMinusRented()
        {
            using var pool = new IntPool();
            pool.Prewarm(3);

            pool.Rent();

            Assert.That(pool.Count, Is.EqualTo(2));
        }

        private sealed class IntPool : ObjectPool<int[]>
        {
            protected override int[] CreateInstance()
            {
                return new int[1];
            }
        }
    }
}
