// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Shared;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueSheetParameterPaneView : VisualElement, IDisposable
    {
        private readonly TextField _nameField;
        private readonly ThrottleTypeField _throttleTypeField;
        private readonly IntegerField _throttleLimitField;
        private readonly SliderAndFloatField _volumeField;
        private readonly SliderAndFloatField _pitchField;
        private readonly Toggle _pitchInvertFiled;

        private readonly Subject<string> _nameChangedSubject = new();
        private readonly Subject<ThrottleType> _throttleTypeChangedSubject = new();
        private readonly Subject<int> _throttleLimitChangedSubject = new();
        private readonly Subject<float> _volumeChangedSubject = new();
        private readonly Subject<float> _pitchChangedSubject = new();
        private readonly Subject<bool> _pitchInvertChangedSubject = new();

        public CueSheetParameterPaneView()
        {
            var tree = AssetLoader.LoadUxml("CueSheetParameterPane");
            tree.CloneTree(this);

            _nameField = this.Q<TextField>("Name");
            _throttleTypeField = this.Q<ThrottleTypeField>();
            _throttleLimitField = this.Q<IntegerField>("ThrottleLimit");
            _volumeField = this.Q<SliderAndFloatField>("Volume");
            _pitchField = this.Q<SliderAndFloatField>("Pitch");
            _pitchInvertFiled = this.Q<Toggle>("PitchInvert");

            _volumeField.lowValue = ValueRangeConst.Volume.Min;
            _volumeField.highValue = ValueRangeConst.Volume.Max;
            _pitchField.lowValue = ValueRangeConst.Pitch.Min;
            _pitchField.highValue = ValueRangeConst.Pitch.Max;
        }

        internal IObservable<string> NameChangedAsObservable => _nameChangedSubject;
        internal IObservable<ThrottleType> ThrottleTypeChangedAsObservable => _throttleTypeChangedSubject;
        internal IObservable<int> ThrottleLimitChangedAsObservable => _throttleLimitChangedSubject;
        internal IObservable<float> VolumeChangedAsObservable => _volumeChangedSubject;
        internal IObservable<float> PitchChangedAsObservable => _pitchChangedSubject;
        internal IObservable<bool> PitchInvertChangedAsObservable => _pitchInvertChangedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
        }

        internal void Setup()
        {
            SetupEventHandlers();
        }

        internal void Open()
        {
            this.SetDisplay(true);
        }

        internal void Close()
        {
            this.SetDisplay(false);
        }

        private void SetupEventHandlers()
        {
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            _throttleTypeField.RegisterValueChangedCallback(OnThrottleTypeChanged);
            _throttleLimitField.RegisterValueChangedCallback(OnThrottleLimitChanged);
            _volumeField.RegisterValueChangedCallback(OnVolumeChanged);
            _pitchField.RegisterValueChangedCallback(OnPitchChanged);
            _pitchInvertFiled.RegisterValueChangedCallback(OnPitchInvertChanged);
        }

        private void CleanupEventHandlers()
        {
            _pitchInvertFiled.UnregisterValueChangedCallback(OnPitchInvertChanged);
            _pitchField.UnregisterValueChangedCallback(OnPitchChanged);
            _volumeField.UnregisterValueChangedCallback(OnVolumeChanged);
            _throttleLimitField.UnregisterValueChangedCallback(OnThrottleLimitChanged);
            _throttleTypeField.UnregisterValueChangedCallback(OnThrottleTypeChanged);
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
        }

        #region Methods - ValueSetters

        internal void SetName(string value)
            => _nameField.SetValueWithoutNotify(value);

        internal void SetThrottleType(ThrottleType value)
            => _throttleTypeField.SetValueWithoutNotify(value);

        internal void SetThrottleLimit(int value)
            => _throttleLimitField.SetValueWithoutNotify(value);

        internal void SetVolume(float value)
            => _volumeField.SetValueWithoutNotify(value);

        internal void SetPitch(float value)
            => _pitchField.SetValueWithoutNotify(value);

        internal void SetPitchInvert(bool value)
            => _pitchInvertFiled.SetValueWithoutNotify(value);

        #endregion

        #region Methods - EventHandler

        private void OnNameChanged(ChangeEvent<string> evt)
            => _nameChangedSubject.OnNext(evt.newValue);

        private void OnThrottleTypeChanged(ChangeEvent<Enum> evt)
            => _throttleTypeChangedSubject.OnNext((ThrottleType)evt.newValue);

        private void OnThrottleLimitChanged(ChangeEvent<int> evt)
            => _throttleLimitChangedSubject.OnNext(evt.newValue);

        private void OnVolumeChanged(ChangeEvent<float> evt)
            => _volumeChangedSubject.OnNext(evt.newValue);

        private void OnPitchChanged(ChangeEvent<float> evt)
            => _pitchChangedSubject.OnNext(evt.newValue);

        private void OnPitchInvertChanged(ChangeEvent<bool> evt)
            => _pitchInvertChangedSubject.OnNext(evt.newValue);

        #endregion

        #region Uxml

        public new class UxmlFactory : UxmlFactory<CueSheetParameterPaneView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }

        #endregion
    }
}
