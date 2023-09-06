// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueTrackInspectorView : VisualElement, IDisposable
    {
        public CueTrackInspectorView()
        {
            var tree = AssetLoader.LoadUxml("CueTrackInspector");
            tree.CloneTree(this);
        }

        public void Dispose()
        {
        }

        internal void Open()
        {
            this.SetDisplay(true);
        }

        internal void Close()
        {
            this.SetDisplay(false);
        }

        #region Uxml

        public new class UxmlFactory : UxmlFactory<CueTrackInspectorView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
