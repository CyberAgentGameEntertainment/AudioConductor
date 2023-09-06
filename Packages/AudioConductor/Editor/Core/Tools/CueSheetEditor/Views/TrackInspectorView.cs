// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Shared;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed partial class TrackInspectorView : VisualElement, IDisposable
    {
        private readonly TextField _nameField;
        private readonly ColorDefinePopupField _colorDefinePopupField;
        private readonly ObjectField _audioClipField;
        private readonly SliderAndFloatField _volumeField;
        private readonly SliderAndFloatField _volumeRangeField;
        private readonly SliderAndFloatField _pitchField;
        private readonly SliderAndFloatField _pitchRangeField;
        private readonly Toggle _pitchInvertField;
        private readonly IntegerField _randomWeightField;
        private readonly IntegerField _priorityField;
        private readonly FloatField _fadeTimeField;
        private readonly SliderAndIntegerField _startSampleField;
        private readonly SliderAndIntegerField _endSampleField;
        private readonly Toggle _isLoopField;
        private readonly SliderAndIntegerField _loopStartSampleField;
        private readonly Button _analyzeButton;
        private readonly Button _playButton;
        private readonly Button _stopButton;
        private readonly IMGUIContainer _previewAreaContainer;

        private readonly Subject<string> _nameChangedSubject = new();
        private readonly Subject<string> _colorChangedSubject = new();
        private readonly Subject<float> _volumeChangedSubject = new();
        private readonly Subject<float> _volumeRangeChangedSubject = new();
        private readonly Subject<float> _pitchChangedSubject = new();
        private readonly Subject<float> _pitchRangeChangedSubject = new();
        private readonly Subject<bool> _pitchInvertChangedSubject = new();
        private readonly Subject<int> _randomWeightChangedSubject = new();
        private readonly Subject<int> _priorityChangedSubject = new();
        private readonly Subject<float> _fadeTimeChangedSubject = new();
        private readonly Subject<int> _startSampleChangedSubject = new();
        private readonly Subject<int> _endSampleChangedSubject = new();
        private readonly Subject<bool> _isLoopChangedSubject = new();
        private readonly Subject<int> _loopStartSampleChangedSubject = new();
        private readonly Subject<Empty> _analyzeClickedSubject = new();
        private readonly Subject<AudioClip> _audioClipChangedSubject = new();
        private readonly Subject<int?> _playRequestedSubject = new();

        private TrackPreviewController _previewController;

        public TrackInspectorView()
        {
            var tree = AssetLoader.LoadUxml("TrackInspector");
            tree.CloneTree(this);

            _nameField = this.Q<TextField>("Name");
            _colorDefinePopupField = this.Q<ColorDefinePopupField>();
            _audioClipField = this.Q<ObjectField>("AudioClip");
            _volumeField = this.Q<SliderAndFloatField>("Volume");
            _volumeRangeField = this.Q<SliderAndFloatField>("VolumeRange");
            _pitchField = this.Q<SliderAndFloatField>("Pitch");
            _pitchRangeField = this.Q<SliderAndFloatField>("PitchRange");
            _pitchInvertField = this.Q<Toggle>("PitchInvert");
            _randomWeightField = this.Q<IntegerField>("RandomWeight");
            _priorityField = this.Q<IntegerField>("Priority");
            _fadeTimeField = this.Q<FloatField>("FadeTime");
            _startSampleField = this.Q<SliderAndIntegerField>("StartSample");
            _endSampleField = this.Q<SliderAndIntegerField>("EndSample");
            _isLoopField = this.Q<Toggle>("Loop");
            _loopStartSampleField = this.Q<SliderAndIntegerField>("LoopStartSample");
            _analyzeButton = this.Q<Button>("Analyze");
            _playButton = this.Q<Button>("Play");
            _stopButton = this.Q<Button>("Stop");
            _previewAreaContainer = this.Q<IMGUIContainer>("Preview");

            // Smoothly moving sliders and preview
            schedule.Execute(MarkDirtyRepaint).Every((int)(1 / 60f * 1000));

            _volumeField.lowValue = ValueRangeConst.Volume.Min;
            _volumeField.highValue = ValueRangeConst.Volume.Max;
            _volumeRangeField.lowValue = ValueRangeConst.VolumeRange.Min;
            _volumeRangeField.highValue = ValueRangeConst.VolumeRange.Max;
            _pitchField.lowValue = ValueRangeConst.Pitch.Min;
            _pitchField.highValue = ValueRangeConst.Pitch.Max;
            _pitchRangeField.lowValue = ValueRangeConst.PitchRange.Min;
            _pitchRangeField.highValue = ValueRangeConst.PitchRange.Max;
        }

        internal IObservable<string> NameChangedAsObservable => _nameChangedSubject;
        internal IObservable<string> ColorChangedAsObservable => _colorChangedSubject;
        internal IObservable<AudioClip> AudioClipChangedAsObservable => _audioClipChangedSubject;
        internal IObservable<float> VolumeChangedAsObservable => _volumeChangedSubject;
        internal IObservable<float> VolumeRangeChangedAsObservable => _volumeRangeChangedSubject;
        internal IObservable<float> PitchChangedAsObservable => _pitchChangedSubject;
        internal IObservable<float> PitchRangeChangedAsObservable => _pitchRangeChangedSubject;
        internal IObservable<bool> PitchInvertChangedAsObservable => _pitchInvertChangedSubject;
        internal IObservable<int> RandomWeightChangedAsObservable => _randomWeightChangedSubject;
        internal IObservable<int> PriorityChangedAsObservable => _priorityChangedSubject;
        internal IObservable<float> FadeTimeChangedAsObservable => _fadeTimeChangedSubject;
        internal IObservable<int> StartSampleChangedAsObservable => _startSampleChangedSubject;
        internal IObservable<int> EndSampleChangedAsObservable => _endSampleChangedSubject;
        internal IObservable<bool> IsLoopChangedAsObservable => _isLoopChangedSubject;
        internal IObservable<int> LoopStartSampleChangedAsObservable => _loopStartSampleChangedSubject;
        internal IObservable<Empty> AnalyzeClickedAsObservable => _analyzeClickedSubject;
        internal IObservable<int?> PlayRequestedAsObservable => _playRequestedSubject;

        public void Dispose()
        {
            Stop();

            CleanupEventHandlers();
            _previewAreaContainer.onGUIHandler = null;
        }

        internal void Setup()
        {
            _previewAreaContainer.onGUIHandler = DrawPreviewArea;

            SetupEventHandlers();
        }

        internal void Open()
        {
            Stop();
            ResetSampleValue();

            this.SetDisplay(true);
        }

        internal void Close()
        {
            Stop();

            this.SetDisplay(false);
        }

        private void SetupEventHandlers()
        {
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            _colorDefinePopupField.RegisterValueChangedCallback(OnColorChanged);
            _audioClipField.RegisterValueChangedCallback(OnAudioClipChanged);
            _volumeField.RegisterValueChangedCallback(OnVolumeChanged);
            _volumeRangeField.RegisterValueChangedCallback(OnVolumeRangeChanged);
            _pitchField.RegisterValueChangedCallback(OnPitchChanged);
            _pitchRangeField.RegisterValueChangedCallback(OnPitchRangeChanged);
            _pitchInvertField.RegisterValueChangedCallback(OnPitchInvertChanged);
            _randomWeightField.RegisterValueChangedCallback(OnRandomWeightChanged);
            _priorityField.RegisterValueChangedCallback(OnPriorityChanged);
            _fadeTimeField.RegisterValueChangedCallback(OnFadeTimeChanged);
            _startSampleField.RegisterValueChangedCallback(OnStartSampleChanged);
            _endSampleField.RegisterValueChangedCallback(OnEndSampleChanged);
            _isLoopField.RegisterValueChangedCallback(OnIsLoopChanged);
            _loopStartSampleField.RegisterValueChangedCallback(OnLoopStartSampleChanged);
            _analyzeButton.RegisterCallback<ClickEvent>(OnAnalyzeButtonClicked);
            _playButton.RegisterCallback<ClickEvent>(OnPlayButtonClicked);
            _stopButton.RegisterCallback<ClickEvent>(OnStopButtonClicked);
        }

        private void CleanupEventHandlers()
        {
            _stopButton.UnregisterCallback<ClickEvent>(OnStopButtonClicked);
            _playButton.UnregisterCallback<ClickEvent>(OnPlayButtonClicked);
            _analyzeButton.UnregisterCallback<ClickEvent>(OnAnalyzeButtonClicked);
            _loopStartSampleField.UnregisterValueChangedCallback(OnLoopStartSampleChanged);
            _isLoopField.UnregisterValueChangedCallback(OnIsLoopChanged);
            _endSampleField.UnregisterValueChangedCallback(OnEndSampleChanged);
            _startSampleField.UnregisterValueChangedCallback(OnStartSampleChanged);
            _fadeTimeField.UnregisterValueChangedCallback(OnFadeTimeChanged);
            _priorityField.UnregisterValueChangedCallback(OnPriorityChanged);
            _randomWeightField.UnregisterValueChangedCallback(OnRandomWeightChanged);
            _pitchInvertField.UnregisterValueChangedCallback(OnPitchInvertChanged);
            _audioClipField.UnregisterValueChangedCallback(OnAudioClipChanged);
            _pitchRangeField.UnregisterValueChangedCallback(OnPitchRangeChanged);
            _pitchField.UnregisterValueChangedCallback(OnPitchChanged);
            _volumeRangeField.UnregisterValueChangedCallback(OnVolumeRangeChanged);
            _volumeField.UnregisterValueChangedCallback(OnVolumeChanged);
            _colorDefinePopupField.UnregisterValueChangedCallback(OnColorChanged);
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
        }

        internal void SetController(TrackPreviewController controller)
        {
            Stop();
            _previewController = controller;
        }

        private void Stop()
        {
            _previewController?.Dispose();
            _previewController = null;
        }

        private void ResetSampleValue()
        {
            _startSampleField.SetValueWithoutNotify(0);
            _endSampleField.SetValueWithoutNotify(0);
            _loopStartSampleField.SetValueWithoutNotify(0);
        }

        #region Methods - ValueSetters

        internal void SetName(MixedValue<string> value)
        {
            _nameField.showMixedValue = false;
            _nameField.SetValueWithoutNotify(value.Value);
            _nameField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetColor(MixedValue<string> value)
        {
            _colorDefinePopupField.showMixedValue = false;
            _colorDefinePopupField.SetValueWithoutNotify(value.Value);
            _colorDefinePopupField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetAudioClip(MixedValue<AudioClip> value)
        {
            _audioClipField.showMixedValue = false;
            _audioClipField.SetValueWithoutNotify(value.Value);
            _audioClipField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetVolume(MixedValue<float> value)
        {
            _volumeField.showMixedValue = false;
            _volumeField.SetValueWithoutNotify(value.Value);
            _volumeField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetVolumeRange(MixedValue<float> value)
        {
            _volumeRangeField.showMixedValue = false;
            _volumeRangeField.SetValueWithoutNotify(value.Value);
            _volumeRangeField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetPitch(MixedValue<float> value)
        {
            _pitchField.showMixedValue = false;
            _pitchField.SetValueWithoutNotify(value.Value);
            _pitchField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetPitchRange(MixedValue<float> value)
        {
            _pitchRangeField.showMixedValue = false;
            _pitchRangeField.SetValueWithoutNotify(value.Value);
            _pitchRangeField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetPitchInvert(MixedValue<bool> value)
        {
            _pitchInvertField.showMixedValue = false;
            _pitchInvertField.SetValueWithoutNotify(value.Value);
            _pitchInvertField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetRandomWeight(MixedValue<int> value)
        {
            _randomWeightField.showMixedValue = false;
            _randomWeightField.ForceSetValueWithoutNotify(value.Value);
            _randomWeightField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetPriority(MixedValue<int> value)
        {
            _priorityField.showMixedValue = false;
            _priorityField.ForceSetValueWithoutNotify(value.Value);
            _priorityField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetFadeTime(MixedValue<float> value)
        {
            _fadeTimeField.showMixedValue = false;
            _fadeTimeField.ForceSetValueWithoutNotify(value.Value);
            _fadeTimeField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetStartSample(MixedValue<int> value)
        {
            _startSampleField.showMixedValue = false;
            _startSampleField.SetValueWithoutNotify(value.Value);
            _startSampleField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetEndSample(MixedValue<int> value)
        {
            _endSampleField.showMixedValue = false;
            _endSampleField.SetValueWithoutNotify(value.Value);
            _endSampleField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetIsLoop(MixedValue<bool> value)
        {
            _isLoopField.showMixedValue = false;
            _isLoopField.SetValueWithoutNotify(value.Value);
            _isLoopField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetLoopStartSample(MixedValue<int> value)
        {
            _loopStartSampleField.showMixedValue = false;
            _loopStartSampleField.SetValueWithoutNotify(value.Value);
            _loopStartSampleField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetSampleRange(MixedValue<AudioClip> value)
        {
            var audioClip = value.Value;
            var samples = audioClip != null ? audioClip.samples : 0;
            _startSampleField.lowValue = ValueRangeConst.StartSample.Min;
            _startSampleField.highValue = ValueRangeConst.StartSample.Clamp(int.MaxValue, samples);
            _endSampleField.lowValue = ValueRangeConst.EndSample.Min;
            _endSampleField.highValue = ValueRangeConst.EndSample.Clamp(int.MaxValue, samples);
            _loopStartSampleField.lowValue = ValueRangeConst.LoopStartSample.Min;
            _loopStartSampleField.highValue = ValueRangeConst.LoopStartSample.Clamp(int.MaxValue, samples);
        }

        #endregion

        #region Methods - EventHandler

        private void OnNameChanged(ChangeEvent<string> evt)
        {
#if UNITY_2022_1_OR_NEWER
            // Even a change from a normal string to a mixed string will send an event
            if (evt.newValue == DummyTextField.mixedValueString)
                return;
#endif
            _nameChangedSubject.OnNext(evt.newValue);
        }

        private void OnColorChanged(ChangeEvent<string> evt)
            => _colorChangedSubject.OnNext(evt.newValue);

        private void OnAudioClipChanged(ChangeEvent<Object> evt)
            => _audioClipChangedSubject.OnNext((AudioClip)evt.newValue);

        private void OnVolumeChanged(ChangeEvent<float> evt)
            => _volumeChangedSubject.OnNext(evt.newValue);

        private void OnVolumeRangeChanged(ChangeEvent<float> evt)
            => _volumeRangeChangedSubject.OnNext(evt.newValue);

        private void OnPitchChanged(ChangeEvent<float> evt)
            => _pitchChangedSubject.OnNext(evt.newValue);

        private void OnPitchRangeChanged(ChangeEvent<float> evt)
            => _pitchRangeChangedSubject.OnNext(evt.newValue);

        private void OnPitchInvertChanged(ChangeEvent<bool> evt)
            => _pitchInvertChangedSubject.OnNext(evt.newValue);

        private void OnRandomWeightChanged(ChangeEvent<int> evt)
            => _randomWeightChangedSubject.OnNext(evt.newValue);

        private void OnPriorityChanged(ChangeEvent<int> evt)
            => _priorityChangedSubject.OnNext(evt.newValue);

        private void OnFadeTimeChanged(ChangeEvent<float> evt)
            => _fadeTimeChangedSubject.OnNext(evt.newValue);

        private void OnStartSampleChanged(ChangeEvent<int> evt)
            => _startSampleChangedSubject.OnNext(evt.newValue);

        private void OnEndSampleChanged(ChangeEvent<int> evt)
            => _endSampleChangedSubject.OnNext(evt.newValue);

        private void OnIsLoopChanged(ChangeEvent<bool> evt)
            => _isLoopChangedSubject.OnNext(evt.newValue);

        private void OnLoopStartSampleChanged(ChangeEvent<int> evt)
            => _loopStartSampleChangedSubject.OnNext(evt.newValue);

        private void OnAnalyzeButtonClicked(ClickEvent _)
            => _analyzeClickedSubject.OnNext(Empty.Default);

        private void OnPlayButtonClicked(ClickEvent _)
            => _playRequestedSubject.OnNext(null);

        private void OnStopButtonClicked(ClickEvent _)
            => Stop();

        #endregion

        #region Uxml

        public new class UxmlFactory : UxmlFactory<TrackInspectorView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
