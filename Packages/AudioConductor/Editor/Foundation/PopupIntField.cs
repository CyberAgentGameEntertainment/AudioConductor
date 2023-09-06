// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEditor.UIElements;
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
