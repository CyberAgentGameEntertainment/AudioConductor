// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Shared;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEngine.UIElements;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views
{
    internal sealed class CueSheetParameterPaneView : VisualElement, IDisposable
    {
        private readonly Subject<string> _codeGenClassSuffixChangedSubject = new();
        private readonly TextField _codeGenClassSuffixField;

        private readonly Subject<bool> _codeGenEnabledChangedSubject = new();
        private readonly Toggle _codeGenEnabledField;
        private readonly Subject<string> _codeGenNamespaceChangedSubject = new();
        private readonly TextField _codeGenNamespaceField;
        private readonly Subject<string> _codeGenOutputPathChangedSubject = new();
        private readonly TextField _codeGenOutputPathField;
        private readonly VisualElement _codeGenSettingsElement;
        private readonly Subject<string> _nameChangedSubject = new();
        private readonly TextField _nameField;
        private readonly Subject<float> _pitchChangedSubject = new();
        private readonly SliderAndFloatField _pitchField;
        private readonly Subject<bool> _pitchInvertChangedSubject = new();
        private readonly Toggle _pitchInvertField;
        private readonly Subject<int> _throttleLimitChangedSubject = new();
        private readonly IntegerField _throttleLimitField;
        private readonly Subject<ThrottleType> _throttleTypeChangedSubject = new();
        private readonly ThrottleTypeField _throttleTypeField;
        private readonly Subject<float> _volumeChangedSubject = new();
        private readonly SliderAndFloatField _volumeField;

        public CueSheetParameterPaneView()
        {
            var tree = AssetLoader.LoadUxml("CueSheetParameterPane");
            tree.CloneTree(this);

            _nameField = this.Q<TextField>("Name");
            _throttleTypeField = this.Q<ThrottleTypeField>();
            _throttleLimitField = this.Q<IntegerField>("ThrottleLimit");
            _volumeField = this.Q<SliderAndFloatField>("Volume");
            _pitchField = this.Q<SliderAndFloatField>("Pitch");
            _pitchInvertField = this.Q<Toggle>("PitchInvert");

            _volumeField.lowValue = ValueRangeConst.Volume.Min;
            _volumeField.highValue = ValueRangeConst.Volume.Max;
            _pitchField.lowValue = ValueRangeConst.Pitch.Min;
            _pitchField.highValue = ValueRangeConst.Pitch.Max;

            _codeGenEnabledField = this.Q<Toggle>("CodeGenEnabled");
            _codeGenSettingsElement = this.Q<VisualElement>("CodeGenSettings");
            _codeGenOutputPathField = this.Q<TextField>("CodeGenOutputPath");
            _codeGenNamespaceField = this.Q<TextField>("CodeGenNamespace");
            _codeGenClassSuffixField = this.Q<TextField>("CodeGenClassSuffix");
        }

        internal IObservable<string> NameChangedAsObservable => _nameChangedSubject;
        internal IObservable<ThrottleType> ThrottleTypeChangedAsObservable => _throttleTypeChangedSubject;
        internal IObservable<int> ThrottleLimitChangedAsObservable => _throttleLimitChangedSubject;
        internal IObservable<float> VolumeChangedAsObservable => _volumeChangedSubject;
        internal IObservable<float> PitchChangedAsObservable => _pitchChangedSubject;
        internal IObservable<bool> PitchInvertChangedAsObservable => _pitchInvertChangedSubject;
        internal IObservable<bool> CodeGenEnabledChangedAsObservable => _codeGenEnabledChangedSubject;
        internal IObservable<string> CodeGenOutputPathChangedAsObservable => _codeGenOutputPathChangedSubject;
        internal IObservable<string> CodeGenNamespaceChangedAsObservable => _codeGenNamespaceChangedSubject;
        internal IObservable<string> CodeGenClassSuffixChangedAsObservable => _codeGenClassSuffixChangedSubject;

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
            _pitchInvertField.RegisterValueChangedCallback(OnPitchInvertChanged);
            _codeGenEnabledField.RegisterValueChangedCallback(OnCodeGenEnabledChanged);
            _codeGenOutputPathField.RegisterValueChangedCallback(OnCodeGenOutputPathChanged);
            _codeGenNamespaceField.RegisterValueChangedCallback(OnCodeGenNamespaceChanged);
            _codeGenClassSuffixField.RegisterValueChangedCallback(OnCodeGenClassSuffixChanged);
        }

        private void CleanupEventHandlers()
        {
            _codeGenClassSuffixField.UnregisterValueChangedCallback(OnCodeGenClassSuffixChanged);
            _codeGenNamespaceField.UnregisterValueChangedCallback(OnCodeGenNamespaceChanged);
            _codeGenOutputPathField.UnregisterValueChangedCallback(OnCodeGenOutputPathChanged);
            _codeGenEnabledField.UnregisterValueChangedCallback(OnCodeGenEnabledChanged);
            _pitchInvertField.UnregisterValueChangedCallback(OnPitchInvertChanged);
            _pitchField.UnregisterValueChangedCallback(OnPitchChanged);
            _volumeField.UnregisterValueChangedCallback(OnVolumeChanged);
            _throttleLimitField.UnregisterValueChangedCallback(OnThrottleLimitChanged);
            _throttleTypeField.UnregisterValueChangedCallback(OnThrottleTypeChanged);
            _nameField.UnregisterValueChangedCallback(OnNameChanged);
        }

        #region Methods - ValueSetters

        internal void SetName(string value)
        {
            _nameField.SetValueWithoutNotify(value);
        }

        internal void SetThrottleType(ThrottleType value)
        {
            _throttleTypeField.SetValueWithoutNotify(value);
        }

        internal void SetThrottleLimit(int value)
        {
            _throttleLimitField.SetValueWithoutNotify(value);
        }

        internal void SetVolume(float value)
        {
            _volumeField.SetValueWithoutNotify(value);
        }

        internal void SetPitch(float value)
        {
            _pitchField.SetValueWithoutNotify(value);
        }

        internal void SetPitchInvert(bool value)
        {
            _pitchInvertField.SetValueWithoutNotify(value);
        }

        internal void SetCodeGenEnabled(bool value)
        {
            _codeGenEnabledField.SetValueWithoutNotify(value);
            _codeGenSettingsElement.SetEnabled(value);
        }

        internal void SetCodeGenOutputPath(string value)
        {
            _codeGenOutputPathField.SetValueWithoutNotify(value);
        }

        internal void SetCodeGenNamespace(string value)
        {
            _codeGenNamespaceField.SetValueWithoutNotify(value);
        }

        internal void SetCodeGenClassSuffix(string value)
        {
            _codeGenClassSuffixField.SetValueWithoutNotify(value);
        }

        #endregion

        #region Methods - EventHandler

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            _nameChangedSubject.OnNext(evt.newValue);
        }

        private void OnThrottleTypeChanged(ChangeEvent<Enum> evt)
        {
            _throttleTypeChangedSubject.OnNext((ThrottleType)evt.newValue);
        }

        private void OnThrottleLimitChanged(ChangeEvent<int> evt)
        {
            _throttleLimitChangedSubject.OnNext(evt.newValue);
        }

        private void OnVolumeChanged(ChangeEvent<float> evt)
        {
            _volumeChangedSubject.OnNext(evt.newValue);
        }

        private void OnPitchChanged(ChangeEvent<float> evt)
        {
            _pitchChangedSubject.OnNext(evt.newValue);
        }

        private void OnPitchInvertChanged(ChangeEvent<bool> evt)
        {
            _pitchInvertChangedSubject.OnNext(evt.newValue);
        }

        private void OnCodeGenEnabledChanged(ChangeEvent<bool> evt)
        {
            _codeGenEnabledChangedSubject.OnNext(evt.newValue);
        }

        private void OnCodeGenOutputPathChanged(ChangeEvent<string> evt)
        {
            _codeGenOutputPathChangedSubject.OnNext(evt.newValue);
        }

        private void OnCodeGenNamespaceChanged(ChangeEvent<string> evt)
        {
            _codeGenNamespaceChangedSubject.OnNext(evt.newValue);
        }

        private void OnCodeGenClassSuffixChanged(ChangeEvent<string> evt)
        {
            _codeGenClassSuffixChangedSubject.OnNext(evt.newValue);
        }

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
