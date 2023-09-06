// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using UnityEngine;

namespace AudioConductor.Editor.Core.Models
{
    [CreateAssetMenu(fileName = nameof(AudioConductorEditorSettings),
                     menuName = "Audio Conductor/" + "EditorSettings",
                     order = 1)]
    internal sealed class AudioConductorEditorSettings : ScriptableObject
    {
        /// <summary>
        ///     List of <see cref="ColorDefine" />.
        /// </summary>
        public List<ColorDefine> colorDefineList = new();
    }
}
