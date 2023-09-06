// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

#if UNITY_EDITOR

using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed partial class AudioConductorInternal
    {
        // https://docs.unity3d.com/ja/2019.3/Manual/DomainReloading.html
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetInstance()
        {
            _instance = null;
        }
    }
}

#endif
