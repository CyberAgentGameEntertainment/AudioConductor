// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Threading.Tasks;
using AudioConductor.Core.Models;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Delegates CueSheet loading and releasing to the asset management system.
    /// </summary>
    public interface ICueSheetProvider
    {
        /// <summary>
        ///     Synchronously loads a <see cref="CueSheetAsset" /> by key.
        /// </summary>
        CueSheetLoadInfo? Load(string key);

        /// <summary>
        ///     Asynchronously loads a <see cref="CueSheetAsset" /> by key.
        /// </summary>
        Task<CueSheetLoadInfo?> LoadAsync(string key);

        /// <summary>
        ///     Releases a previously loaded CueSheet identified by its load ID.
        /// </summary>
        void Release(uint loadId);
    }
}
