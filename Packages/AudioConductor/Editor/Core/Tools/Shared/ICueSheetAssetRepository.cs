// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal interface ICueSheetAssetRepository
    {
        event Action? Changed;

        /// <summary>
        ///     Returns all <see cref="CueSheetAsset" /> assets found in the Assets folder.
        /// </summary>
        CueSheetAsset[] GetAll();
    }
}
