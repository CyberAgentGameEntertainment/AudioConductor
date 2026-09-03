// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    internal sealed partial class TwoPaneSplitView : UnityEngine.UIElements.TwoPaneSplitView
    {
    }
#else
    internal sealed class TwoPaneSplitView : UnityEngine.UIElements.TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<TwoPaneSplitView, UxmlTraits>
        {
        }
    }
#endif
}
