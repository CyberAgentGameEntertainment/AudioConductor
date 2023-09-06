// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueListView : VisualElement, IDisposable
    {
        private readonly List<int> _addedItemIds = new();
        private readonly IMGUIContainer _imguiContainer;

        private readonly Subject<SelectionChangedEvent> _selectionChangedSubject = new();
        private readonly Subject<ItemMoveOperationRequestedEvent> _itemMoveOperationRequestedSubject = new();
        private readonly Subject<CueAddOperationRequestedEvent> _cueAddOperationRequestedSubject = new();
        private readonly Subject<TrackAddOperationRequestedEvent> _trackAddOperationRequestedSubject = new();
        private readonly Subject<ItemRemoveOperationRequestedEvent> _itemRemoveOperationRequestedSubject = new();
        private readonly Subject<ItemDuplicateOperationRequestedEvent> _itemDuplicateOperationRequestedSubject = new();
        private readonly Subject<AssetAddOperationRequestedEvent> _assetAddOperationRequestedSubject = new();
        private readonly Subject<ColumnValueChangedEvent> _columnValueChangedSubject = new();

        private int _lastAddedFrame;

        private CueListTreeView _treeView;

        public CueListView()
        {
            var treeAsset = AssetLoader.LoadUxml("CueList");
            treeAsset.CloneTree(this);

            _imguiContainer = this.Q<IMGUIContainer>();
        }

        internal IObservable<SelectionChangedEvent> SelectionChangedAsObservable
            => _selectionChangedSubject;

        internal IObservable<ItemMoveOperationRequestedEvent> ItemMoveOperationRequestedAsObservable
            => _itemMoveOperationRequestedSubject;

        internal IObservable<CueAddOperationRequestedEvent> CueAddOperationRequestedAsObservable =>
            _cueAddOperationRequestedSubject;

        internal IObservable<TrackAddOperationRequestedEvent> TrackAddOperationRequestedAsObservable =>
            _trackAddOperationRequestedSubject;

        internal IObservable<ItemRemoveOperationRequestedEvent> ItemRemoveOperationRequestedAsObservable =>
            _itemRemoveOperationRequestedSubject;

        internal IObservable<ItemDuplicateOperationRequestedEvent> ItemDuplicateOperationRequestedAsObservable =>
            _itemDuplicateOperationRequestedSubject;

        internal IObservable<AssetAddOperationRequestedEvent> AssetAddOperationRequestedAsObservable =>
            _assetAddOperationRequestedSubject;

        internal IObservable<ColumnValueChangedEvent> ColumnValueChangedAsObservable
            => _columnValueChangedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
            _imguiContainer.onGUIHandler = null;
        }

        internal void Setup(TreeViewItem root, CueListTreeView.State state)
        {
            _imguiContainer.onGUIHandler = OnGUI;

            _treeView = new CueListTreeView(root, state);
            SetupEventHandlers();
        }

        private void OnGUI()
        {
            var rect = _imguiContainer.contentRect;

            var treeViewRect =
                GUILayoutUtility.GetRect(rect.width, rect.height, GUILayout.ExpandWidth(true),
                                         GUILayout.ExpandHeight(true));
            _treeView?.OnGUI(treeViewRect);
        }

        internal void SetSelection(IList<int> selectedIDs)
        {
            _treeView.SetSelection(selectedIDs, TreeViewSelectionOptions.FireSelectionChanged);
        }

        internal void ToggleColumnGroupVisible(bool isVisible, IEnumerable<int> columnsIndex)
        {
            var visibleColumns = _treeView.multiColumnHeader.state.visibleColumns;
            var newVisibleColumns =
                isVisible ? visibleColumns.Union(columnsIndex) : visibleColumns.Except(columnsIndex);
            _treeView.multiColumnHeader.state.visibleColumns = newVisibleColumns.OrderBy(i => i).ToArray();
            _treeView.multiColumnHeader.ResizeToFit();
        }

        internal void Search(string searchString)
        {
            _treeView.searchString = searchString;
        }

        internal void OnItemAdded(CueListItem item)
        {
            if (Time.frameCount != _lastAddedFrame)
            {
                _lastAddedFrame = Time.frameCount;
                _addedItemIds.Clear();
            }

            Refresh();
            _treeView.SetExpanded(item.parent.id, true);
            _addedItemIds.Add(item.id);
            SetSelection(_addedItemIds);
        }

        internal void OnItemRemoved(CueListItem item)
        {
            Refresh();
            var selection = _treeView.GetSelection().ToList();
            selection.Remove(item.id);
            SetSelection(selection);
        }

        internal void Refresh()
        {
            _treeView.Reload();
        }

        private void SetupEventHandlers()
        {
            _treeView.OnSelectionChanged += OnSelectionChanged;
            _treeView.OnItemMoveOperationRequested += OnItemMoveOperationRequested;
            _treeView.OnCueAddOperationRequested += OnCueAddOperationRequested;
            _treeView.OnTrackAddOperationRequested += OnTrackAddOperationRequested;
            _treeView.OnItemRemoveOperationRequested += OnItemRemoveOperationRequested;
            _treeView.OnItemDuplicateOperationRequested += OnItemDuplicateOperationRequested;
            _treeView.OnAssetAddOperationRequested += OnAssetAddOperationRequested;
            _treeView.OnColumnValueChanged += OnColumnValueChanged;
        }

        private void CleanupEventHandlers()
        {
            _treeView.OnColumnValueChanged -= OnColumnValueChanged;
            _treeView.OnAssetAddOperationRequested -= OnAssetAddOperationRequested;
            _treeView.OnItemDuplicateOperationRequested -= OnItemDuplicateOperationRequested;
            _treeView.OnItemRemoveOperationRequested -= OnItemRemoveOperationRequested;
            _treeView.OnTrackAddOperationRequested -= OnTrackAddOperationRequested;
            _treeView.OnCueAddOperationRequested -= OnCueAddOperationRequested;
            _treeView.OnItemMoveOperationRequested -= OnItemMoveOperationRequested;
            _treeView.OnSelectionChanged -= OnSelectionChanged;
        }

        #region Methods - EventHandlers

        private void OnSelectionChanged(SelectionChangedEvent evt)
            => _selectionChangedSubject.OnNext(evt);

        private void OnItemMoveOperationRequested(ItemMoveOperationRequestedEvent evt)
            => _itemMoveOperationRequestedSubject.OnNext(evt);

        private void OnCueAddOperationRequested(CueAddOperationRequestedEvent evt)
            => _cueAddOperationRequestedSubject.OnNext(evt);

        private void OnTrackAddOperationRequested(TrackAddOperationRequestedEvent evt)
            => _trackAddOperationRequestedSubject.OnNext(evt);

        private void OnItemRemoveOperationRequested(ItemRemoveOperationRequestedEvent evt)
            => _itemRemoveOperationRequestedSubject.OnNext(evt);

        private void OnItemDuplicateOperationRequested(ItemDuplicateOperationRequestedEvent evt)
            => _itemDuplicateOperationRequestedSubject.OnNext(evt);

        private void OnAssetAddOperationRequested(AssetAddOperationRequestedEvent evt)
            => _assetAddOperationRequestedSubject.OnNext(evt);

        private void OnColumnValueChanged(ColumnValueChangedEvent evt)
            => _columnValueChangedSubject.OnNext(evt);

        #endregion

        #region Uxml

        public new class UxmlFactory : UxmlFactory<CueListView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
