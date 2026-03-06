// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#if AUDIOCONDUCTOR_ADDRESSABLES
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Runtime.Core.Models;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AudioConductor.Runtime.Core.Providers
{
    /// <summary>
    ///     ICueSheetProvider implementation that loads assets via Addressables with handle management.
    /// </summary>
    public class AddressableCueSheetProvider : ICueSheetProvider
    {
        private readonly Dictionary<CueSheetAsset, Stack<AsyncOperationHandle<CueSheetAsset>>> _handles = new();

        /// <inheritdoc />
        public virtual CueSheetAsset Load(string key)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public virtual async Task<CueSheetAsset> LoadAsync(string key)
        {
            var handle = Addressables.LoadAssetAsync<CueSheetAsset>(key);

            CueSheetAsset asset;
            try
            {
                asset = await handle.Task;
            }
            catch
            {
                Addressables.Release(handle);
                return null;
            }

            if (asset == null)
            {
                Addressables.Release(handle);
                return null;
            }

            if (!_handles.TryGetValue(asset, out var stack))
            {
                stack = new Stack<AsyncOperationHandle<CueSheetAsset>>();
                _handles[asset] = stack;
            }

            stack.Push(handle);
            return asset;
        }

        /// <inheritdoc />
        public virtual void Release(CueSheetAsset asset)
        {
            if (asset == null)
                return;

            if (!_handles.TryGetValue(asset, out var stack) || stack.Count == 0)
                return;

            var handle = stack.Pop();
            Addressables.Release(handle);

            if (stack.Count == 0)
                _handles.Remove(asset);
        }
    }
}
#endif
