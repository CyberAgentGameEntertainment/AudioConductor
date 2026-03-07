// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Foundation
{
    public class PopupIntField : PopupField<int>
    {
        public new class UxmlFactory : UxmlFactory<PopupIntField, UxmlTraits>
        {
        }
    }
}
