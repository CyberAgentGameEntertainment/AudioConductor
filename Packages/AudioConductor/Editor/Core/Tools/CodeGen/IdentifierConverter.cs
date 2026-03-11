// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Text;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class IdentifierConverter
    {
        private static readonly HashSet<string> CSharpKeywords = new()
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using",
            "virtual", "void", "volatile", "while"
        };

        /// <summary>
        ///     Converts the input string to PascalCase, preserving all-uppercase acronyms.
        ///     Spaces, underscores, hyphens, and other non-alphanumeric characters act as word separators.
        ///     A leading digit is prefixed with '_'. C# reserved keywords are prefixed with '@'.
        /// </summary>
        internal static string ToPascalCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input ?? string.Empty;

            // Single token that is a C# keyword: prefix with '@' and return as-is (lowercase)
            var trimmed = input.Trim();
            if (CSharpKeywords.Contains(trimmed))
                return "@" + trimmed;

            var sb = new StringBuilder();
            var i = 0;

            while (i < input.Length)
            {
                // Skip separators
                while (i < input.Length && !char.IsLetterOrDigit(input[i]))
                    i++;
                if (i >= input.Length)
                    break;

                // Extract one explicit token (contiguous alphanumeric run)
                var tokenStart = i;
                while (i < input.Length && char.IsLetterOrDigit(input[i]))
                    i++;
                var token = input.Substring(tokenStart, i - tokenStart);

                // Split CamelCase/acronym boundaries within the token and process each sub-word
                AppendToken(sb, token);
            }

            var result = sb.ToString();
            if (string.IsNullOrEmpty(result))
                return result;

            // Prefix leading digit with '_'
            if (char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }

        private static void AppendToken(StringBuilder sb, string token)
        {
            // Split the token into sub-words based on case transitions and digit runs
            var j = 0;
            while (j < token.Length)
                if (char.IsDigit(token[j]))
                {
                    // Digit run: append as-is
                    var start = j;
                    while (j < token.Length && char.IsDigit(token[j]))
                        j++;
                    sb.Append(token, start, j - start);
                }
                else if (char.IsUpper(token[j]))
                {
                    // Uppercase run
                    var runStart = j;
                    while (j < token.Length && char.IsUpper(token[j]))
                        j++;
                    var runLen = j - runStart;

                    if (runLen == 1)
                    {
                        // Single uppercase letter: start of a PascalCase word
                        // Consume following lowercase letters
                        sb.Append(token[runStart]);
                        while (j < token.Length && char.IsLower(token[j]))
                        {
                            sb.Append(token[j]);
                            j++;
                        }
                    }
                    else
                    {
                        // Multiple uppercase letters = acronym
                        // If followed by lowercase, last uppercase is start of next PascalCase word
                        if (j < token.Length && char.IsLower(token[j]))
                        {
                            // Emit acronym (all but last uppercase)
                            sb.Append(token, runStart, runLen - 1);
                            // Last uppercase + following lowercase = next word
                            sb.Append(token[j - 1]);
                            while (j < token.Length && char.IsLower(token[j]))
                            {
                                sb.Append(token[j]);
                                j++;
                            }
                        }
                        else
                        {
                            // Acronym at end or followed by digit/separator
                            sb.Append(token, runStart, runLen);
                        }
                    }
                }
                else
                {
                    // Lowercase run: capitalize first letter, lowercase rest
                    sb.Append(char.ToUpperInvariant(token[j]));
                    j++;
                    while (j < token.Length && char.IsLower(token[j]))
                    {
                        sb.Append(token[j]);
                        j++;
                    }
                }
        }

        /// <summary>
        ///     Returns true if the input is a valid C# identifier (letters, digits, underscore; not starting with digit).
        /// </summary>
        internal static bool IsValidIdentifier(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Allow verbatim identifiers
            var s = input.StartsWith("@") ? input.Substring(1) : input;
            if (string.IsNullOrEmpty(s))
                return false;

            if (!char.IsLetter(s[0]) && s[0] != '_')
                return false;

            for (var i = 1; i < s.Length; i++)
                if (!char.IsLetterOrDigit(s[i]) && s[i] != '_')
                    return false;

            return true;
        }
    }
}
