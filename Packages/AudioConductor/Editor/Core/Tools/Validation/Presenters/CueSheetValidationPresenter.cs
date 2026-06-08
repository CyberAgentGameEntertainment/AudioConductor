// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Editor.Core.Tools.Validation.Models;
using AudioConductor.Editor.Core.Tools.Validation.Views;
using AudioConductor.Editor.Foundation.TinyRx;

namespace AudioConductor.Editor.Core.Tools.Validation.Presenters
{
    internal sealed class CueSheetValidationPresenter : IDisposable
    {
        private readonly CompositeDisposable _bindDisposable = new();
        private readonly ICueSheetValidationWindowModel _model;
        private readonly ICueSheetValidationView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        internal CueSheetValidationPresenter(ICueSheetValidationWindowModel model, ICueSheetValidationView view)
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
            var showSettings = _model.AllSettings.Length > 1;
            _view.Setup(_model.AllSettings, showSettings);

            SetupViewEventHandlers();
            Bind();

            Action postSetup = _model.Scope switch
            {
                ValidationScope.None => _view.ShowEmpty,
                ValidationScope.Selected => _model.RunValidation,
                ValidationScope.All => _model.RunValidation,
                _ => throw new ArgumentOutOfRangeException(nameof(_model.Scope), _model.Scope, null)
            };
            postSetup();
        }

        private void Bind()
        {
            _model.SelectedSettings
                .Subscribe(settings => _view.SetSelectedSettings(settings))
                .DisposeWith(_bindDisposable);

            _model.ResultRows
                .Subscribe(rows => _view.RenderResults(rows))
                .DisposeWith(_bindDisposable);
        }

        private void Unbind()
        {
            _bindDisposable.Clear();
        }

        private void SetupViewEventHandlers()
        {
            _view.SettingsChangedAsObservable
                .Subscribe(settings => _model.SelectSettings(settings))
                .DisposeWith(_viewEventDisposable);

            _view.ValidateClickedAsObservable
                .Subscribe(_ => _model.RunValidation())
                .DisposeWith(_viewEventDisposable);

            _view.RowSelectedAsObservable
                .Subscribe(row => _model.SelectRow(row))
                .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }
    }
}
