// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    public static class Faders
    {
        public static readonly IFader Linear = new LinearFader();

        private sealed class LinearFader : IFader
        {
            public float Evaluate(float t, float startVolume, float targetVolume)
            {
                return Mathf.Lerp(startVolume, targetVolume, t);
            }
        }
    }
}
