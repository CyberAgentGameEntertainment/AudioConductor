using System;
using System.Collections.Generic;

namespace Sge.Sound.Utility
{
    /// <summary>
    /// Base class of ObjectPool.
    /// </summary>
    public abstract class ObjectPool<T> : IDisposable
    {
        bool _isDisposed = false;
        Stack<T> _pool;

        /// <summary>
        /// Initial capacity of pool.
        /// Default capacity is 4(Same as Stack).
        /// </summary>
        protected virtual int InitialCapacity => 4;

        /// <summary>
        /// Limit of instance count.
        /// </summary>
        protected int MaxPoolCount => int.MaxValue;

        /// <summary>
        /// Create instance when needed.
        /// </summary>
        protected abstract T CreateInstance();

        /// <summary>
        /// Called before return to pool.
        /// </summary>
        protected virtual void OnBeforeRent(T instance)
        {

        }

        /// <summary>
        /// Called before return to pool.
        /// </summary>
        protected virtual void OnBeforeReturn(T instance)
        {

        }

        /// <summary>
        /// Called when clear or disposed.
        /// </summary>
        protected virtual void OnClear(T instance)
        {

        }

        /// <summary>
        /// Current pooled object count.
        /// </summary>
        public int Count
        {
            get
            {
                if (_pool == null) return 0;
                return _pool.Count;
            }
        }

        /// <summary>
        /// Get instance from pool.
        /// </summary>
        public T Rent()
        {
            if (_isDisposed) throw new ObjectDisposedException("ObjectPool was already disposed.");
            if (_pool == null) _pool = new Stack<T>(InitialCapacity);

            var instance = (_pool.Count > 0)
                ? _pool.Pop()
                : CreateInstance();

            OnBeforeRent(instance);
            return instance;
        }

        /// <summary>
        /// Return instance to pool.
        /// </summary>
        public void Return(T instance)
        {
            if (_isDisposed) throw new ObjectDisposedException("ObjectPool was already disposed.");
            if (instance == null) throw new ArgumentNullException("instance");

            if (_pool == null) _pool = new Stack<T>(InitialCapacity);

            if ((_pool.Count + 1) == MaxPoolCount)
            {
                throw new InvalidOperationException("Reached Max PoolSize");
            }

            OnBeforeReturn(instance);
            _pool.Push(instance);
        }

        /// <summary>
        /// Clear pool.
        /// </summary>
        public void Clear(bool callOnBeforeRent = false)
        {
            if (_pool == null) return;
            while (_pool.Count != 0)
            {
                var instance = _pool.Pop();
                if (callOnBeforeRent)
                {
                    OnBeforeRent(instance);
                }

                OnClear(instance);
            }
        }

        /// <summary>
        /// Trim pool instances.
        /// </summary>
        /// <param name="instanceCountRatio">0.0f = clear all ~ 1.0f = live all.</param>
        /// <param name="minSize">Min pool count.</param>
        /// <param name="callOnBeforeRent">If true, call OnBeforeRent before OnClear.</param>
        public void Shrink(float instanceCountRatio, int minSize, bool callOnBeforeRent = false)
        {
            if (_pool == null) return;

            if (instanceCountRatio <= 0) instanceCountRatio = 0;
            if (instanceCountRatio >= 1.0f) instanceCountRatio = 1.0f;

            var size = (int) (_pool.Count * instanceCountRatio);
            size = Math.Max(minSize, size);

            while (_pool.Count > size)
            {
                var instance = _pool.Pop();
                if (callOnBeforeRent)
                {
                    OnBeforeRent(instance);
                }

                OnClear(instance);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    Clear(false);
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}