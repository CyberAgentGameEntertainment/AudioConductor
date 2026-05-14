// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#else
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem;
#endif
using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Enums;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal abstract class CueListItem : TreeViewItem
    {
        protected CueListItem(int id) : base(id)
        {
        }

        public abstract ItemType Type { get; }

        public abstract string TargetId { get; }

        public abstract string Name { get; }
        public abstract string? ColorId { get; }

        public abstract int? CategoryId { get; }

        public abstract ThrottleType? ThrottleType { get; }

        public abstract int? ThrottleLimit { get; }

        public abstract float? Volume { get; }

        public abstract float? VolumeRange { get; }

        public abstract CuePlayType? CuePlayType { get; }

        public abstract int? CueId { get; }

        public void InsertOrAddChild(int index, CueListItem child)
        {
            children ??= new List<TreeViewItem>();

            children.Insert(index, child);

            if (child == null)
                return;

            child.parent = this;
        }

        protected void InitializeChildrenList()
        {
            children = new List<TreeViewItem>();
        }
    }
}
