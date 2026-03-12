// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal static class CueEnumCodeWriter
    {
        /// <summary>
        ///     Writes source code to the specified path atomically.
        ///     Skips writing if the existing file content is identical.
        ///     Does NOT call AssetDatabase.Refresh.
        /// </summary>
        /// <returns>true if the file was written; false if the content was already up to date.</returns>
        internal static bool Write(string outputPath, string sourceCode)
        {
            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == sourceCode)
                return false;

            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            WriteAllTextAtomically(outputPath, sourceCode);
            return true;
        }

        private static void WriteAllTextAtomically(string outputPath, string sourceCode)
        {
            var directory = Path.GetDirectoryName(outputPath);
            var tempPath = Path.Combine(directory ?? ".", Path.GetRandomFileName() + ".tmp");

            try
            {
                File.WriteAllText(tempPath, sourceCode);
                if (File.Exists(outputPath))
                    File.Replace(tempPath, outputPath, null);
                else
                    File.Move(tempPath, outputPath);
            }
            finally
            {
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
    }
}
