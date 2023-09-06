// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class ItemCueSheet : CueListItem
    {
        public ItemCueSheet(int id, [NotNull] CueSheet cueSheet) : base(id)
        {
            RawData = cueSheet;
            InitializeChildrenList();
        }

        public CueSheet RawData { get; }

        public override int depth => -1;
        public override string displayName => RawData.name;

        public override ItemType Type => ItemType.CueSheet;
        public override string TargetId => RawData.Id;
        public override string Name => RawData.name;
        public override string ColorId => null;
        public override int? CategoryId => null;
        public override ThrottleType? ThrottleType => RawData.throttleType;
        public override int? ThrottleLimit => RawData.throttleLimit;
        public override float? Volume => RawData.volume;
        public override float? VolumeRange => null;
        public override CuePlayType? CuePlayType => null;

        public void MoveChild(int oldIndex, int newIndex)
        {
            var child = (CueListItem)children[oldIndex];
            var rawChild = RawData.cueList[oldIndex];

            children.RemoveAt(oldIndex);
            RawData.cueList.RemoveAt(oldIndex);
            InsertOrAddChild(newIndex, child);
            RawData.cueList.Insert(newIndex, rawChild);
        }

        public void RemoveChild(int index)
        {
            children.RemoveAt(index);
            RawData.cueList.RemoveAt(index);
        }

        public void InsertChild(int index, ItemCue child)
        {
            InsertOrAddChild(index, child);
            RawData.cueList.Insert(index, child.RawData);
        }
    }
}
