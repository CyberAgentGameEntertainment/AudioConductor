// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;

namespace AudioConductor.Core
{
    internal sealed partial class ConductorBehaviour : MonoBehaviour
    {
        internal Conductor? Conductor { get; set; }

        private void Update()
        {
            Conductor?.Update(Time.deltaTime);
        }
    }
}
