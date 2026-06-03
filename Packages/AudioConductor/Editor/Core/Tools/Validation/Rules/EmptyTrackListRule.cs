// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class EmptyTrackListRule : ICueValidationRule
    {
        public void Validate(Cue cue, CueSheet cueSheet, AudioConductorSettings? settings,
            ICueSheetValidationContext context)
        {
            if (cue.trackList is null)
                return;

            if (cue.trackList.Count == 0)
                context.AddError("Cue.EmptyTrackList", $"Cue '{cue.name}' has no tracks.");
        }
    }
}
