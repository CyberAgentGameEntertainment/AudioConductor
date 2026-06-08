// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class TrackScalarRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (track.volume is < ValueRangeConst.Volume.Min or > ValueRangeConst.Volume.Max)
                context.AddError("Track.VolumeOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has volume ({track.volume}) out of valid range [{ValueRangeConst.Volume.Min}, {ValueRangeConst.Volume.Max}].");

            if (track.pitch is < ValueRangeConst.Pitch.Min or > ValueRangeConst.Pitch.Max)
                context.AddError("Track.PitchOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has pitch ({track.pitch}) out of valid range [{ValueRangeConst.Pitch.Min}, {ValueRangeConst.Pitch.Max}].");

            if (track.fadeTime < ValueRangeConst.FadeTime.Min)
                context.AddError("Track.FadeTimeOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has fadeTime ({track.fadeTime}) out of valid range [{ValueRangeConst.FadeTime.Min}, ...].");
        }
    }
}
