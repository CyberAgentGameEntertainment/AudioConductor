// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Tools.CueSheetEditor.Enums;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface IInspectorModel
    {
        InspectorType InspectorType { get; }

        ICueInspectorModel? CueInspectorModel { get; }

        ITrackInspectorModel? TrackInspectorModel { get; }
    }
}
