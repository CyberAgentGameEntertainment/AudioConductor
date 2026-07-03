// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL && !UNITY_EDITOR
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;

namespace AudioConductor.Core
{
    // Owns the single visibilitychange JS listener and forwards events to all active ConductorBehaviour instances.
    // Registers the listener on first instance, unregisters when the last instance is removed.
    internal static class AudioConductorWebGLBroadcaster
    {
        [DllImport("__Internal")]
        private static extern void AudioConductor_RegisterVisibilityChange(VisibilityChangeCallback callback);

        [DllImport("__Internal")]
        private static extern void AudioConductor_UnregisterVisibilityChange();

        private delegate void VisibilityChangeCallback(int isHidden);

        // Keep a rooted reference to prevent GC collection of the delegate in IL2CPP/WebGL.
        private static readonly VisibilityChangeCallback _visibilityChangeCallback = OnVisibilityChangedNative;
        private static readonly List<ConductorBehaviour> _instances = new();

        internal static void Register(ConductorBehaviour instance)
        {
            if (_instances.Count == 0)
                AudioConductor_RegisterVisibilityChange(_visibilityChangeCallback);
            _instances.Add(instance);
        }

        internal static void Unregister(ConductorBehaviour instance)
        {
            _instances.Remove(instance);
            if (_instances.Count == 0)
                AudioConductor_UnregisterVisibilityChange();
        }

        [MonoPInvokeCallback(typeof(VisibilityChangeCallback))]
        private static void OnVisibilityChangedNative(int isHidden)
        {
            foreach (var instance in _instances)
                instance.NotifySystemPause(isHidden == 1);
        }
    }
}

#endif
