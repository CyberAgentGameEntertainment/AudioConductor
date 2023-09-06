// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

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

        private ITrackInspectorModel _model;

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
            _model.NameObservable
                  .Subscribe(_view.SetName)
                  .DisposeWith(_bindDisposable);
            _model.ColorObservable
                  .Subscribe(_view.SetColor)
                  .DisposeWith(_bindDisposable);
            _model.AudioClipObservable
                  .Subscribe(_view.SetAudioClip)
                  .DisposeWith(_bindDisposable);
            _model.AudioClipObservable
                  .Subscribe(_view.SetSampleRange)
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
            _model.RandomWeightObservable
                  .Subscribe(_view.SetRandomWeight)
                  .DisposeWith(_bindDisposable);
            _model.PriorityObservable
                  .Subscribe(_view.SetPriority)
                  .DisposeWith(_bindDisposable);
            _model.FadeTimeObservable
                  .Subscribe(_view.SetFadeTime)
                  .DisposeWith(_bindDisposable);
            _model.StartSampleObservable
                  .Subscribe(_view.SetStartSample)
                  .DisposeWith(_bindDisposable);
            _model.EndSampleObservable
                  .Subscribe(_view.SetEndSample)
                  .DisposeWith(_bindDisposable);
            _model.IsLoopObservable
                  .Subscribe(_view.SetIsLoop)
                  .DisposeWith(_bindDisposable);
            _model.LoopStartSampleObservable
                  .Subscribe(_view.SetLoopStartSample)
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
            _view.AudioClipChangedAsObservable
                 .Subscribe(value => _model.AudioClip = value)
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
            _view.RandomWeightChangedAsObservable
                 .Subscribe(value => _model.RandomWeight = value)
                 .DisposeWith(_viewEventDisposable);
            _view.PriorityChangedAsObservable
                 .Subscribe(value => _model.Priority = value)
                 .DisposeWith(_viewEventDisposable);
            _view.FadeTimeChangedAsObservable
                 .Subscribe(value => _model.FadeTime = value)
                 .DisposeWith(_viewEventDisposable);
            _view.StartSampleChangedAsObservable
                 .Subscribe(value => _model.StartSample = value)
                 .DisposeWith(_viewEventDisposable);
            _view.EndSampleChangedAsObservable
                 .Subscribe(value => _model.EndSample = value)
                 .DisposeWith(_viewEventDisposable);
            _view.IsLoopChangedAsObservable
                 .Subscribe(value => _model.IsLoop = value)
                 .DisposeWith(_viewEventDisposable);
            _view.LoopStartSampleChangedAsObservable
                 .Subscribe(value => _model.LoopStartSample = value)
                 .DisposeWith(_viewEventDisposable);
            _view.AnalyzeClickedAsObservable
                 .Subscribe(_ => _model.AnalyzeWaveChunk())
                 .DisposeWith(_viewEventDisposable);
            _view.PlayRequestedAsObservable
                 .Subscribe(sample => { _view.SetController(_model.PlayClip(sample)); })
                 .DisposeWith(_viewEventDisposable);
        }

        private void CleanupViewEventHandlers()
        {
            _viewEventDisposable.Clear();
        }
    }
}
