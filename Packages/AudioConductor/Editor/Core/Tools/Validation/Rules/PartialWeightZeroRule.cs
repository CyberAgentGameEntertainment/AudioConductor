// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Linq;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class PartialWeightZeroRule : ICueValidationRule
    {
        public void Validate(Cue cue, CueSheet cueSheet, AudioConductorSettings? settings,
            ICueSheetValidationContext context)
        {
            if (cue.playType != CuePlayType.Random)
                return;

            // When all tracks have weight 0, the runtime falls back to equal-probability selection — valid, no warning needed.
            if (cue.trackList.All(t => t is null || t.randomWeight == 0))
                return;

            foreach (var track in cue.trackList)
            {
                if (track?.audioClip == null)
                    continue;
                if (track.randomWeight != 0)
                    continue;
                context.AddWarning("Track.PartialWeightZero",
                    $"Track '{track.name}' in Cue '{cue.name}' has randomWeight = 0 in a Random cue.");
            }
        }
    }
}
