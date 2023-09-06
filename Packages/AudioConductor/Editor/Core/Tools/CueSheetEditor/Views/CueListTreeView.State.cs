// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed partial class CueListTreeView
    {
        [Serializable]
        public sealed class State : TreeViewState
        {
            [SerializeField]
            private MultiColumnHeaderState _multiColumnHeaderState;

            public MultiColumnHeaderState MultiColumnHeaderState =>
                _multiColumnHeaderState ??= CreateMultiColumnHeaderState();

            private MultiColumnHeaderState CreateMultiColumnHeaderState()
            {
                var columns = Enum.GetValues(typeof(ColumnType))
                                  .OfType<ColumnType>()
                                  .Select(columnType => columnType.CreateColumn())
                                  .ToArray();
                return new MultiColumnHeaderState(columns);
            }
        }
    }
}
