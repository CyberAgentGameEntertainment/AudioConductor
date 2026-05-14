// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor
{
    internal static class CueSheetAssetOpener
    {
#if UNITY_6000_5_OR_NEWER
        [OnOpenAsset(0)]
        public static bool OnOpen(EntityId entityId, int line)
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
            if (asset is not CueSheetAsset data)
                return false;

            CueSheetAssetEditorWindow.Open(data);
            return true;
        }
    }
}
