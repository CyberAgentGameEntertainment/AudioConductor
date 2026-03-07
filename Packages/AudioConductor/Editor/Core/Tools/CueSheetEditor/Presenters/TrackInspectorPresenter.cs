// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEngine.Assertions;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Presenters
{
    internal sealed class TrackInspectorPresenter : IDisposable
    {
        private readonly CompositeDisposable _bindDisposable = new();

        private readonly TrackInspectorView _view;
        private readonly CompositeDisposable _viewEventDisposable = new();

        private ITrackInspectorModel? _model;

        public TrackInspectorPresenter(TrackInspectorView view)
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

        public void Open([NotNull] ITrackInspectorModel model)
        {
            Assert.IsNotNull(model);

            Unbind();
            CleanupViewEventHandlers();

            _model = model;
            _view.Open();
            _view.SetPreviewAreaEnabled(_model.CanPreview);

            SetupViewEventHandlers();
            Bind();
        }

        public void Close()
        {
            _view.Close();
        }

        private void Bind()
        {
            var model = _model!;
            model.NameObservable
                .Subscribe(_view.SetName)
                .DisposeWith(_bindDisposable);
            model.ColorObservable
                .Subscribe(_view.SetColor)
                .DisposeWith(_bindDisposable);
            model.AudioClipObservable
                .Subscribe(_view.SetAudioClip)
                .DisposeWith(_bindDisposable);
            model.AudioClipObservable
                .Subscribe(_view.SetSampleRange)
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
            model.RandomWeightObservable
                .Subscribe(_view.SetRandomWeight)
                .DisposeWith(_bindDisposable);
            model.PriorityObservable
                .Subscribe(_view.SetPriority)
                .DisposeWith(_bindDisposable);
            model.FadeTimeObservable
                .Subscribe(_view.SetFadeTime)
                .DisposeWith(_bindDisposable);
            model.StartSampleObservable
                .Subscribe(_view.SetStartSample)
                .DisposeWith(_bindDisposable);
            model.EndSampleObservable
                .Subscribe(_view.SetEndSample)
                .DisposeWith(_bindDisposable);
            model.IsLoopObservable
                .Subscribe(_view.SetIsLoop)
                .DisposeWith(_bindDisposable);
            model.LoopStartSampleObservable
                .Subscribe(_view.SetLoopStartSample)
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
            _view.AudioClipChangedAsObservable
                .Subscribe(value => model.AudioClip = value)
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
            _view.RandomWeightChangedAsObservable
                .Subscribe(value => model.RandomWeight = value)
                .DisposeWith(_viewEventDisposable);
            _view.PriorityChangedAsObservable
                .Subscribe(value => model.Priority = value)
                .DisposeWith(_viewEventDisposable);
            _view.FadeTimeChangedAsObservable
                .Subscribe(value => model.FadeTime = value)
                .DisposeWith(_viewEventDisposable);
            _view.StartSampleChangedAsObservable
                .Subscribe(value => model.StartSample = value)
                .DisposeWith(_viewEventDisposable);
            _view.EndSampleChangedAsObservable
                .Subscribe(value => model.EndSample = value)
                .DisposeWith(_viewEventDisposable);
            _view.IsLoopChangedAsObservable
                .Subscribe(value => model.IsLoop = value)
                .DisposeWith(_viewEventDisposable);
            _view.LoopStartSampleChangedAsObservable
                .Subscribe(value => model.LoopStartSample = value)
                .DisposeWith(_viewEventDisposable);
            _view.AnalyzeClickedAsObservable
                .Subscribe(_ => model.AnalyzeWaveChunk())
                .DisposeWith(_viewEventDisposable);
            _view.PlayRequestedAsObservable
                .Subscribe(sample => { _view.SetController(model.PlayClip(sample)); })
                .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }
    }
}
