// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_6000_2_OR_NEWER
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;
#else
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState;
#endif
using System;
using UnityEditor.IMGUI.Controls;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal sealed partial class CueEnumDefinitionTreeView
    {
        [Serializable]
        internal sealed class State : TreeViewState
        {
            private MultiColumnHeaderState? _multiColumnHeaderState;

            internal MultiColumnHeaderState MultiColumnHeaderState
            {
                get
                {
                    _multiColumnHeaderState ??= CreateMultiColumnHeaderState();
                    return _multiColumnHeaderState;
                }
            }

            private static MultiColumnHeaderState CreateMultiColumnHeaderState()
            {
                return new MultiColumnHeaderState(CreateColumns());
            }
        }
    }
}
