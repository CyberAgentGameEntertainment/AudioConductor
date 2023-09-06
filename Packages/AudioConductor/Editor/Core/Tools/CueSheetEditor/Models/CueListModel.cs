// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Models;
using UnityEditor.IMGUI.Controls;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed partial class CueListModel : ICueListModel
    {
        private readonly AutoIncrementHistory _history;
        private readonly IAssetSaveService _assetSaveService;
        private readonly ItemCueSheet _root;

        private readonly Subject<Empty> _moveSubject = new();
        private readonly Subject<CueListItem> _addSubject = new();
        private readonly Subject<CueListItem> _removeSubject = new();

        private readonly Dictionary<int, CueListItem> _itemTable = new();

        private InspectorModel _latestInspectorModel;

        private int _currentItemId;

        public CueListModel([NotNull] CueSheet cueSheet,
                            [NotNull] AutoIncrementHistory history,
                            [NotNull] IAssetSaveService assetSaveService,
                            CueListTreeView.State cueListTreeViewState)
        {
            _history = history;
            _assetSaveService = assetSaveService;
            CueListTreeViewState = cueListTreeViewState;

            _root = new ItemCueSheet(-1, cueSheet);
            _itemTable.Add(_root.id, _root);
            foreach (var cue in cueSheet.cueList)
            {
                var itemCue = new ItemCue(CreateNewId, cue)
                {
                    id = CreateNewId
                };
                _root.AddChild(itemCue);
                _itemTable.Add(itemCue.id, itemCue);
                foreach (var track in cue.trackList)
                {
                    var itemTrack = new ItemTrack(CreateNewId, track);
                    itemCue.AddChild(itemTrack);
                    _itemTable.Add(itemTrack.id, itemTrack);
                }
            }
        }

        private int CreateNewId => _currentItemId++;

        public TreeViewItem Root => _root;

        public CueListTreeView.State CueListTreeViewState { get; }

        public IObservable<Empty> MoveAsObservable => _moveSubject;
        public IObservable<CueListItem> AddAsObservable => _addSubject;
        public IObservable<CueListItem> RemoveAsObservable => _removeSubject;

        public IInspectorModel CreateInspectorModel(CueListItem[] items)
        {
            return _latestInspectorModel = new InspectorModel(items, _history, _assetSaveService);
        }
    }
}
