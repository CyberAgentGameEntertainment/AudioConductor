// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.CodeGen.Tests
{
    internal class IdentifierConverterTests
    {
        [TestCase("battle_bgm", "BattleBgm")]
        [TestCase("BGM - Title", "BGMTitle")]
        [TestCase("SE_01", "SE01")]
        [TestCase("hello_world", "HelloWorld")]
        [TestCase("hello-world", "HelloWorld")]
        [TestCase("hello world", "HelloWorld")]
        [TestCase("hello__world", "HelloWorld")]
        [TestCase("hello--world", "HelloWorld")]
        [TestCase("a_b_c", "ABC")]
        [TestCase("game_over", "GameOver")]
        public void ToPascalCase_WordSeparators_ConvertCorrectly(string input, string expected)
        {
            Assert.That(IdentifierConverter.ToPascalCase(input), Is.EqualTo(expected));
        }

        [TestCase("BGM", "BGM")]
        [TestCase("SE", "SE")]
        [TestCase("BGMTitle", "BGMTitle")]
        [TestCase("SEAttack", "SEAttack")]
        public void ToPascalCase_AllUppercaseAcronyms_Preserved(string input, string expected)
        {
            Assert.That(IdentifierConverter.ToPascalCase(input), Is.EqualTo(expected));
        }

        [TestCase("1shot", "_1Shot")]
        [TestCase("01_bgm", "_01Bgm")]
        public void ToPascalCase_LeadingDigit_PrefixedWithUnderscore(string input, string expected)
        {
            Assert.That(IdentifierConverter.ToPascalCase(input), Is.EqualTo(expected));
        }

        [TestCase("int", "@int")]
        [TestCase("class", "@class")]
        [TestCase("namespace", "@namespace")]
        [TestCase("return", "@return")]
        [TestCase("void", "@void")]
        [TestCase("record", "@record")]
        [TestCase("init", "@init")]
        [TestCase("required", "@required")]
        [TestCase("async", "@async")]
        [TestCase("await", "@await")]
        [TestCase("var", "@var")]
        [TestCase("dynamic", "@dynamic")]
        [TestCase("yield", "@yield")]
        [TestCase("when", "@when")]
        [TestCase("with", "@with")]
        public void ToPascalCase_CSharpKeyword_PrefixedWithAt(string input, string expected)
        {
            Assert.That(IdentifierConverter.ToPascalCase(input), Is.EqualTo(expected));
        }

        [Test]
        public void ToPascalCase_EmptyString_ReturnsEmpty()
        {
            Assert.That(IdentifierConverter.ToPascalCase(string.Empty), Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToPascalCase_OnlySymbols_ReturnsEmpty()
        {
            Assert.That(IdentifierConverter.ToPascalCase("---"), Is.EqualTo(string.Empty));
        }

        [TestCase("HelloWorld", true)]
        [TestCase("_private", true)]
        [TestCase("@class", true)]
        [TestCase("abc123", true)]
        [TestCase("", false)]
        [TestCase("123abc", false)]
        [TestCase("hello world", false)]
        [TestCase("hello-world", false)]
        public void IsValidIdentifier_VariousInputs_ReturnsExpected(string input, bool expected)
        {
            Assert.That(IdentifierConverter.IsValidIdentifier(input), Is.EqualTo(expected));
        }
    }
}
