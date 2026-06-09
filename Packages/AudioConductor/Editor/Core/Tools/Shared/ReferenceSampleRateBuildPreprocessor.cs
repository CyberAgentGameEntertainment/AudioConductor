// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class ReferenceSampleRateBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 1;

        public void OnPreprocessBuild(BuildReport report)
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(CueSheetAsset), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                if (asset == null || asset.cueSheet.referenceSampleRate != 0)
                    continue;

                if (HasAnyClip(asset.cueSheet))
                    Debug.LogWarning(
                        $"[AudioConductor] CueSheet '{asset.name}' has no referenceSampleRate set. Sample positions may drift on platforms with different audio decoding frequencies (e.g. WebGL).");
            }
        }

        private static bool HasAnyClip(CueSheet cueSheet)
        {
            foreach (var cue in cueSheet.cueList)
            foreach (var track in cue.trackList)
                if (track.audioClip != null)
                    return true;
            return false;
        }
    }
}
