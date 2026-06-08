// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class DuplicateCueIdRule : ICueSheetValidationRule
    {
        public void Validate(CueSheet cueSheet, ICueSheetValidationContext context)
        {
            if (CueIdAssigner.HasDuplicateCueIds(cueSheet.cueList))
                context.AddError("CueSheet.DuplicateCueId",
                    "CueSheet contains duplicate or unassigned cueIds. Re-import the asset to fix.");
        }
    }
}
