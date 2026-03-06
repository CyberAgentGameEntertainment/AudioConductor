// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Providers
{
    /// <summary>
    ///     ICueSheetProvider implementation that loads assets from Resources with reference counting.
    /// </summary>
    public class ResourcesCueSheetProvider : ICueSheetProvider
    {
        private readonly Dictionary<CueSheetAsset, int> _refCounts = new();

        /// <inheritdoc />
        public virtual CueSheetAsset Load(string key)
        {
            var asset = Resources.Load<CueSheetAsset>(key);
            if (asset != null)
                _refCounts[asset] = _refCounts.TryGetValue(asset, out var count) ? count + 1 : 1;
            return asset;
        }

        /// <inheritdoc />
        public virtual Task<CueSheetAsset> LoadAsync(string key)
        {
            var tcs = new TaskCompletionSource<CueSheetAsset>();
            var request = Resources.LoadAsync<CueSheetAsset>(key);
            request.completed += _ =>
            {
                var asset = request.asset as CueSheetAsset;
                if (asset != null)
                    _refCounts[asset] = _refCounts.TryGetValue(asset, out var count) ? count + 1 : 1;
                tcs.SetResult(asset);
            };
            return tcs.Task;
        }

        /// <inheritdoc />
        public virtual void Release(CueSheetAsset asset)
        {
            if (asset == null)
                return;

            if (!_refCounts.TryGetValue(asset, out var count))
                return;

            count--;
            if (count <= 0)
            {
                _refCounts.Remove(asset);
                Resources.UnloadAsset(asset);
            }
            else
            {
                _refCounts[asset] = count;
            }
        }
    }
}
