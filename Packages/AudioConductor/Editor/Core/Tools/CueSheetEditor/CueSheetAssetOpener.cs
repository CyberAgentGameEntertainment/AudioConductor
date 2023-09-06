// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;
using UnityEditor;
using UnityEditor.Callbacks;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor
{
    internal static class CueSheetAssetOpener
    {
        [OnOpenAsset(0)]
        public static bool OnOpen(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID);

            if (asset is not CueSheetAsset data)
                return false;

            CueSheetAssetEditorWindow.Open(data);
            return true;
        }
    }
}
