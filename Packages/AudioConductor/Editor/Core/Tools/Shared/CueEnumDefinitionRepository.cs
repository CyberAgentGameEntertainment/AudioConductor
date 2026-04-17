// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.IO;
using System.Linq;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    /// <summary>
    ///     Holds a reference to the <see cref="CueEnumDefinition" /> asset.
    ///     Auto-creates the definition when none exists.
    /// </summary>
    internal sealed class CueEnumDefinitionRepository : ScriptableSingleton<CueEnumDefinitionRepository>
    {
        private const string DefaultAssetPath = "Assets/AudioConductor/Editor/CueEnumDefinition.asset";

        [SerializeField] private CueEnumDefinition? definition;

        internal CueEnumDefinition? Definition
        {
            get
            {
                if (definition == null)
                    definition = FindExistingDefinition();

                return definition;
            }
        }

        /// <summary>
        ///     Gets or creates the definition asset.
        ///     When creating (or recovering from deletion), collects all existing CueSheetAssets into rootEntries.
        /// </summary>
        internal CueEnumDefinition GetOrCreate()
        {
            if (definition != null)
                return definition;

            definition = FindExistingDefinition();
            if (definition != null)
                return definition;

            definition = CreateDefinition();
            return definition;
        }

        internal void SetDefinition(CueEnumDefinition? newDefinition)
        {
            definition = newDefinition;
        }

        private static CueEnumDefinition? FindExistingDefinition()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(CueEnumDefinition), new[] { "Assets" });
            if (guids == null || guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<CueEnumDefinition>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static CueEnumDefinition CreateDefinition()
        {
            var def = CreateInstance<CueEnumDefinition>();

            // Collect all existing CueSheetAssets
            var cueSheetAssets = AssetDatabase.FindAssets("t:" + nameof(CueSheetAsset), new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(path => path)
                .Select(AssetDatabase.LoadAssetAtPath<CueSheetAsset>)
                .Where(asset => asset != null)
                .ToList();
            def.rootEntries.AddRange(cueSheetAssets!);

            var dir = Path.GetDirectoryName(DefaultAssetPath);
            if (!string.IsNullOrEmpty(dir) && !AssetDatabase.IsValidFolder(dir))
            {
                var parts = dir.Split('/');
                var current = parts[0];
                for (var i = 1; i < parts.Length; i++)
                {
                    var next = current + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, parts[i]);
                    current = next;
                }
            }

            AssetDatabase.CreateAsset(def, DefaultAssetPath);
            AssetDatabase.SaveAssets();

            return def;
        }
    }
}
