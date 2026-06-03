// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    /// <summary>
    ///     Validation rule applied at the CueSheet level.
    ///     Implement this interface and place the class in an Editor assembly to have it
    ///     discovered and executed automatically.
    /// </summary>
    public interface ICueSheetValidationRule
    {
        /// <summary>Validates the given <paramref name="cueSheet" /> and reports issues via <paramref name="context" />.</summary>
        void Validate(CueSheet cueSheet, ICueSheetValidationContext context);
    }
}
