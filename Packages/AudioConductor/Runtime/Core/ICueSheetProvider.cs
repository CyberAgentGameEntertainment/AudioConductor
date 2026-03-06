// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Threading.Tasks;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Delegates CueSheet loading and releasing to the asset management system.
    /// </summary>
    public interface ICueSheetProvider
    {
        /// <summary>
        ///     Synchronously loads a <see cref="CueSheetAsset" /> by key.
        /// </summary>
        CueSheetAsset? Load(string key);

        /// <summary>
        ///     Asynchronously loads a <see cref="CueSheetAsset" /> by key.
        /// </summary>
        Task<CueSheetAsset?> LoadAsync(string key);

        /// <summary>
        ///     Releases a previously loaded <see cref="CueSheetAsset" />.
        /// </summary>
        void Release(CueSheetAsset? asset);
    }
}
