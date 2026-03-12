// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Editor.Core.Models;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    /// <summary>
    ///     IMGUI TreeView for displaying the CueEnumDefinition structure.
    /// </summary>
    internal sealed partial class CueEnumDefinitionTreeView : TreeView
    {
        private CueEnumDefinition? _definition;
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

            var children = new List<TreeViewItem>();

            // rootEntries
            foreach (var asset in _definition.rootEntries)
            {
                var item = new CueSheetAssetTreeItem(_nextId++, 0, asset);
                children.Add(item);
            }

            // fileEntries
            for (var i = 0; i < _definition.fileEntries.Count; i++)
            {
                var fe = _definition.fileEntries[i];
                var feItem = new FileEntryTreeItem(_nextId++, 0, fe, i);

                foreach (var asset in fe.assets)
                {
                    var assetItem = new CueSheetAssetTreeItem(_nextId++, 1, asset);
                    feItem.AddChild(assetItem);
                }

                children.Add(feItem);
            }

            root.children = children;
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

                    if (args.item is FileEntryTreeItem)
                        GUI.Label(cellRect, EditorGUIUtility.IconContent("Folder Icon"));
                    else
                        DefaultGUI.Label(cellRect, args.label, args.selected, args.focused);
                    break;

                case ColumnType.OutputFile:
                    if (args.item is CueSheetAssetTreeItem assetItem && assetItem.depth == 0 &&
                        assetItem.Asset != null)
                    {
                        var baseName = !string.IsNullOrEmpty(assetItem.Asset.cueSheet.name)
                            ? assetItem.Asset.cueSheet.name
                            : assetItem.Asset.name;
                        var enumName = IdentifierConverter.ToPascalCase(baseName);
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
                feItem.FileEntry.fileName = args.newName;
                feItem.displayName = args.newName;
                OnStructureChanged?.Invoke();
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args)
        {
            return args.draggedItemIDs.Count > 0;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            var items = args.draggedItemIDs
                .Select(id => FindItem(id, rootItem))
                .OfType<CueEnumDefinitionTreeItem>()
                .ToArray();
            DragAndDrop.SetGenericData(DragAndDropGenericDataKey.SelectionItems, items);
            DragAndDrop.StartDrag(items.Length == 1 ? items[0].displayName : $"{items.Length} items");
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            var items =
                DragAndDrop.GetGenericData(DragAndDropGenericDataKey.SelectionItems) as
                    CueEnumDefinitionTreeItem[];
            if (items == null || items.Length == 0 || _definition == null)
                return DragAndDropVisualMode.Rejected;

            // Only allow CueSheetAsset items to be moved
            if (items.Any(i => i.Kind != CueEnumDefinitionTreeItem.ItemKind.CueSheetAsset))
                return DragAndDropVisualMode.Rejected;

            if (args.performDrop)
            {
                var assetItems = items.OfType<CueSheetAssetTreeItem>().Where(i => i.Asset != null).ToArray();

                if (args.parentItem is FileEntryTreeItem targetFe)
                    MoveAssetsToFileEntry(assetItems, targetFe);
                else
                    MoveAssetsToRoot(assetItems);

                OnStructureChanged?.Invoke();
                Reload();
            }

            return DragAndDropVisualMode.Move;
        }

        private void MoveAssetsToFileEntry(CueSheetAssetTreeItem[] items, FileEntryTreeItem target)
        {
            if (_definition == null)
                return;

            foreach (var item in items)
            {
                if (item.Asset == null)
                    continue;

                // Remove from current location
                _definition.rootEntries.Remove(item.Asset);
                foreach (var fe in _definition.fileEntries)
                    fe.assets.Remove(item.Asset);

                // Add to target
                target.FileEntry.assets.Add(item.Asset);
            }
        }

        private void MoveAssetsToRoot(CueSheetAssetTreeItem[] items)
        {
            if (_definition == null)
                return;

            foreach (var item in items)
            {
                if (item.Asset == null)
                    continue;

                // Remove from current location
                _definition.rootEntries.Remove(item.Asset);
                foreach (var fe in _definition.fileEntries)
                    fe.assets.Remove(item.Asset);

                // Add to root
                _definition.rootEntries.Add(item.Asset);
            }
        }

        // --- Drag and Drop ---

        private static class DragAndDropGenericDataKey
        {
            public static readonly string SelectionItems =
                $"{nameof(CueEnumDefinitionTreeView)}.{nameof(SelectionItems)}";
        }
    }
}
