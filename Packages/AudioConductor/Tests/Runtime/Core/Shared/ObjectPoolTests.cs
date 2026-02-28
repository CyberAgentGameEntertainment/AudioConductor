// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Shared;
using NUnit.Framework;

namespace Tests.Runtime.Core.Shared
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

        private sealed class IntPool : ObjectPool<int[]>
        {
            protected override int[] CreateInstance()
            {
                return new int[1];
            }
        }
    }
}
