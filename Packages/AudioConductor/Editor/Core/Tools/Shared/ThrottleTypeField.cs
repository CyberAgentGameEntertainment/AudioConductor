// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core.Enums;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class ThrottleTypeField : EnumField
    {
        public new class UxmlFactory : UxmlFactory<ThrottleTypeField, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((EnumField)ve).Init(ThrottleType.PriorityOrder, false);
                ((EnumField)ve).label = "Throttle Type";
            }
        }
    }
}
