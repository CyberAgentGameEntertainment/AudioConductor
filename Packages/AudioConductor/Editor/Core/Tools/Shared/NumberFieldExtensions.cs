// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

#if UNITY_2022_1_OR_NEWER
using System.Reflection;
#endif
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class NumberFieldExtensions
    {
        public static void ForceSetValueWithoutNotify(this IntegerField field, int value)
        {
            // NOTE: Unity 2022.1 or newer
            // When changing from multiple selection to single selection, there is a pattern where the display remains a mixed string.
#if UNITY_2022_1_OR_NEWER
            var type = field.GetType().BaseType;
            var fieldInfo = type?.GetField("m_ForceUpdateDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo?.SetValue(field, true);
#endif
            field.SetValueWithoutNotify(value);
        }
        
        public static void ForceSetValueWithoutNotify(this FloatField field, float value)
        {
            // NOTE: Unity 2022.1 or newer
            // When changing from multiple selection to single selection, there is a pattern where the display remains a mixed string.
#if UNITY_2022_1_OR_NEWER
            var type = field.GetType().BaseType;
            var fieldInfo = type?.GetField("m_ForceUpdateDisplay", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo?.SetValue(field, true);
#endif
            field.SetValueWithoutNotify(value);
        }
    }
}
