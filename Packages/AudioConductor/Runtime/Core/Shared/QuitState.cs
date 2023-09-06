// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core.Shared
{
#if UNITY_EDITOR

    internal static class QuittingState
    {
        /// Order of application quit callback.
        /// 1. MonoBehaviour.OnApplicationQuit
        /// 2. Application.quitting
        /// 3. MonoBehaviour.OnDestroy
        private static bool _isQuiting;

        public static bool IsQuitting => _isQuiting && Application.isPlaying;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnInitialize()
        {
            Application.quitting += OnQuitting;
            _isQuiting = false;
        }

        private static void OnQuitting()
        {
            _isQuiting = true;
            Application.quitting -= OnQuitting;
        }
    }

#endif
}
