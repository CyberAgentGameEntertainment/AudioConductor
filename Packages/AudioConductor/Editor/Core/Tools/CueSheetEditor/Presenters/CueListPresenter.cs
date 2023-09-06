// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class CueListPresenter : IDisposable
    {
        private readonly ICueListModel _model;
        private readonly CompositeDisposable _bindEventDisposable = new();

        private readonly CueListView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public CueListPresenter(ICueListModel model, CueListView view)
        {
            _model = model;
            _view = view;
        }

        public void Dispose()
        {
            Unbind();
            CleanupViewEventHandlers();
            _view.Dispose();
        }

        public event Action<IInspectorModel> OnSelectionItemChanged;

        public void Setup()
        {
            _view.Setup(_model.Root, _model.CueListTreeViewState);
            SetupViewEventHandlers();
            Bind();

            RestoreSelection(_model.CueListTreeViewState.selectedIDs);
        }

        private void Bind()
        {
            _model.MoveAsObservable
                  .Subscribe(_ => _view.Refresh())
                  .DisposeWith(_bindEventDisposable);

            _model.AddAsObservable
                  .Subscribe(_view.OnItemAdded)
                  .DisposeWith(_bindEventDisposable);

            _model.RemoveAsObservable
                  .Subscribe(_view.OnItemRemoved)
                  .DisposeWith(_bindEventDisposable);
        }

        private void Unbind()
        {
            _bindEventDisposable.Clear();
        }

        public void RestoreSelection(IList<int> selectedIDs)
        {
            _view.SetSelection(selectedIDs);
        }

        public void OnVolumeToggleChanged(bool active)
            => _view.ToggleColumnGroupVisible(active, CueListTreeView.VolumeColumnGroup);

        public void OnPlayInfoToggleChanged(bool active)
            => _view.ToggleColumnGroupVisible(active, CueListTreeView.PlayInfoColumnGroup);

        public void OnThrottleToggleChanged(bool active)
            => _view.ToggleColumnGroupVisible(active, CueListTreeView.ThrottleColumnGroup);

        public void OnMemoToggleChanged(bool active)
            => _view.ToggleColumnGroupVisible(active, CueListTreeView.MemoColumnGroup);

        public void OnSearchFieldChanged(string text)
            => _view.Search(text);

        private void SetupViewEventHandlers()
        {
            _view.SelectionChangedAsObservable
                 .Subscribe(OnListItemSelected)
                 .DisposeWith(_viewEventDisposable);
            _view.ItemMoveOperationRequestedAsObservable
                 .Subscribe(_model.MoveItem)
                 .DisposeWith(_viewEventDisposable);
            _view.CueAddOperationRequestedAsObservable
                 .Subscribe(_model.AddCue)
                 .DisposeWith(_viewEventDisposable);
            _view.TrackAddOperationRequestedAsObservable
                 .Subscribe(_model.AddTrack)
                 .DisposeWith(_viewEventDisposable);
            _view.ItemRemoveOperationRequestedAsObservable
                 .Subscribe(_model.RemoveItem)
                 .DisposeWith(_viewEventDisposable);
            _view.ItemDuplicateOperationRequestedAsObservable
                 .Subscribe(_model.DuplicateItem)
                 .DisposeWith(_viewEventDisposable);
            _view.AssetAddOperationRequestedAsObservable
                 .Subscribe(_model.AddAsset)
                 .DisposeWith(_viewEventDisposable);
            _view.ColumnValueChangedAsObservable
                 .Subscribe(_model.ChangeValue)
                 .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }

        private void OnListItemSelected(SelectionChangedEvent evt)
        {
            var inspectorModel = _model.CreateInspectorModel(evt.items);
            OnSelectionItemChanged?.Invoke(inspectorModel);
        }
    }
}
