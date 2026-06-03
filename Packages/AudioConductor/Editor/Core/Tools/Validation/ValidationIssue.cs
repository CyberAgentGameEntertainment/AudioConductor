// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Core.Tools.Validation
{
    internal sealed class ValidationIssue
    {
        internal ValidationIssue(
            ValidationSeverity severity,
            string code,
            string message,
            string assetGuid,
            string? cueEditorId)
        {
            Severity = severity;
            Code = code;
            Message = message;
            AssetGuid = assetGuid;
            CueEditorId = cueEditorId;
        }

        internal ValidationSeverity Severity { get; }
        internal string Code { get; }
        internal string Message { get; }
        internal string AssetGuid { get; }
        internal string? CueEditorId { get; }
    }
}
