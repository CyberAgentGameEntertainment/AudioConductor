// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

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
        private sealed class TestComponentPool : ComponentPool<Transform>
        {
            protected override Transform CreateInstance()
            {
                return new GameObject("PooledObject").transform;
            }
        }

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
    }
}
