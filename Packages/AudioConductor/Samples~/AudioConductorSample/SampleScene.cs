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
    ///     - Multiple Conductors (BGM, SE, Voice)
    ///     - Scene-switch BGM demo (BGM_Field / BGM_Battle)
    ///     - Direct RegisterCueSheet (BGM, SE)
    ///     - ResourcesCueSheetProvider + RegisterCueSheetAsync (Voice)
    ///     - PlayOptions with IsLoop and FadeTime (BGM)
    ///     - PlayOneShot (SE)
    ///     - Stop with fade (BGM)
    ///     - Pause / Resume
    ///     - Master Volume control
    ///     - Dispose pattern
    /// </summary>
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] private AudioConductorSettings? _bgmSettings;
        [SerializeField] private AudioConductorSettings? _seSettings;
        [SerializeField] private AudioConductorSettings? _voiceSettings;

        [SerializeField] private CueSheetAsset? _bgmFieldCueSheet;
        [SerializeField] private CueSheetAsset? _bgmBattleCueSheet;
        [SerializeField] private CueSheetAsset? _seCueSheet;

        [Header("UI")]
        [SerializeField] private Text? _bgmStatusText;
        [SerializeField] private Text? _bgmCurrentText;
        [SerializeField] private Text? _voiceStatusText;
        [SerializeField] private Slider? _masterVolumeSlider;
        [SerializeField] private Text? _volumeValueText;

        private Conductor? _bgmConductor;
        private Conductor? _seConductor;
        private Conductor? _voiceConductor;

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
            if (_bgmSettings == null || _seSettings == null || _voiceSettings == null) return;
            if (_bgmFieldCueSheet == null || _bgmBattleCueSheet == null || _seCueSheet == null) return;

            // BGM Conductor: registers both Field and Battle sheets for scene-switching
            _bgmConductor = new Conductor(_bgmSettings);
            _bgmFieldSheetHandle = _bgmConductor.RegisterCueSheet(_bgmFieldCueSheet);
            _bgmBattleSheetHandle = _bgmConductor.RegisterCueSheet(_bgmBattleCueSheet);

            // SE Conductor: direct RegisterCueSheet
            _seConductor = new Conductor(_seSettings);
            _seSheetHandle = _seConductor.RegisterCueSheet(_seCueSheet);

            // Voice Conductor: ResourcesCueSheetProvider + RegisterCueSheetAsync
            var provider = new ResourcesCueSheetProvider();
            _voiceConductor = new Conductor(_voiceSettings, provider);
            _voiceSheetHandle = await _voiceConductor.RegisterCueSheetAsync("CueSheets/Voice");

            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
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
            _seConductor?.Dispose();
            _voiceConductor?.Dispose();
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
            _seConductor?.PlayOneShot(_seSheetHandle, "SE");
        }

        /// <summary>
        ///     Plays Voice.
        /// </summary>
        public void PlayVoice()
        {
            if (_voiceConductor == null) return;
            _voicePlayback = _voiceConductor.Play(_voiceSheetHandle, "Voice");
            _voicePaused = false;
        }

        /// <summary>
        ///     Toggles pause/resume for Voice.
        /// </summary>
        public void PauseResumeVoice()
        {
            if (_voiceConductor == null) return;
            if (_voicePaused)
                _voiceConductor.Resume(_voicePlayback);
            else
                _voiceConductor.Pause(_voicePlayback);
            _voicePaused = !_voicePaused;
        }

        /// <summary>
        ///     Stops Voice.
        /// </summary>
        public void StopVoice()
        {
            _voiceConductor?.Stop(_voicePlayback);
            _voicePaused = false;
        }

        private void OnMasterVolumeChanged(float volume)
        {
            _bgmConductor?.SetMasterVolume(volume);
            _seConductor?.SetMasterVolume(volume);
            _voiceConductor?.SetMasterVolume(volume);
            if (_volumeValueText != null)
                _volumeValueText.text = volume.ToString("F2");
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
            if (_voiceConductor != null && _voiceConductor.IsPlaying(_voicePlayback)) return "Playing";
            return "---";
        }
    }
}
