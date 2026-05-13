// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.CueSheetEditor;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Presenters;
using AudioConductor.Editor.Core.Tools.CueSheetList.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetList
{
    internal sealed class CueSheetListWindow : EditorWindow
    {
        [SerializeField] private string _searchText = string.Empty;

        private readonly CompositeDisposable _disposable = new();
        private CueSheetListModel? _model;
        private CueSheetListPresenter? _presenter;

        private void OnDisable()
        {
            _disposable.Clear();
            _presenter?.Dispose();
            _model?.Dispose();
            _presenter = null;
            _model = null;
        }

        private void CreateGUI()
        {
            titleContent = new GUIContent("CueSheet List");
            minSize = new Vector2(320, 240);

            var repository = CueSheetAssetRepository.instance;
            var model = new CueSheetListModel(repository);
            model.SearchFilter.Value = _searchText;
            _model = model;

            var view = new CueSheetListView(rootVisualElement);
            _presenter = new CueSheetListPresenter(model, view);
            _presenter.Setup();
            view.SetSearchText(_searchText);

            model.SearchFilter
                .Subscribe(text => _searchText = text)
                .DisposeWith(_disposable);

            model.OpenRequested
                .Subscribe(asset => CueSheetAssetEditorWindow.Open(asset))
                .DisposeWith(_disposable);
        }

        [MenuItem("Tools/Audio Conductor/CueSheet List")]
        private static void Open()
        {
            var window = GetWindow<CueSheetListWindow>();
            window.titleContent = new GUIContent("CueSheet List");
            window.minSize = new Vector2(320, 240);
            window.Show();
        }
    }
}
