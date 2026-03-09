// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Core
{
    public interface IFader
    {
        /// <summary>
        ///     Calculates the interpolated value at the given normalized time.
        /// </summary>
        /// <param name="t">Normalized time (0.0 to 1.0).</param>
        /// <param name="from">Value at the start of the fade.</param>
        /// <param name="to">Value at the end of the fade.</param>
        /// <returns>Interpolated value.</returns>
        float Evaluate(float t, float from, float to);
    }
}
