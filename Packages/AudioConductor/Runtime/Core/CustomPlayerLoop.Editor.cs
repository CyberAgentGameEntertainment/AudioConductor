// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

#if UNITY_EDITOR

using UnityEditor;

namespace AudioConductor.Runtime.Core
{
    internal static partial class CustomPlayerLoop
    {
        [InitializeOnLoadMethod]
        private static void InitOnEditor()
        {
            EditorApplication.update += Update;
        }
    }
}

#endif
