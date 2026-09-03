// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    internal sealed partial class InspectorView : VisualElement, IDisposable
#else
    internal sealed class InspectorView : VisualElement, IDisposable
#endif
    {
        public InspectorView()
        {
            var tree = AssetLoader.LoadUxml("Inspector");
            tree.CloneTree(this);
        }

        public void Dispose()
        {
            // nothing
        }

        internal void Setup()
        {
            // nothing
        }

        #region Uxml

#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
#endif

        #endregion
    }
}
