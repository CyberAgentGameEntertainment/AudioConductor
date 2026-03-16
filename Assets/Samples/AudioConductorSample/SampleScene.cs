// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core;
using AudioConductor.Core.Models;
using AudioConductor.Core.Providers;
using UnityEngine;
using UnityEngine.UI;

namespace AudioConductor.Samples
{
    /// <summary>
    ///     Demonstrates AudioConductor v2 features:
    ///     - Two Conductors (BGM / SEVoice) with separate Settings
    ///     - Settings_BGM: dedicated BGM settings (managedPoolCapacity=2 for crossfade)
    ///     - Settings_SEVoice: shared SE+Voice settings with CategoryVolume per category
    ///     - Scene-switch BGM demo with crossfade (BGM_Field / BGM_Battle)
    ///     - Direct RegisterCueSheet (BGM, SE)
    ///     - ResourcesCueSheetProvider + RegisterCueSheetAsync (Voice)
    ///     - PlayOptions with IsLoop and FadeTime (BGM)
    ///     - PlayOneShot (SE)
    ///     - Stop with fade (BGM)
    ///     - Pause / Resume
    ///     - MasterVolume (per-Conductor) and CategoryVolume (per-Category)
    ///     - Dispose pattern
    /// </summary>
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] private AudioConductorSettings? _bgmSettings;
        [SerializeField] private AudioConductorSettings? _seVoiceSettings;

        [SerializeField] private CueSheetAsset? _bgmFieldCueSheet;
        [SerializeField] private CueSheetAsset? _bgmBattleCueSheet;
        [SerializeField] private CueSheetAsset? _seCueSheet;

        [Header("UI")]
        [SerializeField] private Text? _bgmStatusText;
        [SerializeField] private Text? _bgmCurrentText;
        [SerializeField] private Text? _voiceStatusText;
        [SerializeField] private Slider? _bgmMasterVolumeSlider;
        [SerializeField] private Text? _bgmMasterVolumeValueText;
        [SerializeField] private Slider? _bgmCategoryVolumeSlider;
        [SerializeField] private Text? _bgmCategoryVolumeValueText;
        [SerializeField] private Slider? _seVolumeSlider;
        [SerializeField] private Text? _seVolumeValueText;
        [SerializeField] private Slider? _voiceVolumeSlider;
        [SerializeField] private Text? _voiceVolumeValueText;
        [SerializeField] private Slider? _seVoiceMasterVolumeSlider;
        [SerializeField] private Text? _seVoiceMasterVolumeValueText;

        private Conductor? _bgmConductor;
        private Conductor? _seVoiceConductor;

        private CueSheetHandle _bgmFieldSheetHandle;
        private CueSheetHandle _bgmBattleSheetHandle;
        private CueSheetHandle _seSheetHandle;
        private CueSheetHandle _voiceSheetHandle;

        private PlaybackHandle _bgmPlayback;
        private PlaybackHandle _voicePlayback;

        private bool _bgmPaused;
        private bool _voicePaused;
        private string _currentBgm = "";

        private async void Start()
        {
            if (_bgmSettings == null || _seVoiceSettings == null) return;
            if (_bgmFieldCueSheet == null || _bgmBattleCueSheet == null || _seCueSheet == null) return;

            // BGM Conductor: registers both Field and Battle sheets for scene-switching
            _bgmConductor = new Conductor(_bgmSettings);
            _bgmFieldSheetHandle = _bgmConductor.RegisterCueSheet(_bgmFieldCueSheet);
            _bgmBattleSheetHandle = _bgmConductor.RegisterCueSheet(_bgmBattleCueSheet);

            // SEVoice Conductor: SE (direct) + Voice (ResourcesCueSheetProvider)
            var provider = new ResourcesCueSheetProvider();
            _seVoiceConductor = new Conductor(_seVoiceSettings, provider);
            _seSheetHandle = _seVoiceConductor.RegisterCueSheet(_seCueSheet);
            _voiceSheetHandle = await _seVoiceConductor.RegisterCueSheetAsync("CueSheets/Voice");

            if (_bgmMasterVolumeSlider != null)
                _bgmMasterVolumeSlider.onValueChanged.AddListener(OnBgmMasterVolumeChanged);
            if (_bgmCategoryVolumeSlider != null)
                _bgmCategoryVolumeSlider.onValueChanged.AddListener(OnBgmCategoryVolumeChanged);
            if (_seVolumeSlider != null)
                _seVolumeSlider.onValueChanged.AddListener(OnSeVolumeChanged);
            if (_voiceVolumeSlider != null)
                _voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
            if (_seVoiceMasterVolumeSlider != null)
                _seVoiceMasterVolumeSlider.onValueChanged.AddListener(OnSeVoiceMasterVolumeChanged);
        }

        private void Update()
        {
            if (_bgmStatusText != null)
                _bgmStatusText.text = "Status: " + GetBgmStatus();
            if (_bgmCurrentText != null)
                _bgmCurrentText.text = "Current: " + (string.IsNullOrEmpty(_currentBgm) ? "---" : _currentBgm);
            if (_voiceStatusText != null)
                _voiceStatusText.text = "Status: " + GetVoiceStatus();
        }

        private void OnDestroy()
        {
            _bgmConductor?.Dispose();
            _seVoiceConductor?.Dispose();
        }

        /// <summary>
        ///     Plays field BGM with fade-in.
        /// </summary>
        public void PlayFieldBGM()
        {
            if (_bgmConductor == null) return;
            _bgmConductor.Stop(_bgmPlayback, fadeTime: 0.5f);
            var options = new PlayOptions { FadeTime = 1.0f };
            _bgmPlayback = _bgmConductor.Play(_bgmFieldSheetHandle, "FieldBGM", options);
            _currentBgm = "Field";
            _bgmPaused = false;
        }

        /// <summary>
        ///     Plays battle BGM (looping)with fade-in.
        /// </summary>
        public void PlayBattleBGM()
        {
            if (_bgmConductor == null) return;
            _bgmConductor.Stop(_bgmPlayback, fadeTime: 0.5f);
            var options = new PlayOptions { IsLoop = true, FadeTime = 1.0f };
            _bgmPlayback = _bgmConductor.Play(_bgmBattleSheetHandle, "BattleBGM", options);
            _currentBgm = "Battle";
            _bgmPaused = false;
        }

        /// <summary>
        ///     Stops BGM with fade-out.
        /// </summary>
        public void StopBGM()
        {
            _bgmConductor?.Stop(_bgmPlayback, fadeTime: 1.0f);
            _currentBgm = "";
            _bgmPaused = false;
        }

        /// <summary>
        ///     Toggles pause/resume for BGM.
        /// </summary>
        public void PauseResumeBGM()
        {
            if (_bgmConductor == null) return;
            if (_bgmPaused)
                _bgmConductor.Resume(_bgmPlayback);
            else
                _bgmConductor.Pause(_bgmPlayback);
            _bgmPaused = !_bgmPaused;
        }

        /// <summary>
        ///     Plays SE as one-shot (random track selection).
        /// </summary>
        public void PlaySE()
        {
            _seVoiceConductor?.PlayOneShot(_seSheetHandle, "SE");
        }

        /// <summary>
        ///     Plays Voice.
        /// </summary>
        public void PlayVoice()
        {
            if (_seVoiceConductor == null) return;
            _voicePlayback = _seVoiceConductor.Play(_voiceSheetHandle, "Voice");
            _voicePaused = false;
        }

        /// <summary>
        ///     Toggles pause/resume for Voice.
        /// </summary>
        public void PauseResumeVoice()
        {
            if (_seVoiceConductor == null) return;
            if (_voicePaused)
                _seVoiceConductor.Resume(_voicePlayback);
            else
                _seVoiceConductor.Pause(_voicePlayback);
            _voicePaused = !_voicePaused;
        }

        /// <summary>
        ///     Stops Voice.
        /// </summary>
        public void StopVoice()
        {
            _seVoiceConductor?.Stop(_voicePlayback);
            _voicePaused = false;
        }

        private void OnBgmMasterVolumeChanged(float volume)
        {
            _bgmConductor?.SetMasterVolume(volume);
            if (_bgmMasterVolumeValueText != null)
                _bgmMasterVolumeValueText.text = volume.ToString("F2");
        }

        private void OnBgmCategoryVolumeChanged(float volume)
        {
            _bgmConductor?.SetCategoryVolume(0, volume);
            if (_bgmCategoryVolumeValueText != null)
                _bgmCategoryVolumeValueText.text = volume.ToString("F2");
        }

        private void OnSeVolumeChanged(float volume)
        {
            _seVoiceConductor?.SetCategoryVolume(0, volume);
            if (_seVolumeValueText != null)
                _seVolumeValueText.text = volume.ToString("F2");
        }

        private void OnVoiceVolumeChanged(float volume)
        {
            _seVoiceConductor?.SetCategoryVolume(1, volume);
            if (_voiceVolumeValueText != null)
                _voiceVolumeValueText.text = volume.ToString("F2");
        }

        private void OnSeVoiceMasterVolumeChanged(float volume)
        {
            _seVoiceConductor?.SetMasterVolume(volume);
            if (_seVoiceMasterVolumeValueText != null)
                _seVoiceMasterVolumeValueText.text = volume.ToString("F2");
        }

        private string GetBgmStatus()
        {
            if (string.IsNullOrEmpty(_currentBgm)) return "---";
            if (_bgmPaused) return "Paused";
            if (_bgmConductor != null && _bgmConductor.IsPlaying(_bgmPlayback)) return "Playing";
            return "---";
        }

        private string GetVoiceStatus()
        {
            if (_voicePaused) return "Paused";
            if (_seVoiceConductor != null && _seVoiceConductor.IsPlaying(_voicePlayback)) return "Playing";
            return "---";
        }
    }
}
