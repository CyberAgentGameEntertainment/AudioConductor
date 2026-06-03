// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Core.Tools.Validation.Models
{
    internal interface IValidationSettingsPreferences
    {
        string LoadSelectedGuid();
        void SaveSelectedGuid(string guid);
    }
}
