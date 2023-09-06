// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Editor.Core.Models
{
    [Serializable]
    internal sealed class ColorDefine
    {
        [SerializeField]
        internal string id = IdentifierFactory.Create();

        public string name;
        public Color color = Color.white;
        internal string Id => id;
    }
}
