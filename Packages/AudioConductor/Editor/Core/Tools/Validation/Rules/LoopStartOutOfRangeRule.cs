// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class LoopStartOutOfRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (!track.isLoop)
                return;

            if (track.endSample > 0 && track.loopStartSample >= track.endSample)
                context.AddError("Track.LoopStartOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has loopStartSample ({track.loopStartSample}) >= endSample ({track.endSample}).");
        }
    }
}
