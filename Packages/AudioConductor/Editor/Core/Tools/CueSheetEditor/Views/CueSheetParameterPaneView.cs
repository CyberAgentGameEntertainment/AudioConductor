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
        private readonly Subject<bool> _useDefaultCodeGenClassSuffixChangedSubject = new();
        private readonly Toggle _useDefaultCodeGenClassSuffixField;

        private readonly Subject<bool> _codeGenEnabledChangedSubject = new();
        private readonly Toggle _codeGenEnabledField;
        private readonly Subject<CueSheetCodeGenMode> _codeGenModeChangedSubject = new();
        private readonly CueSheetCodeGenModeField _codeGenModeField;
        private readonly Subject<string> _codeGenNamespaceChangedSubject = new();
        private readonly TextField _codeGenNamespaceField;
        private readonly Subject<string> _codeGenOutputPathChangedSubject = new();
        private readonly TextField _codeGenOutputPathField;
        private readonly Subject<bool> _useDefaultCodeGenNamespaceChangedSubject = new();
        private readonly Toggle _useDefaultCodeGenNamespaceField;
        private readonly Subject<bool> _useDefaultCodeGenOutputPathChangedSubject = new();
        private readonly Toggle _useDefaultCodeGenOutputPathField;
        private readonly Button _generateCodeButton;
        private readonly Subject<Empty> _generateCodeClickedSubject = new();
        private bool _isCodeGenEnabled;
        private CueSheetCodeGenMode _codeGenMode;
        private bool _useDefaultCodeGenClassSuffix;
        private bool _useDefaultCodeGenNamespace;
        private bool _useDefaultCodeGenOutputPath;
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
            _codeGenModeField = this.Q<CueSheetCodeGenModeField>();
            _useDefaultCodeGenOutputPathField = this.Q<Toggle>("UseDefaultCodeGenOutputPath");
            _codeGenOutputPathField = this.Q<TextField>("CodeGenOutputPath");
            _useDefaultCodeGenNamespaceField = this.Q<Toggle>("UseDefaultCodeGenNamespace");
            _codeGenNamespaceField = this.Q<TextField>("CodeGenNamespace");
            _useDefaultCodeGenClassSuffixField = this.Q<Toggle>("UseDefaultCodeGenClassSuffix");
            _codeGenClassSuffixField = this.Q<TextField>("CodeGenClassSuffix");
            _generateCodeButton = this.Q<Button>("GenerateCode");
            ApplyTooltips();
        }

        internal IObservable<string> NameChangedAsObservable => _nameChangedSubject;
        internal IObservable<ThrottleType> ThrottleTypeChangedAsObservable => _throttleTypeChangedSubject;
        internal IObservable<int> ThrottleLimitChangedAsObservable => _throttleLimitChangedSubject;
        internal IObservable<float> VolumeChangedAsObservable => _volumeChangedSubject;
        internal IObservable<float> PitchChangedAsObservable => _pitchChangedSubject;
        internal IObservable<bool> PitchInvertChangedAsObservable => _pitchInvertChangedSubject;
        internal IObservable<bool> CodeGenEnabledChangedAsObservable => _codeGenEnabledChangedSubject;
        internal IObservable<CueSheetCodeGenMode> CodeGenModeChangedAsObservable => _codeGenModeChangedSubject;
        internal IObservable<bool> UseDefaultCodeGenOutputPathChangedAsObservable => _useDefaultCodeGenOutputPathChangedSubject;
        internal IObservable<string> CodeGenOutputPathChangedAsObservable => _codeGenOutputPathChangedSubject;
        internal IObservable<bool> UseDefaultCodeGenNamespaceChangedAsObservable => _useDefaultCodeGenNamespaceChangedSubject;
        internal IObservable<string> CodeGenNamespaceChangedAsObservable => _codeGenNamespaceChangedSubject;
        internal IObservable<bool> UseDefaultCodeGenClassSuffixChangedAsObservable => _useDefaultCodeGenClassSuffixChangedSubject;
        internal IObservable<string> CodeGenClassSuffixChangedAsObservable => _codeGenClassSuffixChangedSubject;
        internal IObservable<Empty> GenerateCodeClickedAsObservable => _generateCodeClickedSubject;

        public void Dispose()
        {
            CleanupEventHandlers();
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
            _codeGenEnabledField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.codegen_enabled");
            _codeGenModeField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.codegen_mode");
            _useDefaultCodeGenOutputPathField.tooltip =
                Localization.Localization.Tr("cue_sheet_parameter.codegen_use_default_output_path");
            _codeGenOutputPathField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.codegen_output_path");
            _useDefaultCodeGenNamespaceField.tooltip =
                Localization.Localization.Tr("cue_sheet_parameter.codegen_use_default_namespace");
            _codeGenNamespaceField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.codegen_namespace");
            _useDefaultCodeGenClassSuffixField.tooltip =
                Localization.Localization.Tr("cue_sheet_parameter.codegen_use_default_class_suffix");
            _codeGenClassSuffixField.tooltip = Localization.Localization.Tr("cue_sheet_parameter.codegen_class_suffix");
            _generateCodeButton.tooltip = Localization.Localization.Tr("cue_sheet_parameter.codegen_generate");
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
            _codeGenModeField.RegisterValueChangedCallback(OnCodeGenModeChanged);
            _useDefaultCodeGenOutputPathField.RegisterValueChangedCallback(OnUseDefaultCodeGenOutputPathChanged);
            _codeGenOutputPathField.RegisterValueChangedCallback(OnCodeGenOutputPathChanged);
            _useDefaultCodeGenNamespaceField.RegisterValueChangedCallback(OnUseDefaultCodeGenNamespaceChanged);
            _codeGenNamespaceField.RegisterValueChangedCallback(OnCodeGenNamespaceChanged);
            _useDefaultCodeGenClassSuffixField.RegisterValueChangedCallback(OnUseDefaultCodeGenClassSuffixChanged);
            _codeGenClassSuffixField.RegisterValueChangedCallback(OnCodeGenClassSuffixChanged);
            _generateCodeButton.RegisterCallback<ClickEvent>(OnGenerateCodeClicked);
        }

        private void CleanupEventHandlers()
        {
            _codeGenClassSuffixField.UnregisterValueChangedCallback(OnCodeGenClassSuffixChanged);
            _useDefaultCodeGenClassSuffixField.UnregisterValueChangedCallback(OnUseDefaultCodeGenClassSuffixChanged);
            _codeGenNamespaceField.UnregisterValueChangedCallback(OnCodeGenNamespaceChanged);
            _useDefaultCodeGenNamespaceField.UnregisterValueChangedCallback(OnUseDefaultCodeGenNamespaceChanged);
            _codeGenOutputPathField.UnregisterValueChangedCallback(OnCodeGenOutputPathChanged);
            _useDefaultCodeGenOutputPathField.UnregisterValueChangedCallback(OnUseDefaultCodeGenOutputPathChanged);
            _codeGenModeField.UnregisterValueChangedCallback(OnCodeGenModeChanged);
            _codeGenEnabledField.UnregisterValueChangedCallback(OnCodeGenEnabledChanged);
            _generateCodeButton.UnregisterCallback<ClickEvent>(OnGenerateCodeClicked);
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
            _isCodeGenEnabled = value;
            _codeGenEnabledField.SetValueWithoutNotify(value);
            RefreshCodeGenState();
        }

        internal void SetCodeGenMode(CueSheetCodeGenMode value)
        {
            _codeGenMode = value;
            _codeGenModeField.SetValueWithoutNotify(value);
            RefreshCodeGenState();
        }

        internal void SetCodeGenOutputPath(string value)
        {
            _codeGenOutputPathField.SetValueWithoutNotify(value);
        }

        internal void SetUseDefaultCodeGenOutputPath(bool value)
        {
            _useDefaultCodeGenOutputPath = value;
            _useDefaultCodeGenOutputPathField.SetValueWithoutNotify(value);
            RefreshCodeGenState();
        }

        internal void SetCodeGenNamespace(string value)
        {
            _codeGenNamespaceField.SetValueWithoutNotify(value);
        }

        internal void SetUseDefaultCodeGenNamespace(bool value)
        {
            _useDefaultCodeGenNamespace = value;
            _useDefaultCodeGenNamespaceField.SetValueWithoutNotify(value);
            RefreshCodeGenState();
        }

        internal void SetCodeGenClassSuffix(string value)
        {
            _codeGenClassSuffixField.SetValueWithoutNotify(value);
        }

        internal void SetUseDefaultCodeGenClassSuffix(bool value)
        {
            _useDefaultCodeGenClassSuffix = value;
            _useDefaultCodeGenClassSuffixField.SetValueWithoutNotify(value);
            RefreshCodeGenState();
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

        private void OnCodeGenModeChanged(ChangeEvent<Enum> evt)
        {
            _codeGenModeChangedSubject.OnNext((CueSheetCodeGenMode)evt.newValue);
        }

        private void OnCodeGenOutputPathChanged(ChangeEvent<string> evt)
        {
            _codeGenOutputPathChangedSubject.OnNext(evt.newValue);
        }

        private void OnUseDefaultCodeGenOutputPathChanged(ChangeEvent<bool> evt)
        {
            _useDefaultCodeGenOutputPathChangedSubject.OnNext(evt.newValue);
        }

        private void OnCodeGenNamespaceChanged(ChangeEvent<string> evt)
        {
            _codeGenNamespaceChangedSubject.OnNext(evt.newValue);
        }

        private void OnUseDefaultCodeGenNamespaceChanged(ChangeEvent<bool> evt)
        {
            _useDefaultCodeGenNamespaceChangedSubject.OnNext(evt.newValue);
        }

        private void OnCodeGenClassSuffixChanged(ChangeEvent<string> evt)
        {
            _codeGenClassSuffixChangedSubject.OnNext(evt.newValue);
        }

        private void OnUseDefaultCodeGenClassSuffixChanged(ChangeEvent<bool> evt)
        {
            _useDefaultCodeGenClassSuffixChangedSubject.OnNext(evt.newValue);
        }

        private void OnGenerateCodeClicked(ClickEvent _)
        {
            _generateCodeClickedSubject.OnNext(Empty.Default);
        }

        private void OnLanguageChanged()
        {
            ApplyTooltips();
        }

        private void RefreshCodeGenState()
        {
            _codeGenSettingsElement.SetEnabled(_isCodeGenEnabled);
            _codeGenOutputPathField.SetEnabled(!_useDefaultCodeGenOutputPath && _isCodeGenEnabled);
            _codeGenNamespaceField.SetEnabled(!_useDefaultCodeGenNamespace && _isCodeGenEnabled);
            _codeGenClassSuffixField.SetEnabled(!_useDefaultCodeGenClassSuffix && _isCodeGenEnabled);
            _generateCodeButton.SetEnabled(_isCodeGenEnabled && _codeGenMode == CueSheetCodeGenMode.Manual);
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
