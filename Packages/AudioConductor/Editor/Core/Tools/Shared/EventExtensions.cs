// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class EventExtensions
    {
        internal static bool GetEventAction(Event e)
        {
#if UNITY_EDITOR_WIN
            return e.control;
#else
            return e.command;
#endif
        }

        internal static bool GetEventAction(IKeyboardEvent e)
        {
#if UNITY_EDITOR_WIN
            return e.ctrlKey;
#else
            return e.commandKey;
#endif
        }
    }
}
