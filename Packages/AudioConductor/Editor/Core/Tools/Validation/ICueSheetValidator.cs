// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    internal interface ICueSheetValidator
    {
        IReadOnlyList<ValidationIssue> Validate(CueSheetAsset asset, AudioConductorSettings? settings);
    }
}
