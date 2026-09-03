// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    internal sealed partial class OtherOperationPaneView : VisualElement, IDisposable
#else
    internal sealed class OtherOperationPaneView : VisualElement, IDisposable
#endif
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
            ApplyTooltips();
        }

        internal IObservable<Empty> ExportClickedAsObservable => _exportClickedSubject;
        internal IObservable<Empty> ImportClickedAsObservable => _importClickedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
            Localization.Localization.LanguageChanged -= OnLanguageChanged;
        }

        internal void Setup()
        {
            SetupEventHandlers();
            Localization.Localization.LanguageChanged += OnLanguageChanged;
        }

        internal void Open()
        {
            this.SetDisplay(true);
        }

        internal void Close()
        {
            this.SetDisplay(false);
        }

        private void ApplyTooltips()
        {
            _exportButton.tooltip = Localization.Localization.Tr("other_operation.export_csv");
            _importButton.tooltip = Localization.Localization.Tr("other_operation.import_csv");
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
        {
            _exportClickedSubject.OnNext(Empty.Default);
        }

        private void OnImportButtonClicked(ClickEvent _)
        {
            _importClickedSubject.OnNext(Empty.Default);
        }

        private void OnLanguageChanged()
        {
            ApplyTooltips();
        }

        #endregion

        #region Uxml

#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<OtherOperationPaneView, UxmlTraits>
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
