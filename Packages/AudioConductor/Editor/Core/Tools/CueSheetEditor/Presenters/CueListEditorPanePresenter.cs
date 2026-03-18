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
    internal sealed class CueListEditorPanePresenter : ICueSheetEditorPanePresenter
    {
        private readonly CompositeDisposable _bindDisposable = new();

        private readonly CueListPresenter _cueListPresenter;
        private readonly InspectorPresenter _inspectorPresenter;
        private readonly ICueListEditorPaneModel _model;

        private readonly CueListEditorPaneView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public CueListEditorPanePresenter(ICueListEditorPaneModel model, CueListEditorPaneView view)
        {
            _model = model;
            _view = view;

            _inspectorPresenter = new InspectorPresenter(_view.Q<InspectorView>());
            _cueListPresenter = new CueListPresenter(model.CueListModel, _view.Q<CueListView>());
        }

        public void Dispose()
        {
            _cueListPresenter?.Dispose();
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

            _inspectorPresenter.Setup();
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
            _cueListPresenter.OnSelectionItemChanged += _inspectorPresenter.SetModel;
        }

        private void CleanupEventHandlers()
        {
            _cueListPresenter.OnSelectionItemChanged -= _inspectorPresenter.SetModel;
        }
    }
}
