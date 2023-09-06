// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueSheetEditorView : IDisposable
    {
        private readonly VisualElement _root;

        private readonly TabView _tabView;
        private readonly Subject<int> _tabSelectedSubject = new();

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
        }

        public IObservable<int> TabSelectedAsObservable => _tabSelectedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
            _tabView.Dispose();
        }

        public T Q<T>(string name = null, params string[] classes) where T : VisualElement => _root.Q<T>(name, classes);

        public void SelectTab(int tabIndex)
            => _tabView.SelectTab(tabIndex);

        public void Setup()
        {
            _tabView.Setup();
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _tabView.OnTabSelected += OnTabSelected;
        }

        private void CleanupEventHandlers()
        {
            _tabView.OnTabSelected -= OnTabSelected;
        }

        #region Methods - EventHandlers

        private void OnTabSelected(int index)
            => _tabSelectedSubject.OnNext(index);

        #endregion
    }
}
