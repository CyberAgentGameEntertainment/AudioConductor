// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Views
{
    internal sealed class CueSheetListView : ICueSheetListView
    {
        private readonly Subject<CueSheetAsset> _openRequested = new();
        private readonly VisualElement _root;
        private readonly Subject<string> _searchTextChanged = new();

        private List<CueSheetListItem> _currentItems = new();
        private VisualElement? _emptyHelpBox;
        private ListView? _listView;
        private ToolbarSearchField? _searchField;

        public CueSheetListView(VisualElement root)
        {
            _root = root;
            var tree = AssetLoader.LoadUxml("CueSheetList");
            tree.CloneTree(root);
            root.styleSheets.Add(AssetLoader.LoadUss("Default"));
            root.styleSheets.Add(EditorGUIUtility.isProSkin
                ? AssetLoader.LoadUss("Resource_Dark")
                : AssetLoader.LoadUss("Resource_Light"));
        }

        public IObservable<string> SearchTextChangedAsObservable => _searchTextChanged;
        public IObservable<CueSheetAsset> OpenRequestedAsObservable => _openRequested;

        public void Dispose()
        {
            CleanupEventHandlers();
            _searchTextChanged.Dispose();
            _openRequested.Dispose();
        }

        public void Setup()
        {
            _searchField = _root.Q<ToolbarSearchField>("SearchField");
            _listView = _root.Q<ListView>("ListView");
            _emptyHelpBox = _root.Q<VisualElement>("EmptyHelpBox");

            _listView.makeItem = MakeItem;
            _listView.bindItem = BindItem;
            _listView.itemsSource = _currentItems;

            SetupEventHandlers();
        }

        public void RenderItems(CueSheetListItem[] items)
        {
            _currentItems = items is not null ? new List<CueSheetListItem>(items) : new List<CueSheetListItem>();
            var isEmpty = _currentItems.Count == 0;

            if (_listView is not null)
            {
                _listView.style.display = isEmpty ? DisplayStyle.None : DisplayStyle.Flex;
                _listView.itemsSource = _currentItems;
                _listView.Rebuild();
            }

            if (_emptyHelpBox is not null)
                _emptyHelpBox.style.display = isEmpty ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void SetSearchText(string text)
        {
            _searchField?.SetValueWithoutNotify(text);
        }

        private static VisualElement MakeItem()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingLeft = 4;
            row.style.paddingRight = 4;

            var nameLabel = new Label();
            nameLabel.name = "NameLabel";
            nameLabel.style.flexGrow = 1;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(nameLabel);

            var countLabel = new Label();
            countLabel.name = "CountLabel";
            countLabel.style.unityTextAlign = TextAnchor.MiddleRight;
            row.Add(countLabel);

            return row;
        }

        private void BindItem(VisualElement element, int index)
        {
            if (index < 0 || index >= _currentItems.Count)
                return;
            var item = _currentItems[index];
            element.Q<Label>("NameLabel").text = item.DisplayName;
            element.Q<Label>("CountLabel").text = $"Cues: {item.CueCount}";
        }

        private void SetupEventHandlers()
        {
            if (_searchField is not null)
                _searchField.RegisterValueChangedCallback(OnSearchTextChanged);
            if (_listView is not null)
                _listView.itemsChosen += OnItemsChosen;
        }

        private void CleanupEventHandlers()
        {
            if (_searchField is not null)
                _searchField.UnregisterValueChangedCallback(OnSearchTextChanged);
            if (_listView is not null)
                _listView.itemsChosen -= OnItemsChosen;
        }

        private void OnSearchTextChanged(ChangeEvent<string> e)
        {
            _searchTextChanged.OnNext(e.newValue);
        }

        private void OnItemsChosen(IEnumerable<object> chosen)
        {
            foreach (var obj in chosen)
                if (obj is CueSheetListItem item)
                    _openRequested.OnNext(item.Asset);
        }
    }
}
