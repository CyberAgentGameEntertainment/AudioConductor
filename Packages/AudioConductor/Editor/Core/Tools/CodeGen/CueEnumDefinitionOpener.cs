// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Models;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class CueEnumDefinitionOpener
    {
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);

            if (asset is not CueEnumDefinition definition)
                return false;

            CueEnumDefinitionEditorWindow.Open(definition);
            return true;
        }
    }
}
