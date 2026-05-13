// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetList.Views;
using AudioConductor.Editor.Foundation.TinyRx;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Presenters
{
    internal sealed class CueSheetListPresenter : IDisposable
    {
        private readonly CompositeDisposable _bindDisposable = new();
        private readonly ICueSheetListModel _model;
        private readonly ICueSheetListView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public CueSheetListPresenter(CueSheetListModel model, CueSheetListView view)
            : this(model, (ICueSheetListView)view)
        {
        }

        internal CueSheetListPresenter(ICueSheetListModel model, ICueSheetListView view)
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

        public void Setup()
        {
            _view.Setup();
            SetupViewEventHandlers();
            Bind();
        }

        private void Bind()
        {
            _model.Items
                .Subscribe(items => _view.RenderItems(items))
                .DisposeWith(_bindDisposable);
        }

        private void Unbind()
        {
            _bindDisposable.Clear();
        }

        private void SetupViewEventHandlers()
        {
            _view.SearchTextChangedAsObservable
                .Subscribe(text => _model.SearchFilter.Value = text)
                .DisposeWith(_viewEventDisposable);

            _view.OpenRequestedAsObservable
                .Subscribe(asset => _model.RequestOpen(asset))
                .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }
    }
}
