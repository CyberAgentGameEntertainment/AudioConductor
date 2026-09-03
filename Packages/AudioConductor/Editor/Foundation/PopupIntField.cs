// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Foundation
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    public partial class PopupIntField : PopupField<int>
    {
    }
#else
    public class PopupIntField : PopupField<int>
    {
        public new class UxmlFactory : UxmlFactory<PopupIntField, UxmlTraits>
        {
        }
    }
#endif
}
