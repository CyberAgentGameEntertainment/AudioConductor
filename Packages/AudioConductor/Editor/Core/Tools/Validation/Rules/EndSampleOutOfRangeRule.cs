// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class EndSampleOutOfRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (track.endSample < ValueRangeConst.EndSample.Min)
                context.AddError("Track.EndSampleOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has endSample ({track.endSample}) out of valid range [{ValueRangeConst.EndSample.Min}, ...].");

            if (track.audioClip == null)
                return;

            if (track.endSample > track.audioClip.samples)
                context.AddError("Track.EndSampleOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has endSample ({track.endSample}) out of valid range [0, {track.audioClip.samples}].");
        }
    }
}
