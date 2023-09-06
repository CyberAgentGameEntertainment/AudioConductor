// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor;
using AudioConductor.Runtime.Core.Models;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.CustomEditors
{
    [CustomEditor(typeof(CueSheetAsset))]
    internal sealed class CueSheetAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var data = (CueSheetAsset)target;

            if (GUILayout.Button("Open EditorWindow"))
                CueSheetAssetEditorWindow.Open(data);

            GUI.enabled = false;
            base.OnInspectorGUI();
            GUI.enabled = true;
        }
    }
}
