// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    public interface IFader
    {
        /// <summary>
        ///     Calculates the interpolated volume at the given normalized time.
        /// </summary>
        /// <param name="t">Normalized time (0.0 to 1.0).</param>
        /// <param name="startVolume">Volume at the start of the fade.</param>
        /// <param name="targetVolume">Volume at the end of the fade.</param>
        /// <returns>Interpolated volume value.</returns>
        float Evaluate(float t, float startVolume, float targetVolume);
    }
}
