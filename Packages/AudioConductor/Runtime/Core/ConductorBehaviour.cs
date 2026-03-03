// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class ConductorBehaviour : MonoBehaviour
    {
        internal AudioConductor Conductor { get; set; }

        private void Update()
        {
            Conductor?.Update(Time.deltaTime);
        }
    }
}
