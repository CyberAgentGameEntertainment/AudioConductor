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
        /// <inheritdoc />
        protected override async Task<(CueSheetAsset asset, AsyncOperationHandle<CueSheetAsset> state)?> LoadCoreAsync(
            string key)
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

            return (asset, handle);
        }

        /// <inheritdoc />
        protected override void ReleaseCore(AsyncOperationHandle<CueSheetAsset> state)
        {
            Addressables.Release(state);
        }
    }
}
#endif
