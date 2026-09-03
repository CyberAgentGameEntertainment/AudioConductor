// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Foundation
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    public partial class HelpBox : UnityEngine.UIElements.HelpBox
    {
    }
#else
    public class HelpBox : UnityEngine.UIElements.HelpBox
    {
        public new class UxmlFactory : UxmlFactory<HelpBox, UxmlTraits>
        {
        }
    }
#endif
}
