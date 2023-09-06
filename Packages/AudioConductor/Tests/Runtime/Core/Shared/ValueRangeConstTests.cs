// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Shared;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Runtime.Core.Shared
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
    }
}
