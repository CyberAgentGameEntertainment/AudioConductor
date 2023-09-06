// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class CueSheetExtensions
    {
        public static CueSheet Duplicate(this CueSheet cueSheet)
        {
            if (cueSheet == null)
                return null;

            return new CueSheet
            {
                name = cueSheet.name,
                throttleType = cueSheet.throttleType,
                throttleLimit = cueSheet.throttleLimit,
                volume = cueSheet.volume,
                pitch = cueSheet.pitch,
                pitchInvert = cueSheet.pitchInvert,
                cueList = cueSheet.cueList.Select(cue => cue.Duplicate()).ToList(),
            };
        }
    }
}
