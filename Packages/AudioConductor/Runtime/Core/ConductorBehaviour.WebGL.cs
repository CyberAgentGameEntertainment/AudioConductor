// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;

namespace AudioConductor.Core
{
    internal sealed partial class ConductorBehaviour
    {
        [DllImport("__Internal")]
        private static extern int AudioConductor_IsDocumentHidden();

        private bool _isSystemPaused;

        private void Awake()
        {
            AudioConductorWebGLBroadcaster.Register(this);
        }

        private void OnDestroy()
        {
            AudioConductorWebGLBroadcaster.Unregister(this);
        }

        // Fallback for iOS where visibilitychange can be unreliable.
        // Filter canvas-blur false positives by checking document.hidden:
        // canvas blur → hasFocus=false but document.hidden==false → skip.
        // Screen lock / tab switch → hasFocus=false and document.hidden==true → allow.
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && AudioConductor_IsDocumentHidden() == 0)
                return;
            NotifySystemPause(!hasFocus);
        }

        internal void NotifySystemPause(bool pause)
        {
            return; // temporary: disabled for debugging
            if (_isSystemPaused == pause)
                return;
            _isSystemPaused = pause;
            Conductor?.OnSystemPause(pause);
        }
    }
}

#endif
