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

        [Test]
        public void Shrink_WithHalfRatio_ReducesPool()
        {
            using var pool = new IntPool();
            pool.Prewarm(10);

            pool.Shrink(0.5f, 0);

            Assert.That(pool.Count, Is.EqualTo(5));
        }

        [Test]
        public void Shrink_WithZeroRatio_ClearsPool()
        {
            using var pool = new IntPool();
            pool.Prewarm(5);

            pool.Shrink(0f, 0);

            Assert.That(pool.Count, Is.EqualTo(0));
        }

        [Test]
        public void Shrink_WithMinSize_DoesNotShrinkBelowMinSize()
        {
            using var pool = new IntPool();
            pool.Prewarm(10);

            pool.Shrink(0f, 3);

            Assert.That(pool.Count, Is.EqualTo(3));
        }

        [Test]
        public void Shrink_WithOddCountAndHalfRatio_TruncatesResult()
        {
            using var pool = new IntPool();
            pool.Prewarm(3);

            pool.Shrink(0.5f, 0);

            Assert.That(pool.Count, Is.EqualTo(1));
        }

        [Test]
        public void Shrink_NullPool_DoesNotThrow()
        {
            using var pool = new IntPool();

            Assert.DoesNotThrow(() => pool.Shrink(0.5f, 0));
        }

        [Test]
        public void Shrink_CallsOnClearForRemovedInstances()
        {
            using var pool = new TrackingPool();
            pool.Prewarm(4);

            pool.Shrink(0.5f, 0);

            Assert.That(pool.OnClearCount, Is.EqualTo(2));
        }

        [Test]
        public void Shrink_WithCallOnBeforeRent_CallsOnBeforeRent()
        {
            using var pool = new TrackingPool();
            pool.Prewarm(4);

            pool.Shrink(0f, 0, true);

            Assert.That(pool.OnBeforeRentCount, Is.EqualTo(4));
        }

        [Test]
        public void Clear_WithItems_CallsOnClear()
        {
            using var pool = new TrackingPool();
            pool.Prewarm(3);

            pool.Clear();

            Assert.That(pool.OnClearCount, Is.EqualTo(3));
        }

        [Test]
        public void Clear_WithCallOnBeforeRent_CallsOnBeforeRent()
        {
            using var pool = new TrackingPool();
            pool.Prewarm(3);

            pool.Clear(true);

            Assert.That(pool.OnBeforeRentCount, Is.EqualTo(3));
        }

        [Test]
        public void Clear_NullPool_DoesNotThrow()
        {
            using var pool = new IntPool();

            Assert.DoesNotThrow(() => pool.Clear());
        }

        [Test]
        public void Return_NullInstance_ThrowsArgumentNullException()
        {
            using var pool = new NullableIntPool();

            Assert.Throws<ArgumentNullException>(() => pool.Return(null));
        }

        [Test]
        public void Prewarm_AfterDispose_ThrowsObjectDisposedException()
        {
            var pool = new IntPool();
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Prewarm(1));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var pool = new IntPool();
            pool.Dispose();

            Assert.DoesNotThrow(() => pool.Dispose());
        }

        private sealed class IntPool : ObjectPool<int[]>
        {
            protected override int[] CreateInstance()
            {
                return new int[1];
            }
        }

        private sealed class NullableIntPool : ObjectPool<int[]?>
        {
            protected override int[]? CreateInstance()
            {
                return null;
            }
        }

        private sealed class TrackingPool : ObjectPool<int[]>
        {
            public int OnBeforeRentCount { get; private set; }
            public int OnBeforeReturnCount { get; private set; }
            public int OnClearCount { get; private set; }

            protected override int[] CreateInstance()
            {
                return new int[1];
            }

            protected override void OnBeforeRent(int[] instance)
            {
                OnBeforeRentCount++;
            }

            protected override void OnBeforeReturn(int[] instance)
            {
                OnBeforeReturnCount++;
            }

            protected override void OnClear(int[] instance)
            {
                OnClearCount++;
            }
        }
    }
}
