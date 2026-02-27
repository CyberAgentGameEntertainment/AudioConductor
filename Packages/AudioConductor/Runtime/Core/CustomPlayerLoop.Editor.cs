// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal static partial class CustomPlayerLoop
    {
        [InitializeOnLoadMethod]
        private static void InitOnEditor()
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
        }

        private static void EditorUpdate()
        {
            if (Application.isPlaying)
                return;

            Update();
        }
    }
}

#endif
