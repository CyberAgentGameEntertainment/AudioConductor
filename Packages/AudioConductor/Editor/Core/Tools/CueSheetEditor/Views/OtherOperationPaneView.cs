// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class OtherOperationPaneView : VisualElement, IDisposable
    {
        private readonly Button _exportButton;
        private readonly Subject<Empty> _exportClickedSubject = new();

        private readonly Button _importButton;
        private readonly Subject<Empty> _importClickedSubject = new();

        public OtherOperationPaneView()
        {
            var tree = AssetLoader.LoadUxml("OtherOperationPane");
            tree.CloneTree(this);

            _exportButton = this.Q<Button>("Export");
            _importButton = this.Q<Button>("Import");
        }

        internal IObservable<Empty> ExportClickedAsObservable => _exportClickedSubject;
        internal IObservable<Empty> ImportClickedAsObservable => _importClickedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
        }

        internal void Setup()
        {
            SetupEventHandlers();
        }

        internal void Open()
        {
            this.SetDisplay(true);
        }

        internal void Close()
        {
            this.SetDisplay(false);
        }

        internal void SetupEventHandlers()
        {
            _exportButton.RegisterCallback<ClickEvent>(OnExportButtonClicked);
            _importButton.RegisterCallback<ClickEvent>(OnImportButtonClicked);
        }

        internal void CleanupEventHandlers()
        {
            _importButton.UnregisterCallback<ClickEvent>(OnImportButtonClicked);
            _exportButton.UnregisterCallback<ClickEvent>(OnExportButtonClicked);
        }

        #region Methods - EventHandlers

        private void OnExportButtonClicked(ClickEvent _)
            => _exportClickedSubject.OnNext(Empty.Default);

        private void OnImportButtonClicked(ClickEvent _)
            => _importClickedSubject.OnNext(Empty.Default);

        #endregion

        #region Uxml

        public new class UxmlFactory : UxmlFactory<OtherOperationPaneView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
