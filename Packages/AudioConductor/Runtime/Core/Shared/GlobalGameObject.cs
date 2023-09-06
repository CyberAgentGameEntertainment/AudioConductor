// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core.Shared
{
    internal sealed class GlobalGameObject : MonoSingleton<GlobalGameObject>
    {
        protected override bool IsDontDestroy() => true;
    }
}
