// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Threading.Tasks;
using AudioConductor.Core;
using AudioConductor.Core.Models;
using AudioConductor.Core.Providers;
using UnityEngine;

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

        private Conductor? _bgmConductor;
        private Conductor? _seConductor;
        private Conductor? _voiceConductor;

        private CueSheetHandle _bgmFieldSheetHandle;
        private CueSheetHandle _bgmBattleSheetHandle;
        private CueSheetHandle _seSheetHandle;
        private CueSheetHandle _voiceSheetHandle;

        private PlaybackHandle _bgmPlayback;

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
        }

        /// <summary>
        ///     Stops BGM with fade-out.
        /// </summary>
        public void StopBGM()
        {
            _bgmConductor?.Stop(_bgmPlayback, fadeTime: 1.0f);
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
            _voiceConductor?.Play(_voiceSheetHandle, "Voice");
        }
    }
}
