// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Diagnostics.CodeAnalysis;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class ItemTrack : CueListItem
    {
        public ItemTrack(int id, [NotNull] Track track) : base(id)
        {
            RawData = track;
        }

        public Track RawData { get; }

        public override int depth => 1;
        public override string displayName => RawData.name;

        public override ItemType Type => ItemType.Track;
        public override string TargetId => RawData.Id;
        public override string Name => RawData.name;
        public override string? ColorId => RawData.colorId;
        public override int? CategoryId => null;
        public override ThrottleType? ThrottleType => null;
        public override int? ThrottleLimit => null;
        public override float? Volume => RawData.volume;
        public override float? VolumeRange => RawData.volumeRange;
        public override CuePlayType? CuePlayType => null;
    }
}
