// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class InspectorModel : IInspectorModel
    {
        private readonly CueInspectorModel _cueInspectorModel;
        private readonly TrackInspectorModel _trackInspectorModel;

        public InspectorModel([NotNull] CueListItem[] items,
                              [NotNull] AutoIncrementHistory history,
                              [NotNull] IAssetSaveService assetSaveService)
        {
            var cueItems = items.OfType<ItemCue>().ToArray();
            var trackItems = items.OfType<ItemTrack>().ToArray();

            if (cueItems.Length == 0 && trackItems.Length == 0)
                InspectorType = InspectorType.None;
            else if (cueItems.Length > 0 && trackItems.Length > 0)
                InspectorType = InspectorType.CueAndTrack;
            else if (cueItems.Length > 0)
            {
                InspectorType = InspectorType.Cue;
                _cueInspectorModel = new CueInspectorModel(cueItems, history, assetSaveService);
            }
            else
            {
                InspectorType = InspectorType.Track;
                _trackInspectorModel = new TrackInspectorModel(trackItems, history, assetSaveService);
            }
        }

        public InspectorType InspectorType { get; }

        public ICueInspectorModel CueInspectorModel => _cueInspectorModel;

        public ITrackInspectorModel TrackInspectorModel => _trackInspectorModel;

        public bool Contains(int itemId)
        {
            if (InspectorType is InspectorType.None or InspectorType.CueAndTrack)
                return false;

            if (_cueInspectorModel?.Contains(itemId) ?? false)
                return true;

            return _trackInspectorModel?.Contains(itemId) ?? false;
        }

        public void ChangeValue(CueListTreeView.ColumnType columnType, object newValue)
        {
            _cueInspectorModel?.ChangeValue(columnType, newValue);
            _trackInspectorModel?.ChangeValue(columnType, newValue);
        }
    }
}
