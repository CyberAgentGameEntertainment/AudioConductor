// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed partial class CueListTreeView
    {
        private static readonly int[] DefaultVisibleColumns = { 0, 8, 1, 2, 3, 4, 5, 6, 7 };

        private static int ExpectedColumnCount => Enum.GetValues(typeof(ColumnType)).Length;

        [Serializable]
        public sealed class State : TreeViewState
        {
            [SerializeField] private MultiColumnHeaderState? _multiColumnHeaderState;

            public MultiColumnHeaderState MultiColumnHeaderState
            {
                get
                {
                    if (_multiColumnHeaderState == null)
                        _multiColumnHeaderState = CreateMultiColumnHeaderState();
                    else if (_multiColumnHeaderState.columns.Length < ExpectedColumnCount)
                        _multiColumnHeaderState = MigrateState(_multiColumnHeaderState);

                    return _multiColumnHeaderState;
                }
            }

            private static MultiColumnHeaderState CreateMultiColumnHeaderState()
            {
                var columns = Enum.GetValues(typeof(ColumnType))
                    .OfType<ColumnType>()
                    .Select(columnType => columnType.CreateColumn())
                    .ToArray();
                return new MultiColumnHeaderState(columns)
                {
                    visibleColumns = DefaultVisibleColumns
                };
            }

            private static MultiColumnHeaderState MigrateState(MultiColumnHeaderState oldState)
            {
                var newState = CreateMultiColumnHeaderState();

                // Copy old column widths to matching indices.
                for (var i = 0; i < oldState.columns.Length; i++)
                    newState.columns[i].width = oldState.columns[i].width;

                // Insert CueId(8) right after Name(0) in visible columns.
                var oldVisible = oldState.visibleColumns;
                var newVisible = new List<int>(oldVisible.Length + 1);
                foreach (var col in oldVisible)
                {
                    newVisible.Add(col);
                    if (col == (int)ColumnType.Name)
                        newVisible.Add((int)ColumnType.CueId);
                }

                newState.visibleColumns = newVisible.ToArray();
                return newState;
            }
        }
    }
}
