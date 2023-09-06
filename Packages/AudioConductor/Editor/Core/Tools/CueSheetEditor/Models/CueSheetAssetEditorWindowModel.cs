// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    [Serializable]
    internal sealed class CueSheetAssetEditorWindowModel
    {
        [SerializeField]
        private CueSheetAsset _target;

        [SerializeField]
        private ObservableProperty<CueSheetEditorPresenter.Pane> _paneState = new(CueSheetEditorPresenter.Pane.Default);

        [SerializeField]
        private ObservableProperty<bool> _inspectorUnCollapsed = new(true);

        [SerializeField]
        private CueListTreeView.State _cueListTreeViewState = new();

        private readonly AutoIncrementHistory _history = new();
        private readonly IAssetSaveService _assetSaveService = new AssetSaveService();

        private CueSheetAssetEditorWindowModel()
        {
        }

        public CueSheetAssetEditorWindowModel(CueSheetAsset cueSheetAsset)
        {
            _target = cueSheetAsset;
        }

        public string CueSheetId => _target.cueSheet.Id;

        public ICueSheetEditorModel CueSheetEditorModel { get; private set; }

        public void Setup()
        {
            _assetSaveService.SetAsset(_target);
            CueSheetEditorModel = new CueSheetEditorModel(_target.cueSheet, _history, _assetSaveService, _paneState,
                                                          _inspectorUnCollapsed, _cueListTreeViewState);
        }

        public void Undo() => _history.Undo();

        public void Redo() => _history.Redo();
    }
}
