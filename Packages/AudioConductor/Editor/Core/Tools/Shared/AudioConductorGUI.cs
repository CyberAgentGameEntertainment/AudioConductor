// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class AudioConductorGUI
    {
        internal static class ColorDefine
        {
            private static readonly int PopupHash = nameof(ColorSelectPopupWindowContent).GetHashCode();
            private static CallbackInfo CallbackInfoInstance { get; set; }

            public static string Popup(Rect rect, string colorId, bool showMixedValue = false)
            {
                var controlId = GUIUtility.GetControlID(PopupHash, FocusType.Passive, rect);

                var oldIndex = ColorDefineListRepository.instance.ToIndex(colorId);
                var contents = ColorDefineListRepository.instance.ColorDefineContents;

                var selectedIndex = GetSelectedValueForControl(controlId, oldIndex);
                EditorGUI.showMixedValue = showMixedValue;
                var push = EditorGUI.DropdownButton(rect, contents[oldIndex], FocusType.Passive);
                EditorGUI.showMixedValue = false;
                if (push)
                {
                    CallbackInfoInstance = new CallbackInfo(controlId);
                    PopupWindow.Show(rect,
                                     new ColorSelectPopupWindowContent(rect, oldIndex, CallbackInfoInstance.SetValue));
                }

                return ColorDefineListRepository.instance.ToColorId(selectedIndex);
            }

            private static int GetSelectedValueForControl(int controlID, int selected)
            {
                if (CallbackInfoInstance == null)
                    return selected;

                if (CallbackInfoInstance.ControlId != controlID)
                    return selected;

                if (CallbackInfoInstance.Value.HasValue == false)
                    return selected;

                GUI.changed = selected != CallbackInfoInstance.Value;
                selected = CallbackInfoInstance.Value.Value;
                CallbackInfoInstance = null;

                return selected;
            }

            private sealed class CallbackInfo
            {
                public static readonly string EventName =
                    $"{nameof(AudioConductorGUI)}.{nameof(ColorDefine)}.{nameof(CallbackInfo)}.{nameof(SetValue)}";

                public CallbackInfo(int controlId)
                {
                    ControlId = controlId;
                }

                public int ControlId { get; }
                public int? Value { get; private set; }

                public void SetValue(int value)
                {
                    Value = value;
                }
            }
        }
    }
}
