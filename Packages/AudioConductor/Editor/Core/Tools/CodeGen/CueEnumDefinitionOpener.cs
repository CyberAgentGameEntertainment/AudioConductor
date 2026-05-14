// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Models;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class CueEnumDefinitionOpener
    {
#if UNITY_6000_5_OR_NEWER
        [OnOpenAsset(0)]
        public static bool OnOpen(UnityEngine.EntityId entityId, int line)
            => OnOpenImpl(EditorUtility.EntityIdToObject(entityId));
#else
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            return OnOpenImpl(EditorUtility.InstanceIDToObject(instanceID));
        }
#endif

        private static bool OnOpenImpl(Object asset)
        {
            if (asset is not CueEnumDefinition definition)
                return false;

            CueEnumDefinitionEditorWindow.Open(definition);
            return true;
        }
    }
}
