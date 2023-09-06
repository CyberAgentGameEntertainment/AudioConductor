// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Shared
{
    internal static class Calculator
    {
        internal static float CalcVolume(CueSheet cueSheet, Cue cue, Track track)
            => ValueRangeConst.Volume.Clamp(CalcVolume(cueSheet) * CalcVolume(cue) * CalcVolume(track));

        internal static float CalcVolume(CueSheet cueSheet)
            => CalcVolume(cueSheet.volume, 0);

        internal static float CalcVolume(Cue cue)
            => CalcVolume(cue.volume, cue.volumeRange);

        internal static float CalcVolume(Track track)
            => CalcVolume(track.volume, track.volumeRange);

        internal static float CalcVolume(float volume, float volumeRange)
        {
            var fixedVolume = ValueRangeConst.Volume.Clamp(volume);
            var fixedVolumeRange = ValueRangeConst.VolumeRange.Clamp(volumeRange);

            var random = Random.Range(-fixedVolumeRange, fixedVolumeRange);
            return ValueRangeConst.Volume.Clamp(fixedVolume + random);
        }

        internal static float CalcPitch(CueSheet cueSheet, Cue cue, Track track)
            => Mathf.Clamp(CalcPitch(cueSheet) * CalcPitch(cue) * CalcPitch(track), -ValueRangeConst.Pitch.Max,
                           ValueRangeConst.Pitch.Max);

        internal static float CalcPitch(CueSheet cueSheet)
            => CalcPitch(cueSheet.pitch, 0, cueSheet.pitchInvert);

        internal static float CalcPitch(Cue cue)
            => CalcPitch(cue.pitch, cue.pitchRange, cue.pitchInvert);

        internal static float CalcPitch(Track track)
            => CalcPitch(track.pitch, track.pitchRange, track.pitchInvert);

        internal static float CalcPitch(float pitch, float pitchRange, bool pitchInvert)
        {
            var fixedPitch = ValueRangeConst.Pitch.Clamp(pitch);
            var fixedPitchRange = ValueRangeConst.PitchRange.Clamp(pitchRange);

            var random = Random.Range(-fixedPitchRange, fixedPitchRange);
            var value = ValueRangeConst.Pitch.Clamp(fixedPitch + random);
            return pitchInvert ? -value : value;
        }
    }
}
