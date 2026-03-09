// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

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

        private ICueInspectorModel? _model;

        public CueInspectorPresenter(CueInspectorView view)
        {
            _view = view;
        }

        public void Dispose()
        {
            _model?.StopCue();
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

            _model?.StopCue();
            Unbind();
            CleanupViewEventHandlers();

            _model = model;
            _view.Open();

            SetupViewEventHandlers();
            Bind();
        }

        public void Close()
        {
            _model?.StopCue();
            Unbind();
            CleanupViewEventHandlers();
            _view.Close();
        }

        private void Bind()
        {
            var model = _model!;
            CategoryListRepository.instance.CategoryIdList
                .Subscribe(_view.SetCategoryIdList)
                .DisposeWith(_bindDisposable);
            model.NameObservable
                .Subscribe(_view.SetName)
                .DisposeWith(_bindDisposable);
            model.ColorObservable
                .Subscribe(_view.SetColor)
                .DisposeWith(_bindDisposable);
            model.CategoryIdObservable
                .Subscribe(_view.SetCategory)
                .DisposeWith(_bindDisposable);
            model.ThrottleTypeObservable
                .Subscribe(_view.SetThrottleType)
                .DisposeWith(_bindDisposable);
            model.ThrottleLimitObservable
                .Subscribe(_view.SetThrottleLimit)
                .DisposeWith(_bindDisposable);
            model.VolumeObservable
                .Subscribe(_view.SetVolume)
                .DisposeWith(_bindDisposable);
            model.VolumeRangeObservable
                .Subscribe(_view.SetVolumeRange)
                .DisposeWith(_bindDisposable);
            model.PitchObservable
                .Subscribe(_view.SetPitch)
                .DisposeWith(_bindDisposable);
            model.PitchRangeObservable
                .Subscribe(_view.SetPitchRange)
                .DisposeWith(_bindDisposable);
            model.PitchInvertObservable
                .Subscribe(_view.SetPitchInvert)
                .DisposeWith(_bindDisposable);
            model.PlayTypeObservable
                .Subscribe(_view.SetPlayType)
                .DisposeWith(_bindDisposable);
        }

        private void Unbind()
        {
            _bindDisposable.Clear();
        }

        private void SetupViewEventHandlers()
        {
            var model = _model!;
            _view.NameChangedAsObservable
                .Subscribe(value => model.Name = value)
                .DisposeWith(_viewEventDisposable);
            _view.ColorChangedAsObservable
                .Subscribe(value => model.Color = value)
                .DisposeWith(_viewEventDisposable);
            _view.CategoryChangedAsObservable
                .Subscribe(value => model.CategoryId = value)
                .DisposeWith(_viewEventDisposable);
            _view.ThrottleTypeChangedAsObservable
                .Subscribe(value => model.ThrottleType = value)
                .DisposeWith(_viewEventDisposable);
            _view.ThrottleLimitChangedAsObservable
                .Subscribe(value => model.ThrottleLimit = value)
                .DisposeWith(_viewEventDisposable);
            _view.VolumeChangedAsObservable
                .Subscribe(value => model.Volume = value)
                .DisposeWith(_viewEventDisposable);
            _view.VolumeRangeChangedAsObservable
                .Subscribe(value => model.VolumeRange = value)
                .DisposeWith(_viewEventDisposable);
            _view.PitchChangedAsObservable
                .Subscribe(value => model.Pitch = value)
                .DisposeWith(_viewEventDisposable);
            _view.PitchRangeChangedAsObservable
                .Subscribe(value => model.PitchRange = value)
                .DisposeWith(_viewEventDisposable);
            _view.PitchInvertChangedAsObservable
                .Subscribe(value => model.PitchInvert = value)
                .DisposeWith(_viewEventDisposable);
            _view.PlayTypeChangedAsObservable
                .Subscribe(value => model.PlayType = value)
                .DisposeWith(_viewEventDisposable);
            _view.PlayRequestedAsObservable
                .Subscribe(_ => model.PlayCue())
                .DisposeWith(_viewEventDisposable);
            _view.PauseRequestedAsObservable
                .Subscribe(_ => model.TogglePauseCue())
                .DisposeWith(_viewEventDisposable);
            _view.StopRequestedAsObservable
                .Subscribe(_ => model.StopCue())
                .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }
    }
}
