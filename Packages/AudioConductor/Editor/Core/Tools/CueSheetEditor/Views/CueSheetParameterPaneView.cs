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
#if UNITY_2023_2_OR_NEWER
    [UxmlElement]
    internal sealed partial class CueSheetParameterPaneView : VisualElement, IDisposable
#else
    internal sealed class CueSheetParameterPaneView : VisualElement, IDisposable
#endif
    {
        private readonly Button _applyReferenceSampleRateButton;
        private readonly Subject<Empty> _applyReferenceSampleRateSubject = new();
        private readonly Subject<string> _nameChangedSubject = new();
        private readonly TextField _nameField;
        private readonly Subject<float> _pitchChangedSubject = new();
        private readonly SliderAndFloatField _pitchField;
        private readonly Subject<bool> _pitchInvertChangedSubject = new();
        private readonly Toggle _pitchInvertField;
        private readonly IntegerField _referenceSampleRateField;
        private readonly HelpBox _referenceSampleRateWarning;
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

            _referenceSampleRateField = this.Q<IntegerField>("ReferenceSampleRate");
            _referenceSampleRateField.SetEnabled(false);
            _referenceSampleRateWarning = this.Q<HelpBox>("ReferenceSampleRateWarning");
            _referenceSampleRateWarning.SetDisplay(false);
            _applyReferenceSampleRateButton = this.Q<Button>("ApplyReferenceSampleRateButton");
            _applyReferenceSampleRateButton.SetDisplay(false);

            ApplyTooltips();
        }

        internal IObservable<string> NameChangedAsObservable => _nameChangedSubject;
        internal IObservable<ThrottleType> ThrottleTypeChangedAsObservable => _throttleTypeChangedSubject;
        internal IObservable<int> ThrottleLimitChangedAsObservable => _throttleLimitChangedSubject;
        internal IObservable<float> VolumeChangedAsObservable => _volumeChangedSubject;
        internal IObservable<float> PitchChangedAsObservable => _pitchChangedSubject;
        internal IObservable<bool> PitchInvertChangedAsObservable => _pitchInvertChangedSubject;
        internal IObservable<Empty> ApplyReferenceSampleRateAsObservable => _applyReferenceSampleRateSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
            _applyReferenceSampleRateSubject.Dispose();
            Localization.Localization.LanguageChanged -= OnLanguageChanged;
        }

        internal void Setup()
        {
            SetupEventHandlers();
            Localization.Localization.LanguageChanged += OnLanguageChanged;
        }

        internal void Open()
        {
            this.SetDisplay(true);
        }

        internal void Close()
        {
            this.SetDisplay(false);
        }

        private void ApplyTooltips()
        {
            _nameField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.name");
            _throttleTypeField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.throttle_type");
            _throttleLimitField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.throttle_limit");
            _volumeField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.volume");
            _pitchField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.pitch");
            _pitchInvertField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.pitch_invert");
            _referenceSampleRateField.tooltip =
                Localization.Localization.Tr("cue_sheet_parameter.reference_sample_rate");
            _referenceSampleRateWarning.text =
                Localization.Localization.Tr("cue_sheet_parameter.reference_sample_rate_warning");
            _applyReferenceSampleRateButton.text =
                Localization.Localization.Tr("cue_sheet_parameter.apply_reference_sample_rate");
        }

        private void SetupEventHandlers()
        {
            _nameField.RegisterValueChangedCallback(OnNameChanged);
            _throttleTypeField.RegisterValueChangedCallback(OnThrottleTypeChanged);
            _throttleLimitField.RegisterValueChangedCallback(OnThrottleLimitChanged);
            _volumeField.RegisterValueChangedCallback(OnVolumeChanged);
            _pitchField.RegisterValueChangedCallback(OnPitchChanged);
            _pitchInvertField.RegisterValueChangedCallback(OnPitchInvertChanged);
            _applyReferenceSampleRateButton.clicked += OnApplyReferenceSampleRateClicked;
        }

        private void CleanupEventHandlers()
        {
            _applyReferenceSampleRateButton.clicked -= OnApplyReferenceSampleRateClicked;
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

        internal void SetReferenceSampleRate(int value)
        {
            _referenceSampleRateField.SetValueWithoutNotify(value);
            _referenceSampleRateWarning.SetDisplay(value == 0);
            _applyReferenceSampleRateButton.SetDisplay(value == 0);
        }

        internal void SetApplyButtonEnabled(bool enabled)
        {
            _applyReferenceSampleRateButton.SetEnabled(enabled);
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

        private void OnApplyReferenceSampleRateClicked()
        {
            _applyReferenceSampleRateSubject.OnNext(Empty.Default);
        }

        private void OnLanguageChanged()
        {
            ApplyTooltips();
        }

        #endregion

        #region Uxml

#if !UNITY_2023_2_OR_NEWER
        public new class UxmlFactory : UxmlFactory<CueSheetParameterPaneView, UxmlTraits>
        {
            public override string uxmlNamespace => "Unity.UI.Builder";
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
        }
#endif

        #endregion
    }
}
