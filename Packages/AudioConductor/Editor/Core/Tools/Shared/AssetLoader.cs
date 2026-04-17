// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEditor;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class AssetLoader
    {
        private const string AssetPath = "Packages/jp.co.cyberagent.audioconductor/Editor/PackageResources";

        public static VisualTreeAsset LoadUxml(string name)
        {
            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{AssetPath}/Uxml/{name}.uxml");
        }

        public static StyleSheet LoadUss(string name)
        {
            return AssetDatabase.LoadAssetAtPath<StyleSheet>($"{AssetPath}/Uss/{name}.uss");
        }
    }
}
