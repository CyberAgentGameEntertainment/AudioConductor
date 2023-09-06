// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class CueExtensions
    {
        public static Cue Duplicate(this Cue cue)
        {
            if (cue == null)
                return null;

            return new Cue
            {
                name = cue.name,
                categoryId = cue.categoryId,
                throttleType = cue.throttleType,
                throttleLimit = cue.throttleLimit,
                volume = cue.volume,
                volumeRange = cue.volumeRange,
                pitch = cue.pitch,
                pitchRange = cue.pitchRange,
                pitchInvert = cue.pitchInvert,
                playType = cue.playType,
                trackList = cue.trackList.Select(track => track.Duplicate()).ToList()
            };
        }
    }
}
