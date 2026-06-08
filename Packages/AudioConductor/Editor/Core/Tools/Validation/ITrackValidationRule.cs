// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Validation
{
    /// <summary>
    ///     Validation rule applied at the Track level.
    ///     Implement this interface and place the class in an Editor assembly to have it
    ///     discovered and executed automatically.
    /// </summary>
    public interface ITrackValidationRule
    {
        /// <summary>Validates the given <paramref name="track" /> and reports issues via <paramref name="context" />.</summary>
        void Validate(Track track, Cue cue, ICueSheetValidationContext context);
    }
}
