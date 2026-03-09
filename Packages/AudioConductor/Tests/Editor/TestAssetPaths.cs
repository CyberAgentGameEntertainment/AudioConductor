// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tests
{
    internal static class TestAssetPaths
    {
        private static string? _folder;

        private static string AssemblyName { get; } =
            Path.GetFileNameWithoutExtension(typeof(TestAssetPaths).Assembly.Location);

        public static string Folder
        {
            get
            {
                if (!string.IsNullOrEmpty(_folder))
                    return _folder!;

                var asmdefGuid = AssetDatabase.FindAssets(AssemblyName).First();
                var asmdefPath = AssetDatabase.GUIDToAssetPath(asmdefGuid);
                var asmdefFolderPath = asmdefPath[..asmdefPath.LastIndexOf("/", StringComparison.Ordinal)];
                var baseFolderPath = $"{asmdefFolderPath}/TestAssets";
                return baseFolderPath;
            }
        }

        public static string CreateAbsoluteAssetPath(string relativeAssetPath)
        {
            return $"{Folder}/{relativeAssetPath}";
        }
    }
}
