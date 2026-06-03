// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class StartSampleOutOfRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (track.startSample < 0)
                context.AddError("Track.StartSampleOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has startSample ({track.startSample}) out of valid range [0, ...].");

            if (track.audioClip == null)
                return;

            if (track.startSample >= track.audioClip.samples)
                context.AddError("Track.StartSampleOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has startSample ({track.startSample}) out of valid range [0, {track.audioClip.samples - 1}].");
        }
    }
}
