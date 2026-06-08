// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class ThrottleExceedsCueSheetRule : ICueValidationRule
    {
        public void Validate(Cue cue, CueSheet cueSheet, AudioConductorSettings? settings,
            ICueSheetValidationContext context)
        {
            if (cue.throttleLimit > 0 && cueSheet.throttleLimit > 0 &&
                cue.throttleLimit > cueSheet.throttleLimit)
                context.AddWarning("Cue.ThrottleExceedsCueSheet",
                    $"Cue '{cue.name}' throttleLimit ({cue.throttleLimit}) exceeds CueSheet throttleLimit ({cueSheet.throttleLimit}).");
        }
    }
}
