// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Foundation
{
    public class HelpBox : UnityEngine.UIElements.HelpBox
    {
        public new class UxmlFactory : UxmlFactory<HelpBox, UxmlTraits>
        {
        }
    }
}
