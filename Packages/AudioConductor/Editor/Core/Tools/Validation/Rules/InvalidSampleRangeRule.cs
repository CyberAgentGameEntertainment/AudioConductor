// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class InvalidSampleRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (track.endSample > 0 && track.startSample >= track.endSample)
                context.AddError("Track.InvalidSampleRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has startSample ({track.startSample}) >= endSample ({track.endSample}).");
        }
    }
}
