// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Core.Tools.Validation
{
    /// <summary>
    ///     Provides methods to report validation issues from within a validation rule.
    ///     The asset GUID and cue editor ID are automatically supplied by the validator.
    /// </summary>
    public interface ICueSheetValidationContext
    {
        /// <summary>Reports an error-level validation issue.</summary>
        void AddError(string code, string message);

        /// <summary>Reports a warning-level validation issue.</summary>
        void AddWarning(string code, string message);
    }
}
