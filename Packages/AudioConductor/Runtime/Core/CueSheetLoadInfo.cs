// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Contains the result of a provider load operation: the loaded asset and its unique load identifier.
    /// </summary>
    public readonly struct CueSheetLoadInfo
    {
        /// <summary>
        ///     The loaded CueSheet asset.
        /// </summary>
        public readonly CueSheetAsset Asset;

        /// <summary>
        ///     A unique identifier assigned by the provider for this load operation.
        /// </summary>
        public readonly uint LoadId;

        public CueSheetLoadInfo(CueSheetAsset asset, uint loadId)
        {
            Asset = asset;
            LoadId = loadId;
        }
    }
}
