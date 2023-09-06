// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Runtime.Core.Enums;
using UnityEditor.IMGUI.Controls;

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
        public abstract string ColorId { get; }

        public abstract int? CategoryId { get; }

        public abstract ThrottleType? ThrottleType { get; }

        public abstract int? ThrottleLimit { get; }

        public abstract float? Volume { get; }

        public abstract float? VolumeRange { get; }

        public abstract CuePlayType? CuePlayType { get; }

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
