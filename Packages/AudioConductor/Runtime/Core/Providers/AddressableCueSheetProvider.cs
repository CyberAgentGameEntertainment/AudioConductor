// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#if AUDIOCONDUCTOR_ADDRESSABLES

#nullable enable

using System.Threading.Tasks;
using AudioConductor.Core.Models;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AudioConductor.Core.Providers
{
    /// <summary>
    ///     ICueSheetProvider implementation that loads assets via Addressables with handle management.
    /// </summary>
    public class AddressableCueSheetProvider : CueSheetProviderBase<AsyncOperationHandle<CueSheetAsset>>
    {
        private static readonly IAPIWrapper DefaultWrapper = new OriginalAddressables();

        private readonly IAPIWrapper _apiWrapper;

        public AddressableCueSheetProvider() : this(DefaultWrapper)
        {
        }

        internal AddressableCueSheetProvider(IAPIWrapper apiWrapper)
        {
            _apiWrapper = apiWrapper;
        }

        /// <inheritdoc />
        protected override async Task<(CueSheetAsset asset, AsyncOperationHandle<CueSheetAsset> state)?> LoadCoreAsync(
            string key)
        {
            var handle = _apiWrapper.LoadAssetAsync<CueSheetAsset>(key);

            CueSheetAsset asset;
            try
            {
                asset = await handle.Task;
            }
            catch
            {
                _apiWrapper.Release(handle);
                return null;
            }

            if (asset == null)
            {
                _apiWrapper.Release(handle);
                return null;
            }

            return (asset, handle);
        }

        /// <inheritdoc />
        protected override void ReleaseCore(AsyncOperationHandle<CueSheetAsset> state)
        {
            _apiWrapper.Release(state);
        }

        internal interface IAPIWrapper
        {
            AsyncOperationHandle<T> LoadAssetAsync<T>(string key);
            void Release<T>(AsyncOperationHandle<T> handle);
        }

        private sealed class OriginalAddressables : IAPIWrapper
        {
            public AsyncOperationHandle<T> LoadAssetAsync<T>(string key)
            {
                return Addressables.LoadAssetAsync<T>(key);
            }

            public void Release<T>(AsyncOperationHandle<T> handle)
            {
                Addressables.Release(handle);
            }
        }
    }
}
#endif
