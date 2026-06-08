// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class EmptyCueListRule : ICueSheetValidationRule
    {
        public void Validate(CueSheet cueSheet, ICueSheetValidationContext context)
        {
            if (cueSheet.cueList.Count == 0)
                context.AddWarning("CueSheet.EmptyCueList", "CueSheet has no cues.");
        }
    }
}
