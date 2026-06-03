// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation.Models
{
    internal sealed class ValidationResultRow
    {
        internal ValidationResultRow(
            ValidationIssue? issue,
            CueSheetAsset? asset,
            string displayText,
            bool isIssueRow)
        {
            Issue = issue;
            Asset = asset;
            DisplayText = displayText;
            IsIssueRow = isIssueRow;
        }

        internal ValidationIssue? Issue { get; }
        internal CueSheetAsset? Asset { get; }
        internal string DisplayText { get; }
        internal bool IsIssueRow { get; }
    }
}
