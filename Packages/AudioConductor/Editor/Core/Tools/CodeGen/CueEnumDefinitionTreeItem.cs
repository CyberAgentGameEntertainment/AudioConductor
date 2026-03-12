// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using UnityEditor.IMGUI.Controls;

namespace AudioConductor.Editor.Core.Tools.CodeGen
{
    internal abstract class CueEnumDefinitionTreeItem : TreeViewItem
    {
        protected CueEnumDefinitionTreeItem(int id, int depth, string displayName) : base(id, depth, displayName)
        {
        }

        internal abstract ItemKind Kind { get; }

        internal enum ItemKind
        {
            CueSheetAsset,
            FileEntry,
            ExcludedHeader
        }
    }

    internal sealed class CueSheetAssetTreeItem : CueEnumDefinitionTreeItem
    {
        internal CueSheetAssetTreeItem(int id, int depth, CueSheetAsset? asset)
            : base(id, depth, asset != null ? asset.name : "(Missing)")
        {
            Asset = asset;
        }

        internal CueSheetAsset? Asset { get; }

        internal override ItemKind Kind => ItemKind.CueSheetAsset;
    }

    internal sealed class ExcludedHeaderTreeItem : CueEnumDefinitionTreeItem
    {
        internal ExcludedHeaderTreeItem(int id, int depth)
            : base(id, depth, "Excluded")
        {
            children = new List<TreeViewItem>();
        }

        internal override ItemKind Kind => ItemKind.ExcludedHeader;
    }

    internal sealed class FileEntryTreeItem : CueEnumDefinitionTreeItem
    {
        internal FileEntryTreeItem(int id, int depth, FileEntry fileEntry, int fileEntryIndex)
            : base(id, depth, !string.IsNullOrEmpty(fileEntry.fileName) ? fileEntry.fileName : "(Unnamed)")
        {
            FileEntry = fileEntry;
            FileEntryIndex = fileEntryIndex;
            children = new List<TreeViewItem>();
        }

        internal FileEntry FileEntry { get; }
        internal int FileEntryIndex { get; }

        internal override ItemKind Kind => ItemKind.FileEntry;
    }
}
