// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.Validation.Models
{
    internal sealed class EditorPrefsValidationSettingsPreferences : IValidationSettingsPreferences
    {
        private const string ValidationKey = "AudioConductor.ValidationSelectedSettingsGuid";

        public string LoadSelectedGuid()
        {
            return EditorPrefs.GetString(ValidationKey,
                EditorPrefs.GetString(EditorPrefsKeys.SelectedSettingsGuid, string.Empty));
        }

        public void SaveSelectedGuid(string guid)
        {
            EditorPrefs.SetString(ValidationKey, guid);
        }
    }
}
