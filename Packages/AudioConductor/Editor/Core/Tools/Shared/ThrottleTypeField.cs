// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    internal sealed partial class ThrottleTypeField : EnumField
#else
    internal sealed class ThrottleTypeField : EnumField
#endif
    {
        public ThrottleTypeField()
        {
            Init(ThrottleType.PriorityOrder, false);
            label = "Throttle Type";
        }

#if !UNITY_2023_2_OR_NEWER
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
#endif
    }
}
