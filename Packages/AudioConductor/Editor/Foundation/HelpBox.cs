// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

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
