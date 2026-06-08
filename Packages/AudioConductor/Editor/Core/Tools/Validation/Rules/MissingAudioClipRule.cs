// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class MissingAudioClipRule : ITrackValidationRule
    {
        public void Validate(Track track, Cue cue, ICueSheetValidationContext context)
        {
            if (track.audioClip == null)
                context.AddError("Track.MissingAudioClip",
                    $"Track '{track.name}' in Cue '{cue.name}' has no AudioClip.");
        }
    }
}
