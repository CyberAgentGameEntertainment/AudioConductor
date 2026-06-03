// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class EndSampleZeroRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (track.audioClip == null)
                return;

            if (track.endSample == 0)
                context.AddError("Track.EndSampleZero",
                    $"Track '{track.name}' in Cue '{cue.name}' has endSample = 0 (would stop immediately).");
        }
    }
}
