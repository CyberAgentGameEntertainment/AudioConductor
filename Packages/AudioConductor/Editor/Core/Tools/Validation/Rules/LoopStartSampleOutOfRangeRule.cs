// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class LoopStartSampleOutOfRangeRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (!track.isLoop)
                return;

            if (track.loopStartSample < 0)
                context.AddError("Track.LoopStartSampleOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has loopStartSample ({track.loopStartSample}) out of valid range [0, ...].");

            if (track.audioClip == null)
                return;

            if (track.loopStartSample >= track.audioClip.samples)
                context.AddError("Track.LoopStartSampleOutOfRange",
                    $"Track '{track.name}' in Cue '{cue.name}' has loopStartSample ({track.loopStartSample}) out of valid range [0, {track.audioClip.samples - 1}].");
        }
    }
}
