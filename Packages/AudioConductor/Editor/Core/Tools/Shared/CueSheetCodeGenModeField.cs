// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class CueSheetCodeGenModeField : EnumField
    {
        public new class UxmlFactory : UxmlFactory<CueSheetCodeGenModeField, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                ((EnumField)ve).Init(CueSheetCodeGenMode.Manual, false);
                ((EnumField)ve).label = "Code Gen Mode";
            }
        }
    }
}
