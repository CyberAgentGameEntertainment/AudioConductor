// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal sealed partial class CueEnumDefinitionTreeView
    {
        internal static MultiColumnHeaderState.Column[] CreateColumns()
        {
            return new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    width = 200,
                    minWidth = 100,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Output File"),
                    width = 200,
                    minWidth = 80,
                    autoResize = true
                }
            };
        }

        internal enum ColumnType
        {
            Name,
            OutputFile
        }
    }
}
