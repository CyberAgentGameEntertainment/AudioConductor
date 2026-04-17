// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface IOtherOperationPaneModel
    {
        string CueSheetName { get; }

        string CreateCsvText();

        bool ImportCsv(IReadOnlyList<string> lines);
    }
}
