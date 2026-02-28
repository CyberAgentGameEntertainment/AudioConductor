// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core.Shared
{
    internal sealed class GlobalGameObject : MonoSingleton<GlobalGameObject>
    {
        protected override bool IsDontDestroy()
        {
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            ResetStaticFields();
        }
    }
}
