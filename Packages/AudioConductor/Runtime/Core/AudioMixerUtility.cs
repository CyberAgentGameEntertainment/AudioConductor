// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;

namespace AudioConductor.Core
{
    public static class AudioMixerUtility
    {
        private const float MinDecibel = -80f;

        public static float ToDecibel(float linearVolume)
        {
            if (linearVolume <= 0f)
                return MinDecibel;

            return Mathf.Log10(linearVolume) * 20f;
        }

        public static float ToLinear(float decibel)
        {
            if (decibel <= MinDecibel)
                return 0f;

            return Mathf.Pow(10f, decibel / 20f);
        }
    }
}
