// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using UnityEngine;

namespace AudioConductor.Core.Providers
{
    /// <summary>
    ///     ICueSheetProvider implementation that loads assets from Resources with reference counting.
    /// </summary>
    public class ResourcesCueSheetProvider : CueSheetProviderBase<string>
    {
        private readonly Dictionary<string, (CueSheetAsset asset, int count)> _cache = new();

        /// <inheritdoc />
        protected override (CueSheetAsset asset, string state)? LoadCore(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                _cache[key] = (entry.asset, entry.count + 1);
                return (entry.asset, key);
            }

            var asset = Resources.Load<CueSheetAsset>(key);
            if (asset == null)
                return null;

            _cache[key] = (asset, 1);
            return (asset, key);
        }

        /// <inheritdoc />
        protected override Task<(CueSheetAsset asset, string state)?> LoadCoreAsync(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                _cache[key] = (entry.asset, entry.count + 1);
                return Task.FromResult<(CueSheetAsset asset, string state)?>((entry.asset, key));
            }

            var tcs = new TaskCompletionSource<(CueSheetAsset asset, string state)?>();
            var request = Resources.LoadAsync<CueSheetAsset>(key);
            request.completed += _ =>
            {
                var asset = request.asset as CueSheetAsset;
                if (asset == null)
                {
                    tcs.SetResult(null);
                    return;
                }

                if (_cache.TryGetValue(key, out var existing))
                    _cache[key] = (existing.asset, existing.count + 1);
                else
                    _cache[key] = (asset, 1);

                tcs.SetResult((asset, key));
            };
            return tcs.Task;
        }

        /// <inheritdoc />
        protected override void ReleaseCore(string key)
        {
            if (!_cache.TryGetValue(key, out var entry))
                return;

            var newCount = entry.count - 1;
            if (newCount <= 0)
            {
                _cache.Remove(key);
                Resources.UnloadAsset(entry.asset);
            }
            else
            {
                _cache[key] = (entry.asset, newCount);
            }
        }
    }
}
