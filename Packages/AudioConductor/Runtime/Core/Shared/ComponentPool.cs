// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core.Shared
{
    /// <summary>
    ///     Base class of ComponentPool.
    /// </summary>
    internal abstract class ComponentPool<T> : ObjectPool<T> where T : Component
    {
        /// <summary>
        ///     Called before return to pool, useful for set active object(it is default behavior).
        /// </summary>
        protected override void OnBeforeRent(T instance)
        {
            instance.gameObject.SetActive(true);
        }

        /// <summary>
        ///     Called before return to pool, useful for set inactive object(it is default behavior).
        /// </summary>
        protected override void OnBeforeReturn(T instance)
        {
            instance.gameObject.SetActive(false);
        }

        /// <summary>
        ///     Called when clear or disposed, useful for destroy instance or other finalize method.
        /// </summary>
        protected override void OnClear(T instance)
        {
            if (instance == null)
                return;

            var go = instance.gameObject;
            if (go == null)
                return;
            Object.Destroy(go);
        }
    }
}
