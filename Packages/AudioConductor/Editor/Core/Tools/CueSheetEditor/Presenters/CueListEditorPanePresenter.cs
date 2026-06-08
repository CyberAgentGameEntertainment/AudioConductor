// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class CueListEditorPanePresenter : ICueListEditorPanePresenter
    {
        private readonly CompositeDisposable _bindDisposable = new();

        private readonly ICueListPresenter _cueListPresenter;
        private readonly InspectorPresenter? _inspectorPresenter;
        private readonly ICueListEditorPaneModel _model;

        private readonly ICueListEditorPaneView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public CueListEditorPanePresenter(ICueListEditorPaneModel model, CueListEditorPaneView view)
        {
            _model = model;
            _view = view;

            _inspectorPresenter = new InspectorPresenter(view.Q<InspectorView>());
            _cueListPresenter = new CueListPresenter(model.CueListModel, view.Q<CueListView>());
        }

        internal CueListEditorPanePresenter(ICueListEditorPaneModel model, ICueListEditorPaneView view,
            ICueListPresenter cueListPresenter)
        {
            _model = model;
            _view = view;
            _inspectorPresenter = null;
            _cueListPresenter = cueListPresenter;
        }

        public void Dispose()
        {
            _cueListPresenter.Dispose();
            _inspectorPresenter?.Dispose();

            Unbind();
            CleanupEventHandlers();
            CleanupViewEventHandlers();
            _view.Dispose();
        }

        public void Setup()
        {
            _view.Setup();
            SetupViewEventHandlers();
            SetupEventHandlers();
            Bind();
            _view.SetButtonState(_model.VisibleColumns);
            _view.SetSearchString(_model.SearchString);

            _inspectorPresenter?.Setup();
            _cueListPresenter.Setup();
        }

        public void Open()
        {
            _view.Open();
        }

        public void Close()
        {
            _view.Close();
        }

        public void FocusCue(string cueEditorId)
        {
            var itemId = _model.FindItemIdByCueEditorId(cueEditorId);
            if (itemId < 0)
                return;
            _view.ClearSearch();
            _cueListPresenter.FocusItemById(itemId);
        }

        private void Bind()
        {
            _model.ObservableInspectorUnCollapsed
                .Subscribe(_view.SetInspector)
                .DisposeWith(_bindDisposable);
        }

        private void Unbind()
        {
            _bindDisposable.Clear();
        }

        private void SetupViewEventHandlers()
        {
            _view.InspectorToggleChangedAsObservable
                .Subscribe(unCollapsed => { _model.ObservableInspectorUnCollapsed.Value = unCollapsed; })
                .DisposeWith(_viewEventDisposable);
            _view.VolumeToggleChangedAsObservable
                .Subscribe(_cueListPresenter.OnVolumeToggleChanged)
                .DisposeWith(_viewEventDisposable);
            _view.PlayInfoToggleChangedAsObservable
                .Subscribe(_cueListPresenter.OnPlayInfoToggleChanged)
                .DisposeWith(_viewEventDisposable);
            _view.ThrottleToggleChangedAsObservable
                .Subscribe(_cueListPresenter.OnThrottleToggleChanged)
                .DisposeWith(_viewEventDisposable);
            _view.MemoToggleChangedAsObservable
                .Subscribe(_cueListPresenter.OnMemoToggleChanged)
                .DisposeWith(_viewEventDisposable);
            _view.SearchFieldChangedAsObservable
                .Subscribe(_cueListPresenter.OnSearchFieldChanged)
                .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }

        private void SetupEventHandlers()
        {
            if (_inspectorPresenter is not null)
                _cueListPresenter.OnSelectionItemChanged += _inspectorPresenter.SetModel;
        }

        private void CleanupEventHandlers()
        {
            if (_inspectorPresenter is not null)
                _cueListPresenter.OnSelectionItemChanged -= _inspectorPresenter.SetModel;
        }
    }
}
