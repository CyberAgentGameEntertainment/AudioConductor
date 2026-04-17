// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    [Serializable]
    internal sealed class CueSheetAssetEditorWindowModel
    {
        [SerializeField] private CueSheetAsset _target = null!;

        [SerializeField]
        private ObservableProperty<CueSheetEditorPresenter.Pane> _paneState = new(CueSheetEditorPresenter.Pane.Default);

        [SerializeField] private ObservableProperty<bool> _inspectorUnCollapsed = new(true);

        [SerializeField] private CueListTreeView.State _cueListTreeViewState = new();

        private readonly IAssetSaveService _assetSaveService = new AssetSaveService();

        private readonly AutoIncrementHistory _history = new();

        private CueSheetAssetEditorWindowModel()
        {
        }

        public CueSheetAssetEditorWindowModel(CueSheetAsset cueSheetAsset)
        {
            _target = cueSheetAsset;
        }

        public string CueSheetId => _target.cueSheet.Id;

        public ICueSheetEditorModel CueSheetEditorModel { get; private set; } = null!;

        public void Setup(Func<AudioConductorSettings?>? settingsProvider = null)
        {
            _assetSaveService.SetAsset(_target);
            CueSheetEditorModel = new CueSheetEditorModel(_target.cueSheet, _history, _assetSaveService, _paneState,
                _inspectorUnCollapsed, _cueListTreeViewState, _target,
                settingsProvider);
        }

        public void Undo()
        {
            _history.Undo();
        }

        public void Redo()
        {
            _history.Redo();
        }
    }
}
