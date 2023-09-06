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
    internal sealed class ItemCue : CueListItem
    {
        public ItemCue(int id, [NotNull] Cue cue) : base(id)
        {
            RawData = cue;
            InitializeChildrenList();
        }

        public Cue RawData { get; }

        public override int depth => 0;
        public override string displayName => RawData.name;

        public override ItemType Type => ItemType.Cue;
        public override string TargetId => RawData.Id;
        public override string Name => RawData.name;
        public override string ColorId => RawData.colorId;
        public override int? CategoryId => RawData.categoryId;
        public override ThrottleType? ThrottleType => RawData.throttleType;
        public override int? ThrottleLimit => RawData.throttleLimit;
        public override float? Volume => RawData.volume;
        public override float? VolumeRange => RawData.volumeRange;
        public override CuePlayType? CuePlayType => RawData.playType;

        public void RemoveChild(int index)
        {
            children.RemoveAt(index);
            RawData.trackList.RemoveAt(index);
        }

        public void InsertChild(int index, ItemTrack child)
        {
            InsertOrAddChild(index, child);
            RawData.trackList.Insert(index, child.RawData);
        }
    }
}
