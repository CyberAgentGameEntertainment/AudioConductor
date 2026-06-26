// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class CueSheetEditorPresenter : IDisposable
    {
        public enum Pane
        {
            CueSheetParameter,
            CueList,
            OtherOperation,

            Default = CueList
        }

        private readonly CompositeDisposable _bindDisposable = new();
        private readonly ICueListEditorPanePresenter _cueListEditorPanePresenter;

        private readonly ICueSheetEditorPanePresenter _cueSheetParameterPanePresenter;

        private readonly ICueSheetEditorModel _model;
        private readonly ICueSheetEditorPanePresenter _otherOperationPanePresenter;

        private readonly ICueSheetEditorView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public CueSheetEditorPresenter(ICueSheetEditorModel model, CueSheetEditorView view)
        {
            _model = model;
            _view = view;
            _cueSheetParameterPanePresenter = new CueSheetParameterPanePresenter(model.CueSheetParameterPaneModel,
                view.Q<CueSheetParameterPaneView>());
            _cueListEditorPanePresenter = new CueListEditorPanePresenter(model.CueListEditorPaneModel,
                view.Q<CueListEditorPaneView>());
            _otherOperationPanePresenter = new OtherOperationPanePresenter(model.OtherOperationPaneModel,
                view.Q<OtherOperationPaneView>());
        }

        internal CueSheetEditorPresenter(
            ICueSheetEditorModel model,
            ICueSheetEditorView view,
            ICueSheetEditorPanePresenter cueSheetParameterPanePresenter,
            ICueListEditorPanePresenter cueListEditorPanePresenter,
            ICueSheetEditorPanePresenter otherOperationPanePresenter)
        {
            _model = model;
            _view = view;
            _cueSheetParameterPanePresenter = cueSheetParameterPanePresenter;
            _cueListEditorPanePresenter = cueListEditorPanePresenter;
            _otherOperationPanePresenter = otherOperationPanePresenter;
        }

        public void Dispose()
        {
            _otherOperationPanePresenter.Dispose();
            _cueListEditorPanePresenter.Dispose();
            _cueSheetParameterPanePresenter.Dispose();

            Unbind();
            CleanupViewEventHandlers();
            _view.Dispose();
        }

        public void FocusCue(string cueEditorId)
        {
            _model.ObservablePaneState.Value = Pane.CueList;
            _cueListEditorPanePresenter.FocusCue(cueEditorId);
        }

        public void Setup()
        {
            _view.Setup();
            SetupViewEventHandlers();
            Bind();

            _cueSheetParameterPanePresenter.Setup();
            _cueListEditorPanePresenter.Setup();
            _otherOperationPanePresenter.Setup();
        }

        private void Bind()
        {
            _model.ObservablePaneState
                .Subscribe(value => _view.SelectTab((int)value))
                .DisposeWith(_bindDisposable);
            _model.ObservablePaneState
                .Subscribe(OnSelectPane)
                .DisposeWith(_bindDisposable);
        }

        private void Unbind()
        {
            _bindDisposable.Clear();
        }

        private void SetupViewEventHandlers()
        {
            _view.TabSelectedAsObservable
                .Subscribe(index => { _model.ObservablePaneState.Value = (Pane)index; })
                .DisposeWith(_viewEventDisposable);
            _cueListEditorPanePresenter.TrackClipChanged += OnTrackClipChanged;
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
            _cueListEditorPanePresenter.TrackClipChanged -= OnTrackClipChanged;
        }

        private void OnTrackClipChanged()
        {
            _model.CueSheetParameterPaneModel.NotifyClipsChanged();
        }

        private void OnSelectPane(Pane pane)
        {
            switch (pane)
            {
                case Pane.CueSheetParameter:
                    _cueSheetParameterPanePresenter.Open();
                    _cueListEditorPanePresenter.Close();
                    _otherOperationPanePresenter.Close();
                    break;
                case Pane.CueList:
                    _cueSheetParameterPanePresenter.Close();
                    _cueListEditorPanePresenter.Open();
                    _otherOperationPanePresenter.Close();
                    break;
                case Pane.OtherOperation:
                    _cueSheetParameterPanePresenter.Close();
                    _cueListEditorPanePresenter.Close();
                    _otherOperationPanePresenter.Open();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pane), pane, null);
            }
        }
    }
}
