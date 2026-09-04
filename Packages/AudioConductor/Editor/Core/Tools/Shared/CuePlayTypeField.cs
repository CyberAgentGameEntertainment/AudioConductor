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
    internal sealed partial class CuePlayTypeField : EnumField
#else
    internal sealed class CuePlayTypeField : EnumField
#endif
    {
        public CuePlayTypeField()
        {
            Init(CuePlayType.Sequential, false);
            label = "Play Type";
        }

#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<CuePlayTypeField, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((EnumField)ve).Init(CuePlayType.Sequential, false);
                ((EnumField)ve).label = "Play Type";
            }
        }
#endif
    }
}
