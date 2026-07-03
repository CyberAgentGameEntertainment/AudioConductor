// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class ReferenceSampleRateUnsetRule : ICueSheetValidationRule
    {
        public void Validate(CueSheet cueSheet, ICueSheetValidationContext context)
        {
            if (cueSheet.referenceSampleRate != 0)
                return;

            foreach (var cue in cueSheet.cueList)
            foreach (var track in cue.trackList)
                if (track.audioClip != null)
                {
                    context.AddWarning("CueSheet.ReferenceSampleRateUnset",
                        "referenceSampleRate is not set. Sample positions may drift on platforms with different audio decoding frequencies (e.g. WebGL).");
                    return;
                }
        }
    }
}
