// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Core.Models;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Abstract base class that manages loadId allocation and state tracking for
    ///     <see cref="ICueSheetProvider" /> implementations.
    /// </summary>
    /// <typeparam name="TState">The per-load state stored alongside each load operation (e.g. an operation handle).</typeparam>
    public abstract class CueSheetProviderBase<TState> : ICueSheetProvider, IDisposable
    {
        private readonly Dictionary<uint, TState> _states = new();
        private uint _nextLoadId;

        /// <summary>
        ///     Synchronously loads a CueSheet by key.
        /// </summary>
        public CueSheetLoadInfo? Load(string key)
        {
            var result = LoadCore(key);
            if (result == null)
                return null;

            var (asset, state) = result.Value;
            var loadId = ++_nextLoadId;
            if (loadId == 0)
                loadId = ++_nextLoadId;
            _states[loadId] = state;
            return new CueSheetLoadInfo(asset, loadId);
        }

        /// <summary>
        ///     Asynchronously loads a CueSheet by key.
        /// </summary>
        public async Task<CueSheetLoadInfo?> LoadAsync(string key)
        {
            var result = await LoadCoreAsync(key);
            if (result == null)
                return null;

            var (asset, state) = result.Value;
            var loadId = ++_nextLoadId;
            if (loadId == 0)
                loadId = ++_nextLoadId;
            _states[loadId] = state;
            return new CueSheetLoadInfo(asset, loadId);
        }

        /// <summary>
        ///     Releases the load identified by <paramref name="loadId" />.
        ///     Unknown or zero loadIds are silently ignored.
        /// </summary>
        public void Release(uint loadId)
        {
            if (loadId == 0)
                return;

            if (!_states.Remove(loadId, out var state))
                return;

            ReleaseCore(state);
        }

        /// <summary>
        ///     Releases all tracked loads and clears internal state.
        /// </summary>
        public void Dispose()
        {
            foreach (var state in _states.Values)
                ReleaseCore(state);
            _states.Clear();
        }

        /// <summary>
        ///     Synchronous load implementation. Override to support synchronous loading.
        /// </summary>
        protected virtual (CueSheetAsset asset, TState state)? LoadCore(string key)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Asynchronous load implementation.
        /// </summary>
        protected abstract Task<(CueSheetAsset asset, TState state)?> LoadCoreAsync(string key);

        /// <summary>
        ///     Releases the provider-specific state for a single load.
        /// </summary>
        protected abstract void ReleaseCore(TState state);
    }
}
