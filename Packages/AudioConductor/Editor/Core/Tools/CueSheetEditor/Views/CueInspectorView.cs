// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Shared;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueInspectorView : VisualElement, IDisposable
    {
        private readonly TextField _nameField;
        private readonly ColorDefinePopupField _colorDefinePopupField;
        private readonly PopupIntField _categoryField;
        private readonly ThrottleTypeField _throttleTypeField;
        private readonly IntegerField _throttleLimitField;
        private readonly SliderAndFloatField _volumeField;
        private readonly SliderAndFloatField _volumeRangeField;
        private readonly SliderAndFloatField _pitchField;
        private readonly SliderAndFloatField _pitchRangeField;
        private readonly Toggle _pitchInvertField;
        private readonly CuePlayTypeField _playTypeField;
        private readonly Button _playButton;
        private readonly Button _stopButton;

        private readonly Subject<string> _nameChangedSubject = new();
        private readonly Subject<string> _colorChangedSubject = new();
        private readonly Subject<int> _categoryChangedSubject = new();
        private readonly Subject<ThrottleType> _throttleTypeChangedSubject = new();
        private readonly Subject<int> _throttleLimitChangedSubject = new();
        private readonly Subject<float> _volumeChangedSubject = new();
        private readonly Subject<float> _volumeRangeChangedSubject = new();
        private readonly Subject<float> _pitchChangedSubject = new();
        private readonly Subject<float> _pitchRangeChangedSubject = new();
        private readonly Subject<bool> _pitchInvertChangedSubject = new();
        private readonly Subject<CuePlayType> _playTypeChangedSubject = new();
        private readonly Subject<Empty> _playRequestedSubject = new();
        private readonly Subject<Empty> _stopRequestedSubject = new();

        public CueInspectorView()
        {
            var tree = AssetLoader.LoadUxml("CueInspector");
            tree.CloneTree(this);

            _nameField = this.Q<TextField>("Name");
            _colorDefinePopupField = this.Q<ColorDefinePopupField>();
            _categoryField = this.Q<PopupIntField>("Category");
            _throttleTypeField = this.Q<ThrottleTypeField>();
            _throttleLimitField = this.Q<IntegerField>("ThrottleLimit");
            _volumeField = this.Q<SliderAndFloatField>("Volume");
            _volumeRangeField = this.Q<SliderAndFloatField>("VolumeRange");
            _pitchField = this.Q<SliderAndFloatField>("Pitch");
            _pitchRangeField = this.Q<SliderAndFloatField>("PitchRange");
            _pitchInvertField = this.Q<Toggle>("PitchInvert");
            _playTypeField = this.Q<CuePlayTypeField>();
            _playButton = this.Q<Button>("Play");
            _stopButton = this.Q<Button>("Stop");

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
        internal IObservable<int> CategoryChangedAsObservable => _categoryChangedSubject;
        internal IObservable<ThrottleType> ThrottleTypeChangedAsObservable => _throttleTypeChangedSubject;
        internal IObservable<int> ThrottleLimitChangedAsObservable => _throttleLimitChangedSubject;
        internal IObservable<float> VolumeChangedAsObservable => _volumeChangedSubject;
        internal IObservable<float> VolumeRangeChangedAsObservable => _volumeRangeChangedSubject;
        internal IObservable<float> PitchChangedAsObservable => _pitchChangedSubject;
        internal IObservable<float> PitchRangeChangedAsObservable => _pitchRangeChangedSubject;
        internal IObservable<bool> PitchInvertChangedAsObservable => _pitchInvertChangedSubject;
        internal IObservable<CuePlayType> PlayTypeChangedAsObservable => _playTypeChangedSubject;
        internal IObservable<Empty> PlayRequestedAsObservable => _playRequestedSubject;
        internal IObservable<Empty> StopRequestedAsObservable => _stopRequestedSubject;

        public void Dispose()
        {
            StopRequest();

            CleanupEventHandlers();
        }

        internal void Setup()
        {
            SetupEventHandlers();
        }

        internal void Open()
        {
            StopRequest();

            this.SetDisplay(true);
        }

        internal void Close()
        {
            StopRequest();

            this.SetDisplay(false);
        }

        private void SetupEventHandlers()
        {
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            _colorDefinePopupField.RegisterValueChangedCallback(OnColorChanged);
            _categoryField.RegisterValueChangedCallback(OnCategoryChanged);
            _categoryField.formatListItemCallback += CategoryListRepository.instance.GetName;
            _categoryField.formatSelectedValueCallback += CategoryListRepository.instance.GetName;
            _throttleTypeField.RegisterValueChangedCallback(OnThrottleTypeChanged);
            _throttleLimitField.RegisterValueChangedCallback(OnThrottleLimitChanged);
            _volumeField.RegisterValueChangedCallback(OnVolumeChanged);
            _volumeRangeField.RegisterValueChangedCallback(OnVolumeRangeChanged);
            _pitchField.RegisterValueChangedCallback(OnPitchChanged);
            _pitchRangeField.RegisterValueChangedCallback(OnPitchRangeChanged);
            _pitchInvertField.RegisterValueChangedCallback(OnPitchInvertChanged);
            _playTypeField.RegisterValueChangedCallback(OnPlayTypeChanged);
            _playButton.RegisterCallback<ClickEvent>(OnPlayButtonClicked);
            _stopButton.RegisterCallback<ClickEvent>(OnStopButtonClicked);
        }

        private void CleanupEventHandlers()
        {
            _stopButton.UnregisterCallback<ClickEvent>(OnStopButtonClicked);
            _playButton.UnregisterCallback<ClickEvent>(OnPlayButtonClicked);
            _playTypeField.UnregisterValueChangedCallback(OnPlayTypeChanged);
            _pitchInvertField.UnregisterValueChangedCallback(OnPitchInvertChanged);
            _pitchRangeField.UnregisterValueChangedCallback(OnPitchRangeChanged);
            _pitchField.UnregisterValueChangedCallback(OnPitchChanged);
            _volumeRangeField.UnregisterValueChangedCallback(OnVolumeRangeChanged);
            _volumeField.UnregisterValueChangedCallback(OnVolumeChanged);
            _throttleLimitField.UnregisterValueChangedCallback(OnThrottleLimitChanged);
            _throttleTypeField.UnregisterValueChangedCallback(OnThrottleTypeChanged);
            _categoryField.formatSelectedValueCallback -= CategoryListRepository.instance.GetName;
            _categoryField.formatListItemCallback -= CategoryListRepository.instance.GetName;
            _categoryField.UnregisterValueChangedCallback(OnCategoryChanged);
            _colorDefinePopupField.UnregisterValueChangedCallback(OnColorChanged);
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
        }

        private void StopRequest()
        {
            _stopRequestedSubject.OnNext(Empty.Default);
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

        internal void SetCategoryIdList(List<int> idList)
        {
            _categoryField.choices = idList;
        }

        internal void SetCategory(MixedValue<int> value)
        {
            _categoryField.showMixedValue = false;
            _categoryField.SetValueWithoutNotify(value.Value);
            _categoryField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetThrottleType(MixedValue<ThrottleType> value)
        {
            _throttleTypeField.showMixedValue = false;
            // NOTE: Unity 2021.3.23 or newer
            // When changing from multiple selection to single selection, there is a pattern where the display remains a mixed string.
            _throttleTypeField.SetValueWithoutNotify(value.HasMultipleDifferentValues 
                                                         ? null 
                                                         : value.Value);
            _throttleTypeField.showMixedValue = value.HasMultipleDifferentValues;
        }

        internal void SetThrottleLimit(MixedValue<int> value)
        {
            _throttleLimitField.showMixedValue = false;
            _throttleLimitField.ForceSetValueWithoutNotify(value.Value);
            _throttleLimitField.showMixedValue = value.HasMultipleDifferentValues;
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

        internal void SetPlayType(MixedValue<CuePlayType> value)
        {
            _playTypeField.showMixedValue = false;
            // // NOTE: Unity 2021.3.23 or newer
            // When changing from multiple selection to single selection, there is a pattern where the display remains a mixed string.
            _playTypeField.SetValueWithoutNotify(value.HasMultipleDifferentValues
                                                 ? null
                                                 : value.Value);
            _playTypeField.showMixedValue = value.HasMultipleDifferentValues;
        }

        #endregion

        #region Methods - EventHandler

        private void OnNameChanged(ChangeEvent<string> evt)
        {
#if UNITY_2022_1_OR_NEWER
            // // Even a change from a normal string to a mixed string will send an event
            if (evt.newValue == DummyTextField.mixedValueString)
                return;
#endif
            _nameChangedSubject.OnNext(evt.newValue);
        }

        private void OnColorChanged(ChangeEvent<string> evt)
            => _colorChangedSubject.OnNext(evt.newValue);

        private void OnCategoryChanged(ChangeEvent<int> evt)
            => _categoryChangedSubject.OnNext(evt.newValue);

        private void OnThrottleTypeChanged(ChangeEvent<Enum> evt)
            => _throttleTypeChangedSubject.OnNext((ThrottleType)evt.newValue);

        private void OnThrottleLimitChanged(ChangeEvent<int> evt)
            => _throttleLimitChangedSubject.OnNext(evt.newValue);

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

        private void OnPlayTypeChanged(ChangeEvent<Enum> evt)
            => _playTypeChangedSubject.OnNext((CuePlayType)evt.newValue);

        private void OnPlayButtonClicked(ClickEvent _)
            => _playRequestedSubject.OnNext(Empty.Default);

        private void OnStopButtonClicked(ClickEvent _)
            => StopRequest();

        #endregion

        #region Uxml

        public new class UxmlFactory : UxmlFactory<CueInspectorView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
