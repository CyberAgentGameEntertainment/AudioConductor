// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Runtime.Core.Shared
{
    internal static class Calculator
    {
        internal static float CalcVolume(CueSheet cueSheet, Cue cue, Track track)
        {
            return ValueRangeConst.Volume.Clamp(CalcVolume(cueSheet) * CalcVolume(cue) * CalcVolume(track));
        }

        internal static float CalcVolume(CueSheet cueSheet)
        {
            return CalcVolume(cueSheet.volume, 0);
        }

        internal static float CalcVolume(Cue cue)
        {
            return CalcVolume(cue.volume, cue.volumeRange);
        }

        internal static float CalcVolume(Track track)
        {
            return CalcVolume(track.volume, track.volumeRange);
        }

        internal static float CalcVolume(float volume, float volumeRange)
        {
            var fixedVolume = ValueRangeConst.Volume.Clamp(volume);
            var fixedVolumeRange = ValueRangeConst.VolumeRange.Clamp(volumeRange);

            var random = Random.Range(-fixedVolumeRange, fixedVolumeRange);
            return ValueRangeConst.Volume.Clamp(fixedVolume + random);
        }

        internal static float CalcPitch(CueSheet cueSheet, Cue cue, Track track)
        {
            return Mathf.Clamp(CalcPitch(cueSheet) * CalcPitch(cue) * CalcPitch(track), -ValueRangeConst.Pitch.Max,
                ValueRangeConst.Pitch.Max);
        }

        internal static float CalcPitch(CueSheet cueSheet)
        {
            return CalcPitch(cueSheet.pitch, 0, cueSheet.pitchInvert);
        }

        internal static float CalcPitch(Cue cue)
        {
            return CalcPitch(cue.pitch, cue.pitchRange, cue.pitchInvert);
        }

        internal static float CalcPitch(Track track)
        {
            return CalcPitch(track.pitch, track.pitchRange, track.pitchInvert);
        }

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
