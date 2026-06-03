// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    internal sealed class ValidationContext : ICueSheetValidationContext
    {
        private readonly string _assetGuid;
        private readonly List<ValidationIssue> _issues = new();
        private string? _currentCueId;

        internal ValidationContext(string assetGuid)
        {
            _assetGuid = assetGuid;
        }

        internal IReadOnlyList<ValidationIssue> Issues => _issues;

        public void AddError(string code, string message)
        {
            _issues.Add(new ValidationIssue(ValidationSeverity.Error, code, message, _assetGuid, _currentCueId));
        }

        public void AddWarning(string code, string message)
        {
            _issues.Add(new ValidationIssue(ValidationSeverity.Warning, code, message, _assetGuid, _currentCueId));
        }

        internal void SetCurrentCue(string? cueId)
        {
            _currentCueId = cueId;
        }
    }
}
