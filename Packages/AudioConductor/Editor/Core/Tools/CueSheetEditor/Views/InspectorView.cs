// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class InspectorView : VisualElement, IDisposable
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

        public new class UxmlFactory : UxmlFactory<InspectorView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
