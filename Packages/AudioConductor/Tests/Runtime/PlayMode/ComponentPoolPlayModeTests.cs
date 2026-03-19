// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections;
using AudioConductor.Core.Shared;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.PlayMode
{
    public class ComponentPoolPlayModeTests
    {
        [UnityTest]
        public IEnumerator Rent_SetsGameObjectActive()
        {
            var pool = new TestComponentPool();

            var instance = pool.Rent();

            Assert.That(instance.gameObject.activeSelf, Is.True);

            Object.Destroy(instance.gameObject);
            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Return_SetsGameObjectInactive()
        {
            var pool = new TestComponentPool();
            var instance = pool.Rent();

            pool.Return(instance);

            Assert.That(instance.gameObject.activeSelf, Is.False);

            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Clear_DestroysGameObject()
        {
            var pool = new TestComponentPool();
            var instance = pool.Rent();
            var go = instance.gameObject;
            pool.Return(instance);

            pool.Clear();
            yield return null;

            Assert.That(go == null, Is.True);
        }

        [UnityTest]
        public IEnumerator OnClear_WhenInstanceIsNull_DoesNotThrow()
        {
            var pool = new TestComponentPool();
            var instance = pool.Rent();
            pool.Return(instance);
            Object.Destroy(instance.gameObject);
            yield return null;

            Assert.DoesNotThrow(() => pool.Clear());
        }

        [UnityTest]
        public IEnumerator Dispose_DestroysAllPooledGameObjects()
        {
            var pool = new TestComponentPool();
            pool.Prewarm(2);
            var t1 = pool.Rent();
            var t2 = pool.Rent();
            var go1 = t1.gameObject;
            var go2 = t2.gameObject;
            pool.Return(t1);
            pool.Return(t2);

            pool.Dispose();
            yield return null;

            Assert.That(go1 == null, Is.True);
            Assert.That(go2 == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Prewarm_CreatesSpecifiedNumberOfInstances()
        {
            var pool = new TestComponentPool();

            pool.Prewarm(3);

            Assert.That(pool.Count, Is.EqualTo(3));

            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Prewarm_AfterDispose_ThrowsObjectDisposedException()
        {
            var pool = new TestComponentPool();
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Prewarm(1));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Rent_FromPrewarmedPool_ReusesExistingInstance()
        {
            var pool = new TestComponentPool();
            pool.Prewarm(1);

            var instance = pool.Rent();

            Assert.That(pool.Count, Is.EqualTo(0));
            Assert.That(instance, Is.Not.Null);

            Object.Destroy(instance.gameObject);
            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Rent_AfterDispose_ThrowsObjectDisposedException()
        {
            var pool = new TestComponentPool();
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Rent());
            yield return null;
        }

        [UnityTest]
        public IEnumerator Return_NullInstance_ThrowsArgumentNullException()
        {
            var pool = new TestComponentPool();

            Assert.Throws<ArgumentNullException>(() => pool.Return(null!));

            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Return_AfterDispose_ThrowsObjectDisposedException()
        {
            var pool = new TestComponentPool();
            var instance = pool.Rent();
            pool.Return(instance);
            pool.Dispose();

            Assert.Throws<ObjectDisposedException>(() => pool.Return(instance));
            yield return null;
        }

        [UnityTest]
        public IEnumerator Clear_WithCallOnBeforeRent_SetsGameObjectActive()
        {
            var pool = new TestComponentPool();
            var instance = pool.Rent();
            pool.Return(instance);

            pool.Clear(true);
            yield return null;

            Assert.That(instance == null, Is.True);
        }

        [UnityTest]
        public IEnumerator Shrink_ReducesPoolToMinSize()
        {
            var pool = new TestComponentPool();
            pool.Prewarm(4);

            pool.Shrink(0.5f, 2);
            yield return null;

            Assert.That(pool.Count, Is.EqualTo(2));

            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Shrink_WithCallOnBeforeRent_SetsGameObjectActive()
        {
            var pool = new TestComponentPool();
            pool.Prewarm(2);

            pool.Shrink(0f, 0, true);
            yield return null;

            Assert.That(pool.Count, Is.EqualTo(0));

            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Shrink_EmptyPool_DoesNotThrow()
        {
            var pool = new TestComponentPool();

            Assert.DoesNotThrow(() => pool.Shrink(0.5f, 0));

            pool.Dispose();
            yield return null;
        }

        [UnityTest]
        public IEnumerator Dispose_CalledTwice_DoesNotThrow()
        {
            var pool = new TestComponentPool();
            pool.Dispose();

            Assert.DoesNotThrow(() => pool.Dispose());
            yield return null;
        }

        [UnityTest]
        public IEnumerator Count_BeforeFirstUse_ReturnsZero()
        {
            var pool = new TestComponentPool();

            Assert.That(pool.Count, Is.EqualTo(0));

            pool.Dispose();
            yield return null;
        }

        private sealed class TestComponentPool : ComponentPool<Transform>
        {
            protected override Transform CreateInstance()
            {
                return new GameObject("PooledObject").transform;
            }
        }
    }
}
