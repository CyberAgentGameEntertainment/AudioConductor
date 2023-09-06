// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class CueInspectorPresenter : VisualElement, IDisposable
    {
        private readonly CompositeDisposable _bindDisposable = new();

        private readonly CueInspectorView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        private ICueInspectorModel _model;

        public CueInspectorPresenter(CueInspectorView view)
        {
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
        }

        public void Open([NotNull] ICueInspectorModel model)
        {
            Assert.IsNotNull(model);

            Unbind();
            CleanupViewEventHandlers();

            _model = model;
            _view.Open();

            SetupViewEventHandlers();
            Bind();
        }

        public void Close()
        {
            Unbind();
            CleanupViewEventHandlers();
            _view.Close();
        }

        private void Bind()
        {
            CategoryListRepository.instance.CategoryIdList
                                  .Subscribe(_view.SetCategoryIdList)
                                  .DisposeWith(_bindDisposable);
            _model.NameObservable
                  .Subscribe(_view.SetName)
                  .DisposeWith(_bindDisposable);
            _model.ColorObservable
                  .Subscribe(_view.SetColor)
                  .DisposeWith(_bindDisposable);
            _model.CategoryIdObservable
                  .Subscribe(_view.SetCategory)
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
            _model.VolumeRangeObservable
                  .Subscribe(_view.SetVolumeRange)
                  .DisposeWith(_bindDisposable);
            _model.PitchObservable
                  .Subscribe(_view.SetPitch)
                  .DisposeWith(_bindDisposable);
            _model.PitchRangeObservable
                  .Subscribe(_view.SetPitchRange)
                  .DisposeWith(_bindDisposable);
            _model.PitchInvertObservable
                  .Subscribe(_view.SetPitchInvert)
                  .DisposeWith(_bindDisposable);
            _model.PlayTypeObservable
                  .Subscribe(_view.SetPlayType)
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
            _view.ColorChangedAsObservable
                 .Subscribe(value => _model.Color = value)
                 .DisposeWith(_viewEventDisposable);
            _view.CategoryChangedAsObservable
                 .Subscribe(value => _model.CategoryId = value)
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
            _view.VolumeRangeChangedAsObservable
                 .Subscribe(value => _model.VolumeRange = value)
                 .DisposeWith(_viewEventDisposable);
            _view.PitchChangedAsObservable
                 .Subscribe(value => _model.Pitch = value)
                 .DisposeWith(_viewEventDisposable);
            _view.PitchRangeChangedAsObservable
                 .Subscribe(value => _model.PitchRange = value)
                 .DisposeWith(_viewEventDisposable);
            _view.PitchInvertChangedAsObservable
                 .Subscribe(value => _model.PitchInvert = value)
                 .DisposeWith(_viewEventDisposable);
            _view.PlayTypeChangedAsObservable
                 .Subscribe(value => _model.PlayType = value)
                 .DisposeWith(_viewEventDisposable);
            _view.PlayRequestedAsObservable
                 .Subscribe(_ => _model.PlayCue())
                 .DisposeWith(_viewEventDisposable);
            _view.StopRequestedAsObservable
                 .Subscribe(_ => _model.StopCue())
                 .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }
    }
}
