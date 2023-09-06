// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.Shared;
using NUnit.Framework;

namespace AudioConductor.Tests.Editor.Core.Tools.Shared
{
    internal class MixedValueTests
    {
        public enum TestEnum
        {
            One,
            Two,
            Three
        }

        #region int

        [TestCase(1, true)]
        [TestCase(3, false)]
        public void Equal_int(int value, bool flag)
        {
            var mixedValue1 = new MixedValue<int>(value, flag);
            var mixedValue2 = new MixedValue<int>(value, flag);

            Assert.True(mixedValue1.Equals(mixedValue2));
            Assert.AreEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(1, 2)]
        [TestCase(0, -1)]
        public void NotEqual_int_DifferentValues(int value1, int value2)
        {
            var mixedValue1 = new MixedValue<int>(value1, true);
            var mixedValue2 = new MixedValue<int>(value2, true);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(1)]
        [TestCase(3)]
        public void NotEqual_int_DifferentHasMultipleDifferentValues(int value)
        {
            var mixedValue1 = new MixedValue<int>(value, true);
            var mixedValue2 = new MixedValue<int>(value, false);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(1, true)]
        [TestCase(3, false)]
        public void ToString_int(int value, bool flag)
        {
            var mixedValue = new MixedValue<int>(value, flag);
            Assert.AreEqual(mixedValue.ToString(), $"{value}:{flag}");
        }

        #endregion

        #region float

        [TestCase(1.001f, true)]
        [TestCase(2.34f, false)]
        public void Equal_float(float value, bool flag)
        {
            var mixedValue1 = new MixedValue<float>(value, flag);
            var mixedValue2 = new MixedValue<float>(value, flag);

            Assert.True(mixedValue1.Equals(mixedValue2));
            Assert.AreEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(0.00001234000056f, 0.0000123400057f)]
        [TestCase(-4.3f, 0.5f)]
        public void NotEqual_float_DifferentValues(float value1, float value2)
        {
            var mixedValue1 = new MixedValue<float>(value1, true);
            var mixedValue2 = new MixedValue<float>(value2, true);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(11.3f)]
        [TestCase(-5.7f)]
        public void NotEqual_float_DifferentHasMultipleDifferentValues(float value)
        {
            var mixedValue1 = new MixedValue<float>(value, true);
            var mixedValue2 = new MixedValue<float>(value, false);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(1.001f, true)]
        [TestCase(2.34f, false)]
        public void ToString_float(float value, bool flag)
        {
            var mixedValue = new MixedValue<float>(value, flag);
            Assert.AreEqual(mixedValue.ToString(), $"{value}:{flag}");
        }

        #endregion

        #region bool

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void Equal_bool(bool value, bool flag)
        {
            var mixedValue1 = new MixedValue<bool>(value, flag);
            var mixedValue2 = new MixedValue<bool>(value, flag);

            Assert.True(mixedValue1.Equals(mixedValue2));
            Assert.AreEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [Test]
        public void NotEqual_bool_DifferentValues()
        {
            var mixedValue1 = new MixedValue<bool>(true, true);
            var mixedValue2 = new MixedValue<bool>(false, true);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [Test]
        public void NotEqual_bool_DifferentHasMultipleDifferentValues()
        {
            var mixedValue1 = new MixedValue<bool>(true, true);
            var mixedValue2 = new MixedValue<bool>(true, false);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void ToString_bool(bool value, bool flag)
        {
            var mixedValue = new MixedValue<bool>(value, flag);
            Assert.AreEqual(mixedValue.ToString(), $"{value}:{flag}");
        }

        #endregion

        #region string

        [TestCase("abc", true)]
        [TestCase("def", false)]
        public void Equal_string(string value, bool flag)
        {
            var mixedValue1 = new MixedValue<string>(value, flag);
            var mixedValue2 = new MixedValue<string>(value, flag);

            Assert.True(mixedValue1.Equals(mixedValue2));
            Assert.AreEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase("abc", "bcd")]
        [TestCase("", null)]
        public void NotEqual_string_DifferentValues(string value1, string value2)
        {
            var mixedValue1 = new MixedValue<string>(value1, true);
            var mixedValue2 = new MixedValue<string>(value2, true);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase("a")]
        [TestCase("xyz")]
        public void NotEqual_string_DifferentHasMultipleDifferentValues(string value)
        {
            var mixedValue1 = new MixedValue<string>(value, true);
            var mixedValue2 = new MixedValue<string>(value, false);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase("abc", true)]
        [TestCase("def", false)]
        public void ToString_string(string value, bool flag)
        {
            var mixedValue = new MixedValue<string>(value, flag);
            Assert.AreEqual(mixedValue.ToString(), $"{value}:{flag}");
        }

        #endregion

        #region enum

        [TestCase(TestEnum.One, true)]
        [TestCase(TestEnum.Two, false)]
        public void Equal_enum(TestEnum value, bool flag)
        {
            var mixedValue1 = new MixedValue<TestEnum>(value, flag);
            var mixedValue2 = new MixedValue<TestEnum>(value, flag);

            Assert.True(mixedValue1.Equals(mixedValue2));
            Assert.AreEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(TestEnum.One, TestEnum.Two)]
        [TestCase(TestEnum.Three, TestEnum.One)]
        public void NotEqual_enum_DifferentValues(TestEnum value1, TestEnum value2)
        {
            var mixedValue1 = new MixedValue<TestEnum>(value1, true);
            var mixedValue2 = new MixedValue<TestEnum>(value2, true);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(TestEnum.One)]
        [TestCase(TestEnum.Three)]
        public void NotEqual_enum_DifferentHasMultipleDifferentValues(TestEnum value)
        {
            var mixedValue1 = new MixedValue<TestEnum>(value, true);
            var mixedValue2 = new MixedValue<TestEnum>(value, false);

            Assert.False(mixedValue1.Equals(mixedValue2));
            Assert.AreNotEqual(mixedValue1.GetHashCode(), mixedValue2.GetHashCode());
        }

        [TestCase(TestEnum.One, true)]
        [TestCase(TestEnum.Two, false)]
        public void ToString_enum(TestEnum value, bool flag)
        {
            var mixedValue = new MixedValue<TestEnum>(value, flag);
            Assert.AreEqual(mixedValue.ToString(), $"{value}:{flag}");
        }

        #endregion
    }
}
