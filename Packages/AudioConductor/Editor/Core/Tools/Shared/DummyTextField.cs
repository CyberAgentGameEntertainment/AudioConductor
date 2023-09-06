// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

#if UNITY_2022_1_OR_NEWER

using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal sealed class DummyTextField : TextField
    {
        public new static string mixedValueString => TextField.mixedValueString;
    }
}

#endif
