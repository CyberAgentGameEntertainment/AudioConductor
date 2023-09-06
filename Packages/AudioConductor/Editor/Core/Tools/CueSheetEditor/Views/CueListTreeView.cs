// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Runtime.Core.Shared;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed partial class CueListTreeView : TreeView
    {
        private readonly TreeViewItem _rootItem;

        public CueListTreeView(TreeViewItem root, State state) :
            base(state, new MultiColumnHeader(state.MultiColumnHeaderState))
        {
            _rootItem = root;
            showAlternatingRowBackgrounds = true;
            rowHeight = 16;
            multiColumnHeader.ResizeToFit();
            Reload();
        }

        private bool CanEdit { get; set; } = true;

        private bool IsPressingAltKey { get; set; }

        /// <summary>
        ///     Callback for when the selected items are changed.
        /// </summary>
        public event Action<SelectionChangedEvent> OnSelectionChanged;

        /// <summary>
        ///     Callback for when the item move operation requested.
        /// </summary>
        public event Action<ItemMoveOperationRequestedEvent> OnItemMoveOperationRequested;

        /// <summary>
        ///     Callback for when the cue add operation requested.
        /// </summary>
        public event Action<CueAddOperationRequestedEvent> OnCueAddOperationRequested;

        /// <summary>
        ///     Callback for when the track add operation requested.
        /// </summary>
        public event Action<TrackAddOperationRequestedEvent> OnTrackAddOperationRequested;

        /// <summary>
        ///     Callback for when the item remove operation requested.
        /// </summary>
        public event Action<ItemRemoveOperationRequestedEvent> OnItemRemoveOperationRequested;

        /// <summary>
        ///     Callback for when the item duplicate operation requested.
        /// </summary>
        public event Action<ItemDuplicateOperationRequestedEvent> OnItemDuplicateOperationRequested;

        /// <summary>
        ///     Callback for when the asset add operation requested.
        /// </summary>
        public event Action<AssetAddOperationRequestedEvent> OnAssetAddOperationRequested;

        /// <summary>
        ///     Callback for when the column value changed;
        /// </summary>
        public event Action<ColumnValueChangedEvent> OnColumnValueChanged;

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            rect.height -= multiColumnHeader.height;
            rect.y += multiColumnHeader.height;

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
                SetSelection(Array.Empty<int>(), TreeViewSelectionOptions.FireSelectionChanged);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var cellRect = args.GetCellRect(i);
                var columnIndex = args.GetColumn(i);
                CellGUI(columnIndex, cellRect, args);
            }
        }

        private void CellGUI(int columnIndex, Rect cellRect, RowGUIArgs args)
        {
            using var disabledScope = new EditorGUI.DisabledScope(!CanEdit);
            using var changeCheckScope = new EditorGUI.ChangeCheckScope();

            var value = DrawCellGUI();

            if (changeCheckScope.changed && value != null)
                OnColumnValueChanged?.Invoke(new ColumnValueChangedEvent((ColumnType)columnIndex, value, args.item.id));

            #region LocalMethods

            object DrawCellGUI()
            {
                var item = (CueListItem)args.item;
                switch ((ColumnType)columnIndex)
                {
                    case ColumnType.Name:
                        args.rowRect = cellRect;
                        base.RowGUI(args);
                        return null;
                    case ColumnType.Color:
                        return AudioConductorGUI.ColorDefine.Popup(cellRect, item.ColorId);
                    case ColumnType.Category:
                        var categoryId = item.CategoryId;
                        if (categoryId.HasValue == false)
                            return null;
                        var oldCategoryIndex = CategoryListRepository.instance.ToIndex(categoryId.Value);
                        var values = CategoryListRepository.instance.CategoryNames;
                        var newCategoryIndex = EditorGUI.Popup(cellRect, oldCategoryIndex, values);
                        return CategoryListRepository.instance.ToCategoryId(newCategoryIndex);
                    case ColumnType.ThrottleType:
                        var throttleType = item.ThrottleType;
                        return throttleType.HasValue
                            ? EditorGUI.EnumPopup(cellRect, throttleType.Value)
                            : null;
                    case ColumnType.ThrottleLimit:
                        var throttleLimit = item.ThrottleLimit;
                        return throttleLimit.HasValue
                            ? EditorGUI.DelayedIntField(cellRect, throttleLimit.Value)
                            : null;
                    case ColumnType.Volume:
                        var volume = item.Volume;
                        return volume.HasValue
                            ? EditorGUI.Slider(cellRect, volume.Value, ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max)
                            : null;
                    case ColumnType.VolumeRange:
                        var volumeRange = item.VolumeRange;
                        return volumeRange.HasValue
                            ? EditorGUI.DelayedFloatField(cellRect, volumeRange.Value)
                            : null;
                    case ColumnType.PlayType:
                        var playType = item.CuePlayType;
                        return playType.HasValue
                            ? EditorGUI.EnumPopup(cellRect, playType.Value)
                            : null;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }

            #endregion
        }

        protected override TreeViewItem BuildRoot() => _rootItem;

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        protected override bool CanRename(TreeViewItem item) => true;

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename == false)
                return;

            OnColumnValueChanged?.Invoke(new ColumnValueChangedEvent(ColumnType.Name, args.newName, args.itemID));
        }

        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override bool CanBeParent(TreeViewItem item) => ((CueListItem)item).Type == ItemType.Cue;

        private CueListItem[] FindItemsInVisibleRows(IEnumerable<int> itemIds)
        {
            if (itemIds == null)
                return Array.Empty<CueListItem>();

            var itemIdSet = new HashSet<int>(itemIds);
            if (itemIdSet.Count <= 0)
                return Array.Empty<CueListItem>();

            return GetRows().Where(item => itemIdSet.Contains(item.id))
                            .OfType<CueListItem>()
                            .ToArray();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            var selectedItems = FindItemsInVisibleRows(selectedIds);

            CanEdit = selectedItems.All(item => item.Type == selectedItems[0].Type);

            OnSelectionChanged?.Invoke(new SelectionChangedEvent(selectedItems));
        }

        protected override void KeyEvent()
        {
            var e = Event.current;
            IsPressingAltKey = e.alt;

            if (e.type != EventType.KeyDown)
                return;

            if (GetEventAction(e))
                switch (e.keyCode)
                {
                    case KeyCode.D:
                        OnDuplicateOperationChose();
                        break;
                }
            else
                switch (e.keyCode)
                {
                    case KeyCode.Delete or KeyCode.Backspace:
                        OnRemoveOperationChosen();
                        break;
                }
        }

        private static bool GetEventAction(Event e)
        {
#if UNITY_EDITOR_WIN
            return e.control;
#else
            return e.command;
#endif
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (string.IsNullOrEmpty(searchString) == false)
                return false;

            var items = FindItemsInVisibleRows(args.draggedItemIDs);
            if (items.Length <= 0)
                return false;

            return items.All(item => item.Type == items[0].Type);
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            var items = FindItemsInVisibleRows(args.draggedItemIDs);
            if (items.Length <= 0)
                return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = Array.Empty<Object>();
            DragAndDrop.SetGenericData(DragAndDropGenericDataKey.SelectionItems, items);
            DragAndDrop.SetGenericData(DragAndDropGenericDataKey.RootItem, _rootItem);
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            DragAndDrop.StartDrag(items.Length > 1 ? "<Multiple>" : items[0].displayName);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var visualMode = DragAndDropVisualMode.None;

            visualMode = DragAndDrop.paths is { Length: > 0 }
                ? HandleDragAndDropPaths(args)
                : HandleDragAndDropItems(args);

            return visualMode;
        }

        private DragAndDropVisualMode HandleDragAndDropPaths(DragAndDropArgs args)
        {
            if (args.performDrop == false)
                return DragAndDropVisualMode.Copy;
            
            var insertAtIndex = args.dragAndDropPosition == DragAndDropPosition.UponItem
                                    ? args.parentItem.children.Count
                                    : args.insertAtIndex;

            if (insertAtIndex < 0)
                return DragAndDropVisualMode.Rejected;

            foreach (var path in DragAndDrop.paths)
            {
                if (IsValidPath(path) == false)
                    return DragAndDropVisualMode.Rejected;

                if (AssetDatabase.GetMainAssetTypeAtPath(path) != typeof(AudioClip))
                    return DragAndDropVisualMode.Rejected;
            }

            foreach (var path in DragAndDrop.paths.Reverse())
            {
                var asset = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                var evt = new AssetAddOperationRequestedEvent(insertAtIndex, (CueListItem)args.parentItem, asset);
                OnAssetAddOperationRequested?.Invoke(evt);
            }

            #region LocalMethods

            bool IsValidPath(string path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                path = path.Replace('\\', Path.DirectorySeparatorChar)
                           .Replace(Application.dataPath, "Assets");

                return path.StartsWith("Assets", StringComparison.OrdinalIgnoreCase);
            }

            #endregion

            return DragAndDropVisualMode.Copy;
        }

        private DragAndDropVisualMode HandleDragAndDropItems(DragAndDropArgs args)
        {
            var genericData = DragAndDrop.GetGenericData(DragAndDropGenericDataKey.SelectionItems);
            if (genericData is not CueListItem[] draggedItems || draggedItems.Length == 0)
                return DragAndDropVisualMode.None;

            if (args.performDrop == false)
                return IsPressingAltKey ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Link;

            var type = draggedItems[0].Type;
            var dropParentIsRoot = args.parentItem == rootItem || args.parentItem == null;
            var insertAtIndex = args.dragAndDropPosition == DragAndDropPosition.UponItem
                ? args.parentItem.children.Count
                : args.insertAtIndex;

            if (insertAtIndex < 0
                || type == ItemType.Cue && !dropParentIsRoot
                || type == ItemType.Track && dropParentIsRoot)
                return DragAndDropVisualMode.Rejected;

            if (IsPressingAltKey ||
                DragAndDrop.GetGenericData(DragAndDropGenericDataKey.RootItem) is CueListItem root && root != _rootItem)
            {
                foreach (var item in draggedItems.Reverse())
                    OnItemDuplicateOperationRequested?.Invoke(new ItemDuplicateOperationRequestedEvent(insertAtIndex,
                                                                  (CueListItem)args.parentItem, item));
                return DragAndDropVisualMode.Copy;
            }

            foreach (var item in draggedItems.Reverse())
            {
                var oldIndex = item.parent.children.IndexOf(item);
                if (oldIndex < insertAtIndex)
                    insertAtIndex--;
                OnItemMoveOperationRequested?.Invoke(new ItemMoveOperationRequestedEvent(oldIndex,
                                                      insertAtIndex,
                                                         (CueListItem)args.parentItem,
                                                         item));
            }

            return DragAndDropVisualMode.Move;
        }

        protected override void ContextClickedItem(int id) => ContextClicked();

        protected override void ContextClicked() => ContextMenuRequested().ShowAsContext();

        private GenericMenu ContextMenuRequested()
        {
            var items = FindItemsInVisibleRows(GetSelection());

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Add New Cue"), false, OnCueAddOperationChosen);

            var addNewTrackContent = new GUIContent("Add New Track");
            if (items.Length == 1 && items[0].Type == ItemType.Cue)
                menu.AddItem(addNewTrackContent, false, OnTrackAddOperationChosen);
            else
                menu.AddDisabledItem(addNewTrackContent);

            var removeContent = new GUIContent("Remove");
            if (items.Length > 0)
                menu.AddItem(removeContent, false, OnRemoveOperationChosen);
            else
                menu.AddDisabledItem(removeContent);

            var duplicateContent = new GUIContent("Duplicate");
            if (items.Length > 0 && items.All(item => item.Type == items[0].Type))
                menu.AddItem(duplicateContent, false, OnDuplicateOperationChose);
            else
                menu.AddDisabledItem(duplicateContent);

            return menu;
        }

        private void OnCueAddOperationChosen()
        {
            OnCueAddOperationRequested?.Invoke(new CueAddOperationRequestedEvent());
        }

        private void OnTrackAddOperationChosen()
        {
            var items = FindItemsInVisibleRows(GetSelection());
            if (items.Length != 1 || items[0].Type != ItemType.Cue)
                return;

            OnTrackAddOperationRequested?.Invoke(new TrackAddOperationRequestedEvent((ItemCue)items[0]));
        }

        private void OnRemoveOperationChosen()
        {
            var items = FindItemsInVisibleRows(GetSelection());
            if (items.Length <= 0)
                return;

            foreach (var item in items.Reverse())
                OnItemRemoveOperationRequested?.Invoke(new ItemRemoveOperationRequestedEvent(item));
        }

        private void OnDuplicateOperationChose()
        {
            var items = FindItemsInVisibleRows(GetSelection());
            if (items.Length <= 0)
                return;

            if (items.All(item => item.Type == items[0].Type) == false)
                return;

            foreach (var item in items)
            {
                var parent = (CueListItem)item.parent;
                var insertIndex = parent.children.Count;
                OnItemDuplicateOperationRequested?.Invoke(new ItemDuplicateOperationRequestedEvent(insertIndex, parent,
                                                              item));
            }
        }

        protected override void SearchChanged(string newSearch)
        {
            SetSelection(Array.Empty<int>(), TreeViewSelectionOptions.FireSelectionChanged);
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return true;

            var elements = search.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var element in elements)
            {
                var splits = element.Split(':', StringSplitOptions.RemoveEmptyEntries);

                string tag;
                string text;
                if (splits.Length != 2)
                    (tag, text) = (nameof(ColumnType.Name), element);
                else
                    (tag, text) = (splits[0], splits[1]);

                if (DoesItemMatchSearch((CueListItem)item, tag, text) == false)
                    return false;
            }

            return true;
        }

        private static bool DoesItemMatchSearch(CueListItem item, string tag, string text)
        {
            if (tag.Equals("Type", StringComparison.OrdinalIgnoreCase))
            {
                if (text.Equals("Cue", StringComparison.OrdinalIgnoreCase))
                    return item.Type == ItemType.Cue;
                if (text.Equals("Track", StringComparison.OrdinalIgnoreCase))
                    return item.Type == ItemType.Track;
                return false;
            }

            if (tag.Equals(nameof(ColumnType.Name), StringComparison.OrdinalIgnoreCase))
                return item.Name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
            if (tag.Equals(nameof(ColumnType.Color), StringComparison.OrdinalIgnoreCase))
            {
                var colorName = ColorDefineListRepository.instance.GetName(item.ColorId);
                return colorName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (tag.Equals(nameof(ColumnType.Category), StringComparison.OrdinalIgnoreCase))
            {
                const int invalidId = CategoryListRepository.InvalidId;
                var categoryName = CategoryListRepository.instance.GetName(item.CategoryId ?? invalidId);
                return categoryName.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
            }

            if (tag.Equals(nameof(ColumnType.ThrottleType), StringComparison.OrdinalIgnoreCase))
                return item.ThrottleType?.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
            if (tag.Equals(nameof(ColumnType.ThrottleLimit), StringComparison.OrdinalIgnoreCase))
                return item.ThrottleLimit?.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;
            if (tag.Equals(nameof(ColumnType.PlayType), StringComparison.OrdinalIgnoreCase))
                return item.CuePlayType?.ToString().IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0;

            return false;
        }

        private static class DragAndDropGenericDataKey
        {
            public static readonly string SelectionItems = $"{nameof(CueListTreeView)}.{nameof(SelectionItems)}";
            public static readonly string RootItem = $"{nameof(CueListTreeView)}.{nameof(RootItem)}";
        }
    }
}
