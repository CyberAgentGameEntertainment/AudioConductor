// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Text;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal class CsvUtilityTests
    {
        // --- AppendDelimiter ---

        [Test]
        public void AppendDelimiter_ZeroNum_AppendsNothing()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendDelimiter(builder, 0);

            Assert.That(builder.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void AppendDelimiter_OneNum_AppendsSingleComma()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendDelimiter(builder, 1);

            Assert.That(builder.ToString(), Is.EqualTo(","));
        }

        [Test]
        public void AppendDelimiter_ThreeNum_AppendsThreeCommas()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendDelimiter(builder, 3);

            Assert.That(builder.ToString(), Is.EqualTo(",,,"));
        }

        [Test]
        public void AppendDelimiter_ExistingContent_AppendsAfterExisting()
        {
            var builder = new StringBuilder("abc");

            CsvUtility.AppendDelimiter(builder, 2);

            Assert.That(builder.ToString(), Is.EqualTo("abc,,"));
        }

        // --- AppendRowItems ---

        [Test]
        public void AppendRowItems_EmptyItems_AppendsNothing()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItems(builder);

            Assert.That(builder.ToString(), Is.EqualTo(string.Empty));
        }

        [Test]
        public void AppendRowItems_SingleItem_AppendsWithoutDelimiter()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItems(builder, "x");

            Assert.That(builder.ToString(), Is.EqualTo("x"));
        }

        [Test]
        public void AppendRowItems_MultipleItems_AppendsWithDelimiters()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItems(builder, "a", "b", "c");

            Assert.That(builder.ToString(), Is.EqualTo("a,b,c"));
        }

        [Test]
        public void AppendRowItems_NullItem_AppendsEmptyPosition()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItems(builder, "a", null, "c");

            Assert.That(builder.ToString(), Is.EqualTo("a,,c"));
        }

        // --- AppendRowItemsLine ---

        [Test]
        public void AppendRowItemsLine_ItemsEqualMaxRowNum_AppendsLineOnly()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItemsLine(builder, 3, "a", "b", "c");

            Assert.That(builder.ToString(), Is.EqualTo("a,b,c\n"));
        }

        [Test]
        public void AppendRowItemsLine_ItemsLessThanMaxRowNum_AppendsPaddingAndLine()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItemsLine(builder, 5, "a", "b", "c");

            Assert.That(builder.ToString(), Is.EqualTo("a,b,c,,\n"));
        }

        [Test]
        public void AppendRowItemsLine_EmptyItems_AppendsDelimitersAndLine()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItemsLine(builder, 3);

            Assert.That(builder.ToString(), Is.EqualTo(",,,\n"));
        }

        [Test]
        public void AppendRowItemsLine_MultipleCallsAppendMultipleLines()
        {
            var builder = new StringBuilder();

            CsvUtility.AppendRowItemsLine(builder, 2, "a", "b");
            CsvUtility.AppendRowItemsLine(builder, 2, "c", "d");

            Assert.That(builder.ToString(), Is.EqualTo("a,b\nc,d\n"));
        }
    }
}
