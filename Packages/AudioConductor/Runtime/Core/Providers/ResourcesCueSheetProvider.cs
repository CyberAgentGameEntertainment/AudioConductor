// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Providers
{
    /// <summary>
    ///     ICueSheetProvider implementation that loads assets from Resources with reference counting.
    /// </summary>
    public class ResourcesCueSheetProvider : CueSheetProviderBase<string>
    {
        private static readonly IAPIWrapper DefaultWrapper = new OriginalResources();

        private readonly IAPIWrapper _apiWrapper;
        private readonly Dictionary<string, (CueSheetAsset asset, int count)> _cache = new();

        public ResourcesCueSheetProvider() : this(DefaultWrapper)
        {
        }

        internal ResourcesCueSheetProvider(IAPIWrapper apiWrapper)
        {
            _apiWrapper = apiWrapper;
        }

        /// <inheritdoc />
        protected override (CueSheetAsset asset, string state)? LoadCore(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                _cache[key] = (entry.asset, entry.count + 1);
                return (entry.asset, key);
            }

            var asset = _apiWrapper.Load<CueSheetAsset>(key);
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
            var request = _apiWrapper.LoadAsync<CueSheetAsset>(key);
            request.completed += req =>
            {
                var asset = req.asset as CueSheetAsset;
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
                _apiWrapper.UnloadAsset(entry.asset);
            }
            else
            {
                _cache[key] = (entry.asset, newCount);
            }
        }

        internal interface IResourceRequest
        {
            Object? asset { get; }
            event Action<IResourceRequest> completed;
        }

        internal interface IAPIWrapper
        {
            IResourceRequest LoadAsync<T>(string key) where T : Object;
            T? Load<T>(string key) where T : Object;
            void UnloadAsset(Object asset);
        }

        private sealed class OriginalResources : IAPIWrapper
        {
            public IResourceRequest LoadAsync<T>(string key) where T : Object
            {
                return new ResourceRequestAdapter(Resources.LoadAsync<T>(key));
            }

            public T? Load<T>(string key) where T : Object
            {
                return Resources.Load<T>(key);
            }

            public void UnloadAsset(Object asset)
            {
                Resources.UnloadAsset(asset);
            }

            private sealed class ResourceRequestAdapter : IResourceRequest
            {
                private readonly ResourceRequest _request;

                internal ResourceRequestAdapter(ResourceRequest request)
                {
                    _request = request;
                    _request.completed += _ => Completed?.Invoke(this);
                }

                public Object? asset => _request.asset;

                public event Action<IResourceRequest> completed
                {
                    add
                    {
                        if (_request.isDone)
                            value.Invoke(this);
                        else
                            Completed += value;
                    }
                    remove => Completed -= value;
                }

                private event Action<IResourceRequest>? Completed;
            }
        }
    }
}
