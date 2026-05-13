// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Models
{
    internal sealed class CueSheetListItem
    {
        public CueSheetListItem(CueSheetAsset asset, string displayName, int cueCount)
        {
            Asset = asset;
            DisplayName = displayName;
            CueCount = cueCount;
        }

        public CueSheetAsset Asset { get; }
        public string DisplayName { get; }
        public int CueCount { get; }
    }
}
