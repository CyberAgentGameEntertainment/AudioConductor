// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Shared;
using UnityEngine;

namespace AudioConductor.Editor.Core.Models
{
    [Serializable]
    internal sealed class ColorDefine
    {
        [SerializeField] internal string id = IdentifierFactory.Create();

        public string name = null!;
        public Color color = Color.white;
        internal string Id => id;
    }
}
