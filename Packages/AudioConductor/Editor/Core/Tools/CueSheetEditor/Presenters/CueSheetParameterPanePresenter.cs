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
    internal sealed class CueSheetParameterPanePresenter : IDisposable
    {
        private readonly CompositeDisposable _bindDisposable = new();
        private readonly ICueSheetParameterPaneModel _model;

        private readonly CueSheetParameterPaneView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        public CueSheetParameterPanePresenter(ICueSheetParameterPaneModel model, CueSheetParameterPaneView view)
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
            _model.NameObservable
                .Subscribe(_view.SetName)
                .DisposeWith(_bindDisposable);
            _model.ThrottleTypeObservable
                .Subscribe(_view.SetThrottleType)
                .DisposeWith(_bindDisposable);
            _model.ThrottleLimitObservable
                .Subscribe(_view.SetThrottleLimit)
                .DisposeWith(_bindDisposable);
            _model.VolumeObservable
                .Subscribe(_view.SetVolume)
                .DisposeWith(_bindDisposable);
            _model.PitchObservable
                .Subscribe(_view.SetPitch)
                .DisposeWith(_bindDisposable);
            _model.PitchInvertObservable
                .Subscribe(_view.SetPitchInvert)
                .DisposeWith(_bindDisposable);
            _model.CodeGenEnabledObservable
                .Subscribe(_view.SetCodeGenEnabled)
                .DisposeWith(_bindDisposable);
            _model.CodeGenOutputPathObservable
                .Subscribe(_view.SetCodeGenOutputPath)
                .DisposeWith(_bindDisposable);
            _model.CodeGenNamespaceObservable
                .Subscribe(_view.SetCodeGenNamespace)
                .DisposeWith(_bindDisposable);
            _model.CodeGenClassSuffixObservable
                .Subscribe(_view.SetCodeGenClassSuffix)
                .DisposeWith(_bindDisposable);
        }

        private void Unbind()
        {
            _bindDisposable.Clear();
        }

        private void SetupViewEventHandlers()
        {
            _view.NameChangedAsObservable
                .Subscribe(value => _model.Name = value)
                .DisposeWith(_viewEventDisposable);
            _view.ThrottleTypeChangedAsObservable
                .Subscribe(value => _model.ThrottleType = value)
                .DisposeWith(_viewEventDisposable);
            _view.ThrottleLimitChangedAsObservable
                .Subscribe(value => _model.ThrottleLimit = value)
                .DisposeWith(_viewEventDisposable);
            _view.VolumeChangedAsObservable
                .Subscribe(value => _model.Volume = value)
                .DisposeWith(_viewEventDisposable);
            _view.PitchChangedAsObservable
                .Subscribe(value => _model.Pitch = value)
                .DisposeWith(_viewEventDisposable);
            _view.PitchInvertChangedAsObservable
                .Subscribe(value => _model.PitchInvert = value)
                .DisposeWith(_viewEventDisposable);
            _view.CodeGenEnabledChangedAsObservable
                .Subscribe(value => _model.CodeGenEnabled = value)
                .DisposeWith(_viewEventDisposable);
            _view.CodeGenOutputPathChangedAsObservable
                .Subscribe(value => _model.CodeGenOutputPath = value)
                .DisposeWith(_viewEventDisposable);
            _view.CodeGenNamespaceChangedAsObservable
                .Subscribe(value => _model.CodeGenNamespace = value)
                .DisposeWith(_viewEventDisposable);
            _view.CodeGenClassSuffixChangedAsObservable
                .Subscribe(value => _model.CodeGenClassSuffix = value)
                .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }

        public void Open()
        {
            _view.Open();
        }

        public void Close()
        {
            _view.Close();
        }
    }
}
