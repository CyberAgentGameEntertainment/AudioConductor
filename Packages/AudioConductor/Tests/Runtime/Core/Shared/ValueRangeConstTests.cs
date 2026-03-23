// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Core.Shared.Tests
{
    public static class ValueRangeConstTests
    {
        public class ThrottleLimitTests
        {
            [Test]
            public void LessThanMin()
            {
                const int testValue = ValueRangeConst.ThrottleLimit.Min - 1;
                var result = ValueRangeConst.ThrottleLimit.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.ThrottleLimit.Min));
            }

            [Test]
            public void EqualMin()
            {
                const int testValue = ValueRangeConst.ThrottleLimit.Min;
                var result = ValueRangeConst.ThrottleLimit.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.ThrottleLimit.Min));
            }

            [Test]
            public void GreaterThanMax()
            {
                const int testValue = ValueRangeConst.ThrottleLimit.Max + 1;
                var result = ValueRangeConst.ThrottleLimit.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.ThrottleLimit.Max));
            }

            [Test]
            public void EqualMax()
            {
                const int testValue = ValueRangeConst.ThrottleLimit.Max;
                var result = ValueRangeConst.ThrottleLimit.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.ThrottleLimit.Max));
            }

            [Test]
            public void InRange()
            {
                var testValue = Random.Range(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Max);
                var result = ValueRangeConst.ThrottleLimit.Clamp(testValue);
                Assert.That(result, Is.InRange(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Max));
            }
        }

        public class VolumeTests
        {
            [Test]
            public void LessThanMin()
            {
                const float testValue = ValueRangeConst.Volume.Min - 1;
                var result = ValueRangeConst.Volume.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Volume.Min));
            }

            [Test]
            public void EqualMin()
            {
                const float testValue = ValueRangeConst.Volume.Min;
                var result = ValueRangeConst.Volume.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Volume.Min));
            }

            [Test]
            public void GreaterThanMax()
            {
                const float testValue = ValueRangeConst.Volume.Max + 1;
                var result = ValueRangeConst.Volume.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Volume.Max));
            }

            [Test]
            public void EqualMax()
            {
                const float testValue = ValueRangeConst.Volume.Max;
                var result = ValueRangeConst.Volume.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Volume.Max));
            }

            [Test]
            public void InRange()
            {
                var testValue = Random.Range(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max);
                var result = ValueRangeConst.Volume.Clamp(testValue);
                Assert.That(result, Is.InRange(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max));
            }
        }

        public class VolumeRangeTests
        {
            [Test]
            public void LessThanMin()
            {
                const float testValue = ValueRangeConst.VolumeRange.Min - 1;
                var result = ValueRangeConst.VolumeRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.VolumeRange.Min));
            }

            [Test]
            public void EqualMin()
            {
                const float testValue = ValueRangeConst.VolumeRange.Min;
                var result = ValueRangeConst.VolumeRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.VolumeRange.Min));
            }

            [Test]
            public void GreaterThanMax()
            {
                const float testValue = ValueRangeConst.VolumeRange.Max + 1;
                var result = ValueRangeConst.VolumeRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.VolumeRange.Max));
            }

            [Test]
            public void EqualMax()
            {
                const float testValue = ValueRangeConst.VolumeRange.Max;
                var result = ValueRangeConst.VolumeRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.VolumeRange.Max));
            }

            [Test]
            public void InRange()
            {
                var testValue = Random.Range(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max);
                var result = ValueRangeConst.VolumeRange.Clamp(testValue);
                Assert.That(result, Is.InRange(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max));
            }
        }

        public class PitchTests
        {
            [Test]
            public void LessThanMin()
            {
                const float testValue = ValueRangeConst.Pitch.Min - 1;
                var result = ValueRangeConst.Pitch.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Pitch.Min));
            }

            [Test]
            public void EqualMin()
            {
                const float testValue = ValueRangeConst.Pitch.Min;
                var result = ValueRangeConst.Pitch.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Pitch.Min));
            }

            [Test]
            public void GreaterThanMax()
            {
                const float testValue = ValueRangeConst.Pitch.Max + 1;
                var result = ValueRangeConst.Pitch.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Pitch.Max));
            }

            [Test]
            public void EqualMax()
            {
                const float testValue = ValueRangeConst.Pitch.Max;
                var result = ValueRangeConst.Pitch.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.Pitch.Max));
            }

            [Test]
            public void InRange()
            {
                var testValue = Random.Range(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Max);
                var result = ValueRangeConst.Pitch.Clamp(testValue);
                Assert.That(result, Is.InRange(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Max));
            }
        }

        public class PitchRangeTests
        {
            [Test]
            public void LessThanMin()
            {
                const float testValue = ValueRangeConst.PitchRange.Min - 1;
                var result = ValueRangeConst.PitchRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.PitchRange.Min));
            }

            [Test]
            public void EqualMin()
            {
                const float testValue = ValueRangeConst.PitchRange.Min;
                var result = ValueRangeConst.PitchRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.PitchRange.Min));
            }

            [Test]
            public void GreaterThanMax()
            {
                const float testValue = ValueRangeConst.PitchRange.Max + 1;
                var result = ValueRangeConst.PitchRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.PitchRange.Max));
            }

            [Test]
            public void EqualMax()
            {
                const float testValue = ValueRangeConst.PitchRange.Max;
                var result = ValueRangeConst.PitchRange.Clamp(testValue);
                Assert.That(result, Is.EqualTo(ValueRangeConst.PitchRange.Max));
            }

            [Test]
            public void InRange()
            {
                var testValue = Random.Range(ValueRangeConst.PitchRange.Min, ValueRangeConst.PitchRange.Max);
                var result = ValueRangeConst.PitchRange.Clamp(testValue);
                Assert.That(result, Is.InRange(ValueRangeConst.PitchRange.Min, ValueRangeConst.PitchRange.Max));
            }
        }

        public class StartSampleTests
        {
            [Test]
            public void LessThanMin()
            {
                var result = ValueRangeConst.StartSample.Clamp(-1, 100);
                Assert.That(result, Is.EqualTo(ValueRangeConst.StartSample.Min));
            }

            [Test]
            public void EqualMin()
            {
                var result = ValueRangeConst.StartSample.Clamp(0, 100);
                Assert.That(result, Is.EqualTo(0));
            }

            [Test]
            public void EqualMax()
            {
                // Max is audioClipSamples - 1
                var result = ValueRangeConst.StartSample.Clamp(99, 100);
                Assert.That(result, Is.EqualTo(99));
            }

            [Test]
            public void GreaterThanMax_ClampsToAudioClipSamplesMinusOne()
            {
                var result = ValueRangeConst.StartSample.Clamp(200, 100);
                Assert.That(result, Is.EqualTo(99));
            }

            [Test]
            public void ZeroAudioClipSamples_ReturnsMin()
            {
                var result = ValueRangeConst.StartSample.Clamp(10, 0);
                Assert.That(result, Is.EqualTo(ValueRangeConst.StartSample.Min));
            }
        }

        public class EndSampleTests
        {
            [Test]
            public void LessThanMin()
            {
                var result = ValueRangeConst.EndSample.Clamp(-1, 100);
                Assert.That(result, Is.EqualTo(ValueRangeConst.EndSample.Min));
            }

            [Test]
            public void EqualMin()
            {
                var result = ValueRangeConst.EndSample.Clamp(0, 100);
                Assert.That(result, Is.EqualTo(0));
            }

            [Test]
            public void EqualMax_AllowsAudioClipSamples()
            {
                // EndSample upper bound is audioClipSamples (not -1), representing exclusive end
                var result = ValueRangeConst.EndSample.Clamp(100, 100);
                Assert.That(result, Is.EqualTo(100));
            }

            [Test]
            public void GreaterThanMax_ClampsToAudioClipSamples()
            {
                var result = ValueRangeConst.EndSample.Clamp(200, 100);
                Assert.That(result, Is.EqualTo(100));
            }

            [Test]
            public void ZeroAudioClipSamples_ReturnsMin()
            {
                var result = ValueRangeConst.EndSample.Clamp(10, 0);
                Assert.That(result, Is.EqualTo(ValueRangeConst.EndSample.Min));
            }

            [Test]
            public void EndSample_AllowsOneMoreThanStartSample_Max()
            {
                // EndSample.Max == audioClipSamples, while StartSample.Max == audioClipSamples - 1
                const int audioClipSamples = 100;
                var startMax = ValueRangeConst.StartSample.Clamp(audioClipSamples, audioClipSamples);
                var endMax = ValueRangeConst.EndSample.Clamp(audioClipSamples, audioClipSamples);
                Assert.That(endMax, Is.EqualTo(startMax + 1));
            }
        }

        public class LoopStartSampleTests
        {
            [Test]
            public void LessThanMin()
            {
                var result = ValueRangeConst.LoopStartSample.Clamp(-1, 100);
                Assert.That(result, Is.EqualTo(ValueRangeConst.LoopStartSample.Min));
            }

            [Test]
            public void EqualMax_ClampsToAudioClipSamplesMinusOne()
            {
                var result = ValueRangeConst.LoopStartSample.Clamp(99, 100);
                Assert.That(result, Is.EqualTo(99));
            }

            [Test]
            public void GreaterThanMax_ClampsToAudioClipSamplesMinusOne()
            {
                var result = ValueRangeConst.LoopStartSample.Clamp(200, 100);
                Assert.That(result, Is.EqualTo(99));
            }

            [Test]
            public void ZeroAudioClipSamples_ReturnsMin()
            {
                var result = ValueRangeConst.LoopStartSample.Clamp(10, 0);
                Assert.That(result, Is.EqualTo(ValueRangeConst.LoopStartSample.Min));
            }

            [Test]
            public void LoopStartSample_HasSameUpperBoundAsStartSample()
            {
                // Both LoopStartSample and StartSample are clamped to audioClipSamples - 1
                const int audioClipSamples = 100;
                var startResult = ValueRangeConst.StartSample.Clamp(audioClipSamples, audioClipSamples);
                var loopStartResult = ValueRangeConst.LoopStartSample.Clamp(audioClipSamples, audioClipSamples);
                Assert.That(loopStartResult, Is.EqualTo(startResult));
            }
        }
    }
}
