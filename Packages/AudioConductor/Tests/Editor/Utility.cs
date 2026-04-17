// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Linq;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tests
{
    internal static class Utility
    {
        public static string RandomString => Guid.NewGuid().ToString();

        /// <summary>
        ///     Creates folders recursively via <see cref="AssetDatabase.CreateFolder" />.
        /// </summary>
        /// <param name="path">An asset path starting with "Assets/" (e.g. "Assets/gen/MyTests").</param>
        public static void CreateFolderRecursively(string path)
        {
            if (!path.StartsWith("Assets/"))
                return;

            var dirs = path.Split('/');
            var combinePath = dirs[0];
            foreach (var dir in dirs.Skip(1))
            {
                if (!AssetDatabase.IsValidFolder(combinePath + '/' + dir))
                    AssetDatabase.CreateFolder(combinePath, dir);
                combinePath += '/' + dir;
            }
        }
    }
}
