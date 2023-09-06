// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Core.Tools.CueSheetEditor.Enums;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface IInspectorModel
    {
        InspectorType InspectorType { get; }

        ICueInspectorModel CueInspectorModel { get; }

        ITrackInspectorModel TrackInspectorModel { get; }
    }
}
