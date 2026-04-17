// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor;
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
