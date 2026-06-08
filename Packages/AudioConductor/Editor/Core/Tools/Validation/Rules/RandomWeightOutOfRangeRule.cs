// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class RandomWeightOutOfRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (cue.playType != CuePlayType.Random)
                return;

            if (track.randomWeight < ValueRangeConst.RandomWeight.Min)
                context.AddError("Track.RandomWeightOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has randomWeight ({track.randomWeight}) out of valid range [{ValueRangeConst.RandomWeight.Min}, ...].");
        }
    }
}
