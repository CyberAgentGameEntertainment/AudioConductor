// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     IMGUI TreeView for displaying the CueEnumDefinition structure.
    /// </summary>
    internal sealed partial class CueEnumDefinitionTreeView : TreeView
    {
        private CueEnumDefinition? _definition;
        private AutoIncrementHistory? _history;
        private int _nextId;

        internal CueEnumDefinitionTreeView(State state)
            : base(state, new MultiColumnHeader(state.MultiColumnHeaderState))
        {
            showAlternatingRowBackgrounds = true;
            rowHeight = 18;
            multiColumnHeader.ResizeToFit();
            Reload();
        }

        internal event Action<CueEnumDefinitionTreeItem?>? OnSelectionChanged;
        internal event Action? OnStructureChanged;

        internal void SetHistory(AutoIncrementHistory? history)
        {
            _history = history;
        }

        internal TreeViewItem? FindItemById(int id)
        {
            return FindItem(id, rootItem);
        }

        internal void SetDefinition(CueEnumDefinition? definition)
        {
            _definition = definition;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            _nextId = 0;
            var root = new TreeViewItem(_nextId++, -1, "Root");

            if (_definition == null)
            {
                root.children = new List<TreeViewItem>();
                return root;
            }

            // rootEntries
            foreach (var asset in _definition.rootEntries)
                root.AddChild(new CueSheetAssetTreeItem(_nextId++, 0, asset));

            // fileEntries
            for (var i = 0; i < _definition.fileEntries.Count; i++)
            {
                var fe = _definition.fileEntries[i];
                var feItem = new FileEntryTreeItem(_nextId++, 0, fe, i);

                foreach (var asset in fe.assets)
                    feItem.AddChild(new CueSheetAssetTreeItem(_nextId++, 1, asset));

                root.AddChild(feItem);
            }

            // excludedEntries
            if (_definition.excludedEntries.Count > 0)
            {
                var excludedHeader = new ExcludedHeaderTreeItem(_nextId++, 0);
                foreach (var asset in _definition.excludedEntries)
                    excludedHeader.AddChild(new CueSheetAssetTreeItem(_nextId++, 1, asset));
                root.AddChild(excludedHeader);
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (var i = 0; i < args.GetNumVisibleColumns(); i++)
                CellGUI(args.GetColumn(i), args.GetCellRect(i), args);
        }

        private void CellGUI(int columnIndex, Rect cellRect, RowGUIArgs args)
        {
            var columnType = (ColumnType)columnIndex;

            switch (columnType)
            {
                case ColumnType.Name:
                    var indent = GetContentIndent(args.item);
                    cellRect.xMin += indent;

                    if (args.item is ExcludedHeaderTreeItem)
                    {
                        var style = new GUIStyle(EditorStyles.label)
                        {
                            alignment = TextAnchor.MiddleCenter,
                            fontStyle = FontStyle.Italic
                        };
                        var prevColor = GUI.color;
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                        GUI.Label(cellRect, "── Excluded ──", style);
                        GUI.color = prevColor;
                    }
                    else if (args.item is FileEntryTreeItem)
                    {
                        GUI.Label(cellRect, EditorGUIUtility.IconContent("Folder Icon"));
                    }
                    else
                    {
                        DefaultGUI.Label(cellRect, args.label, args.selected, args.focused);
                    }

                    break;

                case ColumnType.OutputFile:
                    if (args.item is ExcludedHeaderTreeItem)
                        break;
                    if (args.item is CueSheetAssetTreeItem assetItem && assetItem.depth == 0 &&
                        assetItem.Asset != null)
                    {
                        var baseName = !string.IsNullOrEmpty(assetItem.Asset.cueSheet.name)
                            ? assetItem.Asset.cueSheet.name
                            : assetItem.Asset.name;
                        var suffix = _definition != null ? _definition.defaultClassSuffix ?? "" : "";
                        var enumName = IdentifierConverter.ToPascalCase(baseName) + suffix;
                        DefaultGUI.Label(cellRect, $"{enumName}.cs", args.selected, args.focused);
                    }
                    else if (args.item is FileEntryTreeItem feItem)
                    {
                        var fileName = !string.IsNullOrEmpty(feItem.FileEntry.fileName)
                            ? feItem.FileEntry.fileName
                            : "(Unnamed)";
                        DefaultGUI.Label(cellRect, $"{fileName}.cs", args.selected, args.focused);
                    }

                    break;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
            {
                OnSelectionChanged?.Invoke(null);
                return;
            }

            var item = FindItem(selectedIds[0], rootItem) as CueEnumDefinitionTreeItem;
            OnSelectionChanged?.Invoke(item);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override bool CanRename(TreeViewItem item)
        {
            return item is FileEntryTreeItem;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (!args.acceptedRename || string.IsNullOrEmpty(args.newName))
                return;

            if (FindItem(args.itemID, rootItem) is FileEntryTreeItem feItem)
            {
                var oldName = args.originalName;
                var newName = args.newName;

                if (_history != null)
                {
                    _history.Register(
                        $"Rename FileEntry {newName}",
                        () =>
                        {
                            feItem.FileEntry.fileName = newName;
                            OnStructureChanged?.Invoke();
                        },
                        () =>
                        {
                            feItem.FileEntry.fileName = oldName;
                            OnStructureChanged?.Invoke();
                        });
                }
                else
                {
                    feItem.FileEntry.fileName = newName;
                    feItem.displayName = newName;
                    OnStructureChanged?.Invoke();
                }
            }
        }

        protected override bool CanBeParent(TreeViewItem item)
        {
            return item is FileEntryTreeItem or ExcludedHeaderTreeItem || item == rootItem;
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            if (args.draggedItemIDs.Count == 0)
                return false;

            // ExcludedHeaderTreeItem itself cannot be dragged
            foreach (var id in args.draggedItemIDs)
                if (FindItem(id, rootItem) is ExcludedHeaderTreeItem)
                    return false;

            return true;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            var items = args.draggedItemIDs
                .Select(id => FindItem(id, rootItem))
                .OfType<CueEnumDefinitionTreeItem>()
                .ToArray();
            if (items.Length == 0)
                return;

            DragAndDrop.PrepareStartDrag();
            DragAndDrop.paths = null;
            DragAndDrop.objectReferences = Array.Empty<Object>();
            DragAndDrop.SetGenericData(DragAndDropGenericDataKey.SelectionItems, items);
            DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
            DragAndDrop.StartDrag(items.Length == 1 ? items[0].displayName : $"{items.Length} items");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var items =
                DragAndDrop.GetGenericData(DragAndDropGenericDataKey.SelectionItems) as
                    CueEnumDefinitionTreeItem[];

            if (items == null || items.Length == 0 || _definition == null)
                return DragAndDropVisualMode.Rejected;

            if (items.Any(i => i.Kind != CueEnumDefinitionTreeItem.ItemKind.CueSheetAsset))
                return DragAndDropVisualMode.Rejected;

            // Resolve target list and raw insert index (following CueListTreeView pattern)
            List<CueSheetAsset> targetList;
            int insertIndex;

            if (args.parentItem is ExcludedHeaderTreeItem)
            {
                // Drop onto ExcludedHeader → move to excludedEntries
                targetList = _definition.excludedEntries;
                insertIndex = args.dragAndDropPosition == DragAndDropPosition.UponItem
                    ? targetList.Count
                    : args.insertAtIndex;
            }
            else if (args.parentItem is FileEntryTreeItem targetFe)
            {
                // Drop onto or between items inside a FileEntry
                targetList = targetFe.FileEntry.assets;
                insertIndex = args.dragAndDropPosition == DragAndDropPosition.UponItem
                    ? targetList.Count
                    : args.insertAtIndex;
            }
            else if (args.parentItem is CueSheetAssetTreeItem assetParent)
            {
                // UponItem on a CueSheetAsset leaf — treat as "insert after this item"
                targetList = FindListContaining(assetParent.Asset);
                insertIndex = targetList.IndexOf(assetParent.Asset!) + 1;
            }
            else
            {
                // BetweenItems at root level, or OutsideItems
                targetList = _definition.rootEntries;
                insertIndex = args.dragAndDropPosition == DragAndDropPosition.OutsideItems
                    ? targetList.Count
                    : Math.Min(args.insertAtIndex, targetList.Count);
            }

            if (!args.performDrop)
                return DragAndDropVisualMode.Move;

            var assets = items.OfType<CueSheetAssetTreeItem>()
                .Where(i => i.Asset != null)
                .Select(i => i.Asset!)
                .ToArray();
            if (assets.Length == 0)
                return DragAndDropVisualMode.Rejected;

            // Adjust insert index for items being removed from the same list before the target
            foreach (var asset in assets)
            {
                var idx = targetList.IndexOf(asset);
                if (idx >= 0 && idx < insertIndex)
                    insertIndex--;
            }

            var snapshotBefore = SnapshotDefinitionStructure();

            // Remove all dragged assets from every list
            foreach (var asset in assets)
            {
                _definition.rootEntries.Remove(asset);
                foreach (var fe in _definition.fileEntries)
                    fe.assets.Remove(asset);
                _definition.excludedEntries.Remove(asset);
            }

            // Insert at the adjusted position
            insertIndex = Math.Max(0, Math.Min(insertIndex, targetList.Count));
            targetList.InsertRange(insertIndex, assets);

            var snapshotAfter = SnapshotDefinitionStructure();

            if (_history != null)
                _history.Register(
                    "Move Assets",
                    () =>
                    {
                        RestoreDefinitionStructure(snapshotAfter);
                        OnStructureChanged?.Invoke();
                    },
                    () =>
                    {
                        RestoreDefinitionStructure(snapshotBefore);
                        OnStructureChanged?.Invoke();
                    });
            else
                OnStructureChanged?.Invoke();

            Reload();
            return DragAndDropVisualMode.Move;
        }

        private DefinitionStructureSnapshot SnapshotDefinitionStructure()
        {
            var snapshot = new DefinitionStructureSnapshot();
            snapshot.RootEntries = _definition!.rootEntries.ToList();
            snapshot.FileEntryAssets = _definition.fileEntries
                .Select(fe => fe.assets.ToList())
                .ToList();
            snapshot.ExcludedEntries = _definition.excludedEntries.ToList();
            return snapshot;
        }

        private void RestoreDefinitionStructure(DefinitionStructureSnapshot snapshot)
        {
            if (_definition == null)
                return;

            _definition.rootEntries.Clear();
            _definition.rootEntries.AddRange(snapshot.RootEntries);

            for (var i = 0; i < _definition.fileEntries.Count && i < snapshot.FileEntryAssets.Count; i++)
            {
                _definition.fileEntries[i].assets.Clear();
                _definition.fileEntries[i].assets.AddRange(snapshot.FileEntryAssets[i]);
            }

            _definition.excludedEntries.Clear();
            _definition.excludedEntries.AddRange(snapshot.ExcludedEntries);
        }

        private List<CueSheetAsset> FindListContaining(CueSheetAsset? asset)
        {
            if (_definition != null && asset != null)
            {
                foreach (var fe in _definition.fileEntries)
                    if (fe.assets.Contains(asset))
                        return fe.assets;

                if (_definition.excludedEntries.Contains(asset))
                    return _definition.excludedEntries;
            }

            return _definition!.rootEntries;
        }

        private class DefinitionStructureSnapshot
        {
            public List<CueSheetAsset> ExcludedEntries = new();
            public List<List<CueSheetAsset>> FileEntryAssets = new();
            public List<CueSheetAsset> RootEntries = new();
        }

        private static class DragAndDropGenericDataKey
        {
            public static readonly string SelectionItems =
                $"{nameof(CueEnumDefinitionTreeView)}.{nameof(SelectionItems)}";
        }
    }
}
