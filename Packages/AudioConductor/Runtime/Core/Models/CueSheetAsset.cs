// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    /// <summary>
    ///     Cue-sheet asset.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCueSheet",
        menuName = "Audio Conductor/" + nameof(CueSheetAsset),
        order = 2)]
    public sealed class CueSheetAsset : ScriptableObject
    {
        public CueSheet cueSheet = new();
    }
}
