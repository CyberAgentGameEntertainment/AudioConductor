// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class TwoPaneSplitView : UnityEngine.UIElements.TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<TwoPaneSplitView, UxmlTraits>
        {
        }
    }
}
