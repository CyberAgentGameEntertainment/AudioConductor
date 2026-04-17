// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class VisualElementExtensions
    {
        public static void SetDisplay(this VisualElement? element, bool active)
        {
            if (element == null)
                return;

            element.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public static void SetVisible(this VisualElement? element, bool visible)
        {
            if (element == null)
                return;

            element.visible = visible;
        }
    }
}
