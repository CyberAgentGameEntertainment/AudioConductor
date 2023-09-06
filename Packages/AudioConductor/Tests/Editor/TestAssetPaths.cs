// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using UnityEditor;

namespace AudioConductor.Tests.Editor
{
    internal static class TestAssetPaths
    {
        private static string _folder;

        private static string AssemblyName { get; } =
            Path.GetFileNameWithoutExtension(typeof(TestAssetPaths).Assembly.Location);

        public static string Folder
        {
            get
            {
                if (!string.IsNullOrEmpty(_folder))
                    return _folder;

                var asmdefGuid = AssetDatabase.FindAssets(AssemblyName).First();
                var asmdefPath = AssetDatabase.GUIDToAssetPath(asmdefGuid);
                var asmdefFolderPath = asmdefPath[..asmdefPath.LastIndexOf("/", StringComparison.Ordinal)];
                var baseFolderPath = $"{asmdefFolderPath}/TestAssets";
                return baseFolderPath;
            }
        }

        public static string CreateAbsoluteAssetPath(string relativeAssetPath) => $"{Folder}/{relativeAssetPath}";
    }
}
