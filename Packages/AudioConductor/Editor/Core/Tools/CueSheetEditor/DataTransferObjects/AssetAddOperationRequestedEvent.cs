// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects
{
    internal readonly struct AssetAddOperationRequestedEvent
    {
        public readonly int insertIndex;
        public readonly CueListItem parent;
        public readonly AudioClip asset;

        public AssetAddOperationRequestedEvent(int insertIndex, CueListItem parent, AudioClip asset)
        {
            this.insertIndex = insertIndex;
            this.parent = parent;
            this.asset = asset;
        }
    }
}
