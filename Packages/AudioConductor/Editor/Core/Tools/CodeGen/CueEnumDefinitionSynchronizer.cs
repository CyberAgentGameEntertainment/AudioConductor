// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Linq;
using System.Text.RegularExpressions;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     Keeps the <see cref="CueEnumDefinition" /> in sync when CueSheetAssets are created, deleted, or moved.
    /// </summary>
    internal sealed class CueEnumDefinitionSynchronizer : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var definition = CueEnumDefinitionRepository.instance.Definition;
            if (definition == null)
                return;

            var dirty = false;

            dirty |= HandleDeletedAssets(definition, deletedAssets);
            dirty |= HandleImportedAssets(definition, importedAssets);
            dirty |= HandleMovedAssets(definition, movedAssets);

            if (dirty)
            {
                EditorUtility.SetDirty(definition);
                AssetDatabase.SaveAssets();
            }
        }

        private static bool HandleDeletedAssets(CueEnumDefinition definition, string[] deletedAssets)
        {
            if (deletedAssets == null || deletedAssets.Length == 0)
                return false;

            var dirty = false;

            // Clean up null references from rootEntries
            var rootBefore = definition.rootEntries.Count;
            definition.rootEntries.RemoveAll(a => a == null);
            if (definition.rootEntries.Count != rootBefore)
                dirty = true;

            // Clean up null references from fileEntries
            foreach (var fe in definition.fileEntries)
            {
                var before = fe.assets.Count;
                fe.assets.RemoveAll(a => a == null);
                if (fe.assets.Count != before)
                    dirty = true;
            }

            return dirty;
        }

        private static bool HandleImportedAssets(CueEnumDefinition definition, string[] importedAssets)
        {
            if (importedAssets == null || importedAssets.Length == 0)
                return false;

            var dirty = false;

            foreach (var path in importedAssets)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                if (asset == null)
                    continue;

                if (IsContainedInDefinition(definition, asset))
                    continue;

                // New CueSheetAsset not in any definition → add to rootEntries
                definition.rootEntries.Add(asset);
                dirty = true;
            }

            return dirty;
        }

        private static bool HandleMovedAssets(CueEnumDefinition definition, string[] movedAssets)
        {
            if (movedAssets == null || movedAssets.Length == 0)
                return false;

            var dirty = false;

            foreach (var path in movedAssets)
            {
                var asset = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(path);
                if (asset == null)
                    continue;

                // Re-evaluate path rules for fileEntries
                foreach (var fe in definition.fileEntries)
                {
                    if (string.IsNullOrEmpty(fe.pathRule))
                        continue;

                    // If asset is manually placed in this file entry, skip
                    if (fe.assets.Contains(asset))
                        continue;

                    if (!MatchesPathRule(path, fe.pathRule))
                        continue;

                    // Move from rootEntries to this fileEntry
                    if (definition.rootEntries.Remove(asset))
                    {
                        fe.assets.Add(asset);
                        dirty = true;
                    }
                }
            }

            return dirty;
        }

        internal static bool IsContainedInDefinition(CueEnumDefinition definition, CueSheetAsset asset)
        {
            if (definition.rootEntries.Contains(asset))
                return true;

            return definition.fileEntries.Any(fe => fe.assets.Contains(asset));
        }

        internal static bool MatchesPathRule(string assetPath, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            // Simple glob matching: support *, ** and ? patterns
            // **/ → zero or more directory segments (including none)
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*\\*/", "(.+/)?")
                .Replace("\\*\\*", ".*")
                .Replace("\\*", "[^/]*")
                .Replace("\\?", "[^/]") + "$";

            return Regex.IsMatch(assetPath, regexPattern);
        }
    }
}
