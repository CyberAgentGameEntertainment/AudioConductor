// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using NUnit.Framework;

namespace Tests.Runtime.Core.Shared
{
    public class CalculatorTests
    {
        private const int TestCount = 20;

        [TestCase(1, 1)]
        [TestCase(0, 0)]
        [TestCase(-1, 0)]
        [TestCase(2, 1)]
        [TestCase(0.5f, 0.5f)]
        public void CalcVolume_CueSheet(float testValue, float expected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var cueSheet = new CueSheet { volume = testValue };

                var volume = Calculator.CalcVolume(cueSheet);
                Assert.That(volume, Is.EqualTo(expected));
            }
        }

        [TestCase(1, 0, 1, 1)]
        [TestCase(1, 1, 0, 1)]
        [TestCase(1, 0.5f, 0.5f, 1)]
        [TestCase(0, 0.5f, 0, 0.5f)]
        public void CalcVolume_Cue(float testVolume, float testVolumeRange, float minExpected, float maxExpected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var cue = new Cue { volume = testVolume, volumeRange = testVolumeRange };

                var volume = Calculator.CalcVolume(cue);
                Assert.That(volume, Is.GreaterThanOrEqualTo(minExpected));
                Assert.That(volume, Is.LessThanOrEqualTo(maxExpected));
            }
        }

        [TestCase(1, 0, 1, 1)]
        [TestCase(1, 1, 0, 1)]
        [TestCase(1, 0.5f, 0.5f, 1)]
        [TestCase(0, 0.5f, 0, 0.5f)]
        public void CalcVolume_Track(float testVolume, float testVolumeRange, float minExpected, float maxExpected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var track = new Track { volume = testVolume, volumeRange = testVolumeRange };

                var volume = Calculator.CalcVolume(track);
                Assert.That(volume, Is.GreaterThanOrEqualTo(minExpected));
                Assert.That(volume, Is.LessThanOrEqualTo(maxExpected));
            }
        }

        [TestCase(1, 1, 0, 1, 0, 1, 1)]
        [TestCase(1, 1, 0, 0.5f, 0, 0.5f, 0.5f)]
        [TestCase(1, 0.5f, 0, 1, 0, 0.5f, 0.5f)]
        [TestCase(0.5f, 1, 0, 1, 0, 0.5f, 0.5f)]
        [TestCase(1, 0.5f, 0, 0.5f, 0, 0.25f, 0.25f)]
        [TestCase(0.5f, 0.5f, 0, 1, 0, 0.25f, 0.25f)]
        [TestCase(0.5f, 1, 0, 0.5f, 0, 0.25f, 0.25f)]
        [TestCase(1, 1, 0.1f, 1, 0, 0.9f, 1)]
        [TestCase(1, 1, 0, 1, 0.1f, 0.9f, 1)]
        [TestCase(1, 1, 0.1f, 1, 0.1f, 0.81f, 1)]
        public void CalcVolume(float cueSheetVolume,
                               float cueVolume, float cueVolumeRange,
                               float trackVolume, float trackVolumeRange,
                               float minExpected, float maxExpected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var cueSheet = new CueSheet { volume = cueSheetVolume };
                var cue = new Cue { volume = cueVolume, volumeRange = cueVolumeRange };
                var track = new Track { volume = trackVolume, volumeRange = trackVolumeRange };

                var volume = Calculator.CalcVolume(cueSheet, cue, track);
                Assert.That(volume, Is.GreaterThanOrEqualTo(minExpected));
                Assert.That(volume, Is.LessThanOrEqualTo(maxExpected));
            }
        }

        [TestCase(1, false, 1)]
        [TestCase(1, true, -1)]
        [TestCase(0, false, 0.01f)]
        [TestCase(0, true, -0.01f)]
        [TestCase(4, false, 3)]
        [TestCase(4, true, -3)]
        [TestCase(0.5f, false, 0.5f)]
        [TestCase(0.5f, true, -0.5f)]
        public void CalcPitch_CueSheet(float testPitch, bool testPitchInvert, float expected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var cueSheet = new CueSheet { pitch = testPitch, pitchInvert = testPitchInvert };

                var pitch = Calculator.CalcPitch(cueSheet);
                Assert.That(pitch, Is.EqualTo(expected));
            }
        }

        [TestCase(1, 0, false, 1, 1)]
        [TestCase(1, 0, true, -1, -1)]
        [TestCase(1, 0.5f, false, 0.5f, 1.5f)]
        [TestCase(1, 0.5f, true, -1.5f, -0.5f)]
        [TestCase(0, 0, false, 0, 0.01f)]
        [TestCase(0, 0, true, -0.01f, -0.01f)]
        [TestCase(0, 0.5f, false, 0.01f, 0.51f)]
        [TestCase(0, 0.5f, true, -0.51f, -0.01f)]
        [TestCase(4, 0, false, 3, 3)]
        [TestCase(4, 0, true, -3, -3)]
        [TestCase(4, 0.5f, false, 2.5f, 3)]
        [TestCase(4, 0.5f, true, -3, -2.5f)]
        public void CalcPitch_Cue(float testPitch, float testPitchRange, bool testPitchInvert,
                                  float minExpected, float maxExpected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var cue = new Cue { pitch = testPitch, pitchRange = testPitchRange, pitchInvert = testPitchInvert };

                var pitch = Calculator.CalcPitch(cue);
                Assert.That(pitch, Is.GreaterThanOrEqualTo(minExpected));
                Assert.That(pitch, Is.LessThanOrEqualTo(maxExpected));
            }
        }

        [TestCase(1, 0, false, 1, 1)]
        [TestCase(1, 0, true, -1, -1)]
        [TestCase(1, 0.5f, false, 0.5f, 1.5f)]
        [TestCase(1, 0.5f, true, -1.5f, -0.5f)]
        [TestCase(0, 0, false, 0, 0.01f)]
        [TestCase(0, 0, true, -0.01f, -0.01f)]
        [TestCase(0, 0.5f, false, 0.01f, 0.51f)]
        [TestCase(0, 0.5f, true, -0.51f, -0.01f)]
        [TestCase(4, 0, false, 3, 3)]
        [TestCase(4, 0, true, -3, -3)]
        [TestCase(4, 0.5f, false, 2.5f, 3)]
        [TestCase(4, 0.5f, true, -3, -2.5f)]
        public void CalcPitch_Track(float testPitch, float testPitchRange, bool testPitchInvert,
                                    float minExpected, float maxExpected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var track = new Track { pitch = testPitch, pitchRange = testPitchRange, pitchInvert = testPitchInvert };

                var pitch = Calculator.CalcPitch(track);
                Assert.That(pitch, Is.GreaterThanOrEqualTo(minExpected));
                Assert.That(pitch, Is.LessThanOrEqualTo(maxExpected));
            }
        }

        [TestCase(1, false, 1, 0, false, 1, 0, false, 1, 1)]
        [TestCase(1, false, 1, 0, false, 1, 0, true, -1, -1)]
        [TestCase(1, false, 1, 0, true, 1, 0, false, -1, -1)]
        [TestCase(1, true, 1, 0, false, 1, 0, false, -1, -1)]
        [TestCase(1, true, 1, 0, true, 1, 0, false, 1, 1)]
        [TestCase(1, true, 1, 0, false, 1, 0, true, 1, 1)]
        [TestCase(1, false, 1, 0, true, 1, 0, true, 1, 1)]
        [TestCase(1, false, 1, 0, false, 0.5f, 0f, false, 0.5f, 0.5f)]
        [TestCase(1, false, 1, 0.2f, false, 0.5f, 0, false, 0.4f, 0.6f)]
        [TestCase(1, false, 1, 0, false, 0.5f, 0.2f, false, 0.3f, 0.7f)]
        public void CalcPitch(float cueSheetPitch, bool cueSheetPitchInvert,
                              float cuePitch, float cuePitchRange, bool cuePitchInvert,
                              float trackPitch, float trackPitchRange, bool trackPitchInvert,
                              float minExpected, float maxExpected)
        {
            for (var i = 0; i < TestCount; ++i)
            {
                var cueSheet = new CueSheet { pitch = cueSheetPitch, pitchInvert = cueSheetPitchInvert };
                var cue = new Cue { pitch = cuePitch, pitchRange = cuePitchRange, pitchInvert = cuePitchInvert };
                var track = new Track
                    { pitch = trackPitch, pitchRange = trackPitchRange, pitchInvert = trackPitchInvert };

                var pitch = Calculator.CalcPitch(cueSheet, cue, track);
                Assert.That(pitch, Is.GreaterThanOrEqualTo(minExpected));
                Assert.That(pitch, Is.LessThanOrEqualTo(maxExpected));
            }
        }
    }
}
