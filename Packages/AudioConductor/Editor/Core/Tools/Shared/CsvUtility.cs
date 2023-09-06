// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Text;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class CsvUtility
    {
        private const char Delimiter = ',';

        public static void AppendDelimiter(StringBuilder builder, int num)
        {
            builder.Append(new string(Delimiter, num));
        }

        public static void AppendRowItems(StringBuilder builder, params object[] items)
        {
            for (var i = 0; i < items.Length; ++i)
            {
                if (i != 0)
                    builder.Append(Delimiter);

                builder.Append(items[i]);
            }
        }

        public static void AppendRowItemsLine(StringBuilder builder, int maxRowNum, params object[] items)
        {
            AppendRowItems(builder, items);
            AppendDelimiter(builder, maxRowNum - items.Length);
            builder.AppendLine();
        }
    }
}
