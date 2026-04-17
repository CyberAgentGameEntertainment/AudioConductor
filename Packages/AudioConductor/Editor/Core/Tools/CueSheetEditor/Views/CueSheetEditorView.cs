// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEngine.UIElements;
using TabView = AudioConductor.Editor.Core.Tools.Shared.TabView;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueSheetEditorView : ICueSheetEditorView
    {
        private readonly Button _cueListButton;
        private readonly Button _otherOperationButton;
        private readonly Button _parameterButton;
        private readonly VisualElement _root;
        private readonly Subject<int> _tabSelectedSubject = new();

        private readonly TabView _tabView;

        public CueSheetEditorView(VisualElement root)
        {
            _root = root;
            var tree = AssetLoader.LoadUxml("CueSheetEditor");
            tree.CloneTree(root);
            root.styleSheets.Add(AssetLoader.LoadUss("Default"));
            var resourceStyleSheets = EditorGUIUtility.isProSkin
                ? AssetLoader.LoadUss("Resource_Dark")
                : AssetLoader.LoadUss("Resource_Light");
            root.styleSheets.Add(resourceStyleSheets);
            _tabView = new TabView(root.Q<VisualElement>("TabContainer"));
            _parameterButton = root.Q<Button>("Parameter");
            _cueListButton = root.Q<Button>("CueList");
            _otherOperationButton = root.Q<Button>("OtherOperation");
            ApplyTooltips();
        }

        public IObservable<int> TabSelectedAsObservable => _tabSelectedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
            _tabView.Dispose();
        }

        public void SelectTab(int tabIndex)
        {
            _tabView.SelectTab(tabIndex);
        }

        public void Setup()
        {
            _tabView.Setup();
            SetupEventHandlers();
        }

        public T Q<T>(string? name = null, params string[] classes) where T : VisualElement
        {
            return _root.Q<T>(name, classes);
        }

        private void ApplyTooltips()
        {
            _parameterButton.tooltip = Localization.Localization.Tr("cue_sheet.tab_parameter");
            _cueListButton.tooltip = Localization.Localization.Tr("cue_sheet.tab_cue_list");
            _otherOperationButton.tooltip = Localization.Localization.Tr("cue_sheet.tab_other_operation");
        }

        private void SetupEventHandlers()
        {
            _tabView.OnTabSelected += OnTabSelected;
            Localization.Localization.LanguageChanged += OnLanguageChanged;
        }

        private void CleanupEventHandlers()
        {
            _tabView.OnTabSelected -= OnTabSelected;
            Localization.Localization.LanguageChanged -= OnLanguageChanged;
        }

        #region Methods - EventHandlers

        private void OnTabSelected(int index)
        {
            _tabSelectedSubject.OnNext(index);
        }

        private void OnLanguageChanged()
        {
            ApplyTooltips();
        }

        #endregion
    }
}
