// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules
{
    internal sealed class InvalidCategoryIdRule : ICueValidationRule
    {
        public void Validate(Cue cue, CueSheet cueSheet, AudioConductorSettings? settings,
            ICueSheetValidationContext context)
        {
            if (settings == null)
                return;

            foreach (var category in settings.categoryList)
                if (category.id == cue.categoryId)
                    return;

            context.AddWarning("Cue.InvalidCategoryId",
                $"Cue '{cue.name}' categoryId ({cue.categoryId}) not found in Settings.");
        }
    }
}
