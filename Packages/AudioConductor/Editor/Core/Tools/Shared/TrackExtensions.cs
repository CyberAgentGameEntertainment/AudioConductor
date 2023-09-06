// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class TrackExtensions
    {
        public static Track Duplicate(this Track track)
        {
            if (track == null)
                return null;

            return new Track
            {
                name = track.name,
                audioClip = track.audioClip,
                volume = track.volume,
                volumeRange = track.volumeRange,
                pitch = track.pitch,
                pitchRange = track.pitchRange,
                pitchInvert = track.pitchInvert,
                startSample = track.startSample,
                endSample = track.endSample,
                loopStartSample = track.loopStartSample,
                isLoop = track.isLoop,
                randomWeight = track.randomWeight,
                priority = track.priority,
                fadeTime = track.fadeTime
            };
        }
    }
}
