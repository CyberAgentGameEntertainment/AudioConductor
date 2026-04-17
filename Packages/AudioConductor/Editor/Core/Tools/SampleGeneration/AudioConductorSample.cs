// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.IO;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Models;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AudioConductor.Editor.Core.Tools.SampleGeneration
{
    /// <summary>
    ///     Generates an AudioConductor v2 sample demonstrating multiple Conductors,
    ///     ICueSheetProvider, CueEnumDefinition, and various playback patterns.
    /// </summary>
    /// <remarks>
    ///     Generation is split into two phases due to Unity's domain reload:
    ///     Phase 1 generates all assets and scripts, then triggers a domain reload via AssetDatabase.Refresh().
    ///     Phase 2 runs automatically after the domain reload via [InitializeOnLoad], creates the .unity scene
    ///     file, and wires up the SampleScene component references.
    /// </remarks>
    [InitializeOnLoad]
    internal class AudioConductorSample : ISample
    {
        private const string SoundSourcePath =
            "Packages/jp.co.cyberagent.audioconductor/Editor/PackageResources/SoundAssets";

        private const string VoiceResourcesPath = "CueSheets/Voice";

        private const string PrefKeyPending = "AudioConductorSample.Phase2Pending";
        private const string PrefKeySamplePath = "AudioConductorSample.Phase2SamplePath";
        private const string PrefKeyPostDeploy = "AudioConductorSample.Phase2PostDeploy";

        static AudioConductorSample()
        {
            var pending = ConsumePendingPhase2State();
            if (!pending.HasPending)
                return;

            // Defer execution to avoid running inside a constructor context.
            EditorApplication.delayCall += () => RunPhase2(pending.SamplePath, pending.PostDeploy);
        }

        /// <inheritdoc />
        public string SampleName => "AudioConductorSample";

        /// <inheritdoc />
        public string DisplayName => "Audio Conductor Sample";

        /// <inheritdoc />
        public SampleGenerationResult Generate()
        {
            return Generate(false);
        }

        /// <inheritdoc />
        public void Clean()
        {
            var samplePath = Path.Combine(SampleRegistry.SamplesRootPath, SampleName);
            if (AssetDatabase.IsValidFolder(samplePath))
                AssetDatabase.DeleteAsset(samplePath);
        }

        /// <summary>
        ///     Runs Phase 1 of generation. Phase 2 continues automatically after domain reload.
        /// </summary>
        /// <param name="postDeploy">When true, Deploy is executed automatically after Phase 2 completes.</param>
        internal SampleGenerationResult Generate(bool postDeploy)
        {
            var result = new SampleGenerationResult();

            try
            {
                var samplePath = Path.Combine(SampleRegistry.SamplesRootPath, SampleName);

                EnsureDirectoryExists(samplePath);
                CreateAsmdef(samplePath, result);
                CopySoundAssets(samplePath, result);

                AssetDatabase.Refresh();

                CreateBgmSettings(samplePath, result);
                CreateSeVoiceSettings(samplePath, result);
                var (inGameColorId, cutsceneColorId) = CreateEditorSettings(samplePath, result);
                CreateBgmFieldCueSheet(samplePath, result, inGameColorId);
                CreateBgmBattleCueSheet(samplePath, result, inGameColorId);
                CreateSeCueSheet(samplePath, result, inGameColorId);
                var voiceResourcesSheet = CreateVoiceResourcesCueSheet(samplePath, result, cutsceneColorId);
                CreateCueEnumDefinition(samplePath, voiceResourcesSheet, result);
                CreateSampleScene(samplePath, result);
                CreateReadme(samplePath, result);

                // Store phase 2 state so it can resume after domain reload triggered by AssetDatabase.Refresh().
                SavePendingPhase2State(samplePath, postDeploy);

                AssetDatabase.Refresh();

                result.IsSuccess = true;
                result.SamplePath = samplePath;
            }
            catch (Exception ex)
            {
                ClearPendingPhase2State();
                result.IsSuccess = false;
                result.Errors.Add($"Failed to generate sample: {ex.Message}");
            }

            return result;
        }

        internal static void SavePendingPhase2State(string samplePath, bool postDeploy)
        {
            EditorPrefs.SetBool(PrefKeyPending, true);
            EditorPrefs.SetString(PrefKeySamplePath, samplePath);
            EditorPrefs.SetBool(PrefKeyPostDeploy, postDeploy);
        }

        internal static PendingPhase2State ConsumePendingPhase2State()
        {
            if (!EditorPrefs.GetBool(PrefKeyPending, false))
                return default;

            var samplePath = EditorPrefs.GetString(PrefKeySamplePath, string.Empty);
            var postDeploy = EditorPrefs.GetBool(PrefKeyPostDeploy, false);
            ClearPendingPhase2State();

            return string.IsNullOrEmpty(samplePath) ? default : new PendingPhase2State(samplePath, postDeploy);
        }

        internal static void ClearPendingPhase2State()
        {
            EditorPrefs.DeleteKey(PrefKeyPending);
            EditorPrefs.DeleteKey(PrefKeySamplePath);
            EditorPrefs.DeleteKey(PrefKeyPostDeploy);
        }

        private static void RunPhase2(string samplePath, bool postDeploy)
        {
            try
            {
                var bgmSettingsPath = Path.Combine(samplePath, "Settings_BGM.asset").Replace('\\', '/');
                var bgmSettings = AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(bgmSettingsPath);

                var seVoiceSettingsPath = Path.Combine(samplePath, "Settings_SEVoice.asset").Replace('\\', '/');
                var seVoiceSettings = AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(seVoiceSettingsPath);

                var bgmFieldSheetPath = Path.Combine(samplePath, "BGM_Field.asset").Replace('\\', '/');
                var bgmFieldSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(bgmFieldSheetPath);

                var bgmBattleSheetPath = Path.Combine(samplePath, "BGM_Battle.asset").Replace('\\', '/');
                var bgmBattleSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(bgmBattleSheetPath);

                var seSheetPath = Path.Combine(samplePath, "SE.asset").Replace('\\', '/');
                var seSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(seSheetPath);

                if (bgmSettings == null || seVoiceSettings == null ||
                    bgmFieldSheet == null || bgmBattleSheet == null || seSheet == null)
                {
                    Debug.LogError(
                        "[AudioConductorSample] Phase 2 failed: required assets not found. Re-run Generate.");
                    return;
                }

                CreateSampleSceneFile(samplePath, bgmSettings, seVoiceSettings, bgmFieldSheet, bgmBattleSheet,
                    seSheet);

                AssetDatabase.Refresh();

                Debug.Log($"[AudioConductorSample] Phase 2 complete. Scene created at {samplePath}/SampleScene.unity");

                if (postDeploy)
                    SamplesMenu.ExecuteDeployInternal();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AudioConductorSample] Phase 2 failed: {ex.Message}");
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void CreateAsmdef(string samplePath, SampleGenerationResult result)
        {
            var asmdefPath = Path.Combine(samplePath, "AudioConductor.Samples.asmdef");
            var content = @"{
    ""name"": ""AudioConductor.Samples"",
    ""rootNamespace"": ""AudioConductor.Samples"",
    ""references"": [
        ""AudioConductor"",
        ""UnityEngine.UI""
    ],
    ""includePlatforms"": [],
    ""excludePlatforms"": [],
    ""allowUnsafeCode"": false,
    ""overrideReferences"": false,
    ""precompiledReferences"": [],
    ""autoReferenced"": false,
    ""defineConstraints"": [],
    ""versionDefines"": [],
    ""noEngineReferences"": false
}";
            File.WriteAllText(asmdefPath, content);
            result.CreatedFiles.Add(asmdefPath);
        }

        private static void CopySoundAssets(string samplePath, SampleGenerationResult result)
        {
            var soundDestPath = Path.Combine(samplePath, "Sound");
            EnsureDirectoryExists(soundDestPath);

            var wavFiles = Directory.GetFiles(SoundSourcePath, "*.wav");
            foreach (var src in wavFiles)
            {
                var dest = Path.Combine(soundDestPath, Path.GetFileName(src));
                File.Copy(src, dest, true);
                result.CreatedFiles.Add(dest);
            }
        }

        private static void CreateBgmSettings(string samplePath, SampleGenerationResult result)
        {
            var settingsPath = Path.Combine(samplePath, "Settings_BGM.asset");

            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            settings.throttleType = ThrottleType.PriorityOrder;
            settings.managedPoolCapacity = 2;
            settings.oneShotPoolCapacity = 0;
            settings.deactivatePooledObjects = false;

            // BGM category: throttleLimit=2 for crossfade (fade-out + fade-in coexist)
            settings.categoryList.Add(new Category { id = 0, name = "BGM", throttleLimit = 2 });

            AssetDatabase.CreateAsset(settings, settingsPath);
            result.CreatedFiles.Add(settingsPath);
        }

        private static void CreateSeVoiceSettings(string samplePath, SampleGenerationResult result)
        {
            var settingsPath = Path.Combine(samplePath, "Settings_SEVoice.asset");

            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            settings.throttleType = ThrottleType.FirstComeFirstServed;
            settings.managedPoolCapacity = 1;
            settings.oneShotPoolCapacity = 8;
            settings.deactivatePooledObjects = true;

            // SE category: throttleType=PriorityOrder, no throttle limit
            settings.categoryList.Add(new Category
                { id = 0, name = "SE", throttleType = ThrottleType.PriorityOrder });

            // Voice category: throttleLimit=1 (only one voice at a time)
            settings.categoryList.Add(new Category { id = 1, name = "Voice", throttleLimit = 1 });

            AssetDatabase.CreateAsset(settings, settingsPath);
            result.CreatedFiles.Add(settingsPath);
        }

        private static (string inGame, string cutscene) CreateEditorSettings(string samplePath,
            SampleGenerationResult result)
        {
            var settingsPath = Path.Combine(samplePath, "AudioConductorEditorSettings.asset");

            var settings = ScriptableObject.CreateInstance<AudioConductorEditorSettings>();

            var wip = new ColorDefine { name = "WIP", color = new Color(0.9f, 0.3f, 0.3f) };
            var inGame = new ColorDefine { name = "InGame", color = new Color(0.3f, 0.5f, 0.9f) };
            var cutscene = new ColorDefine { name = "Cutscene", color = new Color(0.3f, 0.8f, 0.4f) };

            settings.colorDefineList.Add(wip);
            settings.colorDefineList.Add(inGame);
            settings.colorDefineList.Add(cutscene);

            AssetDatabase.CreateAsset(settings, settingsPath);
            result.CreatedFiles.Add(settingsPath);

            return (inGame.Id, cutscene.Id);
        }

        private static CueSheetAsset CreateBgmFieldCueSheet(string samplePath, SampleGenerationResult result,
            string colorId)
        {
            var sheetPath = Path.Combine(samplePath, "BGM_Field.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "BGM_Field";

            var cue = new Cue
            {
                name = "FieldBGM",
                categoryId = 0, // BGM category
                colorId = colorId
            };
            var track = new Track { name = "FieldBGM" };
            cue.trackList.Add(track);
            asset.cueSheet.cueList.Add(cue);

            AssetDatabase.CreateAsset(asset, sheetPath);
            result.CreatedFiles.Add(sheetPath);

            AssignAudioClip(asset, cue, track, samplePath, "sample_bgm_01.wav");

            return asset;
        }

        private static CueSheetAsset CreateBgmBattleCueSheet(string samplePath, SampleGenerationResult result,
            string colorId)
        {
            var sheetPath = Path.Combine(samplePath, "BGM_Battle.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "BGM_Battle";

            var cue = new Cue
            {
                name = "BattleBGM",
                categoryId = 0, // BGM category
                colorId = colorId
            };
            var track = new Track { name = "BattleBGM", isLoop = true };
            cue.trackList.Add(track);
            asset.cueSheet.cueList.Add(cue);

            AssetDatabase.CreateAsset(asset, sheetPath);
            result.CreatedFiles.Add(sheetPath);

            AssignAudioClip(asset, cue, track, samplePath, "sample_bgm_loop_01.wav");

            return asset;
        }

        private static CueSheetAsset CreateSeCueSheet(string samplePath, SampleGenerationResult result,
            string colorId)
        {
            var sheetPath = Path.Combine(samplePath, "SE.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "SE";

            var cue = new Cue
            {
                name = "SE",
                categoryId = 0, // SE category (id=0 in Settings_SEVoice)
                playType = CuePlayType.Random,
                colorId = colorId
            };
            cue.trackList.Add(new Track { name = "SE01" });
            cue.trackList.Add(new Track { name = "SE02" });
            cue.trackList.Add(new Track { name = "SE03" });
            asset.cueSheet.cueList.Add(cue);

            AssetDatabase.CreateAsset(asset, sheetPath);
            result.CreatedFiles.Add(sheetPath);

            AssignAudioClip(asset, cue, cue.trackList[0], samplePath, "sample_se_01.wav");
            AssignAudioClip(asset, cue, cue.trackList[1], samplePath, "sample_se_02.wav");
            AssignAudioClip(asset, cue, cue.trackList[2], samplePath, "sample_se_03.wav");

            return asset;
        }

        private static CueSheetAsset CreateVoiceResourcesCueSheet(string samplePath, SampleGenerationResult result,
            string colorId)
        {
            var resourcesDir = Path.Combine(samplePath, "Resources", "CueSheets");
            EnsureDirectoryExists(resourcesDir);

            var sheetPath = Path.Combine(resourcesDir, "Voice.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "Voice";

            var cue = new Cue
            {
                name = "Voice",
                categoryId = 1, // Voice category (id=1 in Settings_SEVoice)
                colorId = colorId
            };
            cue.trackList.Add(new Track { name = "Voice01" });
            cue.trackList.Add(new Track { name = "Voice02" });
            asset.cueSheet.cueList.Add(cue);

            AssetDatabase.CreateAsset(asset, sheetPath);
            result.CreatedFiles.Add(sheetPath);

            AssignAudioClip(asset, cue, cue.trackList[0], samplePath, "sample_vo_01.wav");
            AssignAudioClip(asset, cue, cue.trackList[1], samplePath, "sample_vo_02.wav");

            return asset;
        }

        private static void AssignAudioClip(
            CueSheetAsset asset,
            Cue cue,
            Track track,
            string samplePath,
            string wavFileName)
        {
            var clipPath = Path.Combine(samplePath, "Sound", wavFileName).Replace('\\', '/');
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
            if (clip != null)
            {
                track.audioClip = clip;
                track.endSample = clip.samples;
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }

        private static void CreateCueEnumDefinition(
            string samplePath,
            CueSheetAsset voiceResourcesSheet,
            SampleGenerationResult result)
        {
            var bgmFieldSheetPath = Path.Combine(samplePath, "BGM_Field.asset").Replace('\\', '/');
            var bgmFieldSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(bgmFieldSheetPath);
            var bgmBattleSheetPath = Path.Combine(samplePath, "BGM_Battle.asset").Replace('\\', '/');
            var bgmBattleSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(bgmBattleSheetPath);
            var seSheetPath = Path.Combine(samplePath, "SE.asset").Replace('\\', '/');
            var seSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(seSheetPath);

            var defPath = Path.Combine(samplePath, "CueEnumDefinition.asset");
            var definition = ScriptableObject.CreateInstance<CueEnumDefinition>();

            definition.defaultOutputPath = Path.Combine(samplePath, "Generated/").Replace('\\', '/');
            definition.defaultNamespace = "AudioConductor.Samples";

            // rootEntries: SE CueSheet generates an individual file
            definition.rootEntries.Add(seSheet);

            // fileEntries: BGM_Field + BGM_Battle combined into one file (realistic scene-switching use case)
            var bgmFileEntry = new FileEntry
            {
                fileName = "BGMCues",
                useDefaultOutputPath = true,
                useDefaultNamespace = true,
                useDefaultClassSuffix = true
            };
            bgmFileEntry.assets.Add(bgmFieldSheet);
            bgmFileEntry.assets.Add(bgmBattleSheet);
            definition.fileEntries.Add(bgmFileEntry);

            // excludePathRule: auto-exclude CueSheets under Resources/ on import
            definition.excludePathRule = "**/Resources/**";

            // excludedEntries: Voice CueSheet excluded (loaded dynamically, no enum needed)
            // In normal workflow, excludePathRule handles this automatically on import.
            // Here we add it explicitly because programmatic asset creation does not trigger the import rule.
            definition.excludedEntries.Add(voiceResourcesSheet);

            AssetDatabase.CreateAsset(definition, defPath);
            result.CreatedFiles.Add(defPath);
        }

        private static void CreateSampleScene(string samplePath, SampleGenerationResult result)
        {
            var scriptPath = Path.Combine(samplePath, "SampleScene.cs");

            var content =
                $@"// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core;
using AudioConductor.Core.Models;
using AudioConductor.Core.Providers;
using UnityEngine;
using UnityEngine.UI;

namespace AudioConductor.Samples
{{
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
    {{
        [SerializeField] private AudioConductorSettings? _bgmSettings;
        [SerializeField] private AudioConductorSettings? _seVoiceSettings;

        [SerializeField] private CueSheetAsset? _bgmFieldCueSheet;
        [SerializeField] private CueSheetAsset? _bgmBattleCueSheet;
        [SerializeField] private CueSheetAsset? _seCueSheet;

        [Header(""UI"")]
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
        private string _currentBgm = """";

        private async void Start()
        {{
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
            _voiceSheetHandle = await _seVoiceConductor.RegisterCueSheetAsync(""{VoiceResourcesPath}"");

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
        }}

        private void Update()
        {{
            if (_bgmStatusText != null)
                _bgmStatusText.text = ""Status: "" + GetBgmStatus();
            if (_bgmCurrentText != null)
                _bgmCurrentText.text = ""Current: "" + (string.IsNullOrEmpty(_currentBgm) ? ""---"" : _currentBgm);
            if (_voiceStatusText != null)
                _voiceStatusText.text = ""Status: "" + GetVoiceStatus();
        }}

        private void OnDestroy()
        {{
            _bgmConductor?.Dispose();
            _seVoiceConductor?.Dispose();
        }}

        /// <summary>
        ///     Plays field BGM with fade-in.
        /// </summary>
        public void PlayFieldBGM()
        {{
            if (_bgmConductor == null) return;
            _bgmConductor.Stop(_bgmPlayback, fadeTime: 0.5f);
            var options = new PlayOptions {{ FadeTime = 1.0f }};
            _bgmPlayback = _bgmConductor.Play(_bgmFieldSheetHandle, ""FieldBGM"", options);
            _currentBgm = ""Field"";
            _bgmPaused = false;
        }}

        /// <summary>
        ///     Plays battle BGM (looping)with fade-in.
        /// </summary>
        public void PlayBattleBGM()
        {{
            if (_bgmConductor == null) return;
            _bgmConductor.Stop(_bgmPlayback, fadeTime: 0.5f);
            var options = new PlayOptions {{ IsLoop = true, FadeTime = 1.0f }};
            _bgmPlayback = _bgmConductor.Play(_bgmBattleSheetHandle, ""BattleBGM"", options);
            _currentBgm = ""Battle"";
            _bgmPaused = false;
        }}

        /// <summary>
        ///     Stops BGM with fade-out.
        /// </summary>
        public void StopBGM()
        {{
            _bgmConductor?.Stop(_bgmPlayback, fadeTime: 1.0f);
            _currentBgm = """";
            _bgmPaused = false;
        }}

        /// <summary>
        ///     Toggles pause/resume for BGM.
        /// </summary>
        public void PauseResumeBGM()
        {{
            if (_bgmConductor == null) return;
            if (_bgmPaused)
                _bgmConductor.Resume(_bgmPlayback);
            else
                _bgmConductor.Pause(_bgmPlayback);
            _bgmPaused = !_bgmPaused;
        }}

        /// <summary>
        ///     Plays SE as one-shot (random track selection).
        /// </summary>
        public void PlaySE()
        {{
            _seVoiceConductor?.PlayOneShot(_seSheetHandle, ""SE"");
        }}

        /// <summary>
        ///     Plays Voice.
        /// </summary>
        public void PlayVoice()
        {{
            if (_seVoiceConductor == null) return;
            _voicePlayback = _seVoiceConductor.Play(_voiceSheetHandle, ""Voice"");
            _voicePaused = false;
        }}

        /// <summary>
        ///     Toggles pause/resume for Voice.
        /// </summary>
        public void PauseResumeVoice()
        {{
            if (_seVoiceConductor == null) return;
            if (_voicePaused)
                _seVoiceConductor.Resume(_voicePlayback);
            else
                _seVoiceConductor.Pause(_voicePlayback);
            _voicePaused = !_voicePaused;
        }}

        /// <summary>
        ///     Stops Voice.
        /// </summary>
        public void StopVoice()
        {{
            _seVoiceConductor?.Stop(_voicePlayback);
            _voicePaused = false;
        }}

        private void OnBgmMasterVolumeChanged(float volume)
        {{
            _bgmConductor?.SetMasterVolume(volume);
            if (_bgmMasterVolumeValueText != null)
                _bgmMasterVolumeValueText.text = volume.ToString(""F2"");
        }}

        private void OnBgmCategoryVolumeChanged(float volume)
        {{
            _bgmConductor?.SetCategoryVolume(0, volume);
            if (_bgmCategoryVolumeValueText != null)
                _bgmCategoryVolumeValueText.text = volume.ToString(""F2"");
        }}

        private void OnSeVolumeChanged(float volume)
        {{
            _seVoiceConductor?.SetCategoryVolume(0, volume);
            if (_seVolumeValueText != null)
                _seVolumeValueText.text = volume.ToString(""F2"");
        }}

        private void OnVoiceVolumeChanged(float volume)
        {{
            _seVoiceConductor?.SetCategoryVolume(1, volume);
            if (_voiceVolumeValueText != null)
                _voiceVolumeValueText.text = volume.ToString(""F2"");
        }}

        private void OnSeVoiceMasterVolumeChanged(float volume)
        {{
            _seVoiceConductor?.SetMasterVolume(volume);
            if (_seVoiceMasterVolumeValueText != null)
                _seVoiceMasterVolumeValueText.text = volume.ToString(""F2"");
        }}

        private string GetBgmStatus()
        {{
            if (string.IsNullOrEmpty(_currentBgm)) return ""---"";
            if (_bgmPaused) return ""Paused"";
            if (_bgmConductor != null && _bgmConductor.IsPlaying(_bgmPlayback)) return ""Playing"";
            return ""---"";
        }}

        private string GetVoiceStatus()
        {{
            if (_voicePaused) return ""Paused"";
            if (_seVoiceConductor != null && _seVoiceConductor.IsPlaying(_voicePlayback)) return ""Playing"";
            return ""---"";
        }}
    }}
}}
";

            File.WriteAllText(scriptPath, content);
            result.CreatedFiles.Add(scriptPath);
        }

        private static void CreateSampleSceneFile(
            string samplePath,
            AudioConductorSettings bgmSettings,
            AudioConductorSettings seVoiceSettings,
            CueSheetAsset bgmFieldSheet,
            CueSheetAsset bgmBattleSheet,
            CueSheetAsset seSheet)
        {
            var scenePath = Path.Combine(samplePath, "SampleScene.unity").Replace('\\', '/');
            var previousScenePath = EditorSceneManager.GetActiveScene().path;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var go = new GameObject("SampleScene");

            var sampleSceneType = Type.GetType("AudioConductor.Samples.SampleScene, AudioConductor.Samples");
            if (sampleSceneType == null)
            {
                Debug.LogWarning(
                    "[AudioConductorSample] SampleScene type not found after domain reload. Scene saved without component.");
                EditorSceneManager.SaveScene(scene, scenePath);
                if (!string.IsNullOrEmpty(previousScenePath))
                    EditorSceneManager.OpenScene(previousScenePath);
                return;
            }

            var component = (MonoBehaviour)go.AddComponent(sampleSceneType);

            // --- Build UGUI Canvas hierarchy ---
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();

            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // Main panel (full-screen, vertical layout)
            var mainPanel = CreateUIPanel(canvasGo.transform, "MainPanel");
            var mainRect = mainPanel.GetComponent<RectTransform>();
            mainRect.anchorMin = Vector2.zero;
            mainRect.anchorMax = Vector2.one;
            mainRect.offsetMin = new Vector2(30, 30);
            mainRect.offsetMax = new Vector2(-30, -30);
            var mainLayout = mainPanel.AddComponent<VerticalLayoutGroup>();
            mainLayout.childControlWidth = true;
            mainLayout.childControlHeight = true;
            mainLayout.childForceExpandWidth = true;
            mainLayout.childForceExpandHeight = false;
            mainLayout.spacing = 20;
            mainLayout.padding = new RectOffset(20, 20, 20, 20);

            // Title
            CreateUIText(mainPanel.transform, "Title", "AudioConductor v2 Sample", 36, FontStyle.Bold,
                TextAnchor.MiddleLeft, 50);

            // Columns container
            var columnsPanel = CreateUIPanel(mainPanel.transform, "Columns");
            var columnsLayout = columnsPanel.AddComponent<HorizontalLayoutGroup>();
            columnsLayout.childControlWidth = true;
            columnsLayout.childControlHeight = true;
            columnsLayout.childForceExpandWidth = true;
            columnsLayout.childForceExpandHeight = false;
            columnsLayout.spacing = 20;

            // --- BGM Column ---
            var bgmPanel = CreateColumnPanel(columnsPanel.transform, "BGMPanel");
            CreateUIText(bgmPanel.transform, "BGMTitle", "BGM Conductor", 26, FontStyle.Bold, TextAnchor.MiddleCenter,
                40);
            var btnPlayField = CreateUIButton(bgmPanel.transform, "PlayFieldBGMBtn", "Play Field BGM", 22);
            var btnPlayBattle = CreateUIButton(bgmPanel.transform, "PlayBattleBGMBtn", "Play Battle BGM", 22);
            var btnPauseBgm = CreateUIButton(bgmPanel.transform, "PauseResumeBGMBtn", "Pause / Resume", 22);
            var btnStopBgm = CreateUIButton(bgmPanel.transform, "StopBGMBtn", "Stop (Fade)", 22);
            var bgmStatusText = CreateUIText(bgmPanel.transform, "BGMStatus", "Status: ---", 20, FontStyle.Normal,
                TextAnchor.MiddleLeft, 30);
            var bgmCurrentText = CreateUIText(bgmPanel.transform, "BGMCurrent", "Current: ---", 20, FontStyle.Normal,
                TextAnchor.MiddleLeft, 30);
            var (bgmMasterVolumeSlider, bgmMasterVolumeValueText) =
                CreateVolumeSlider(bgmPanel.transform, "BgmMasterVolume", "Master Volume:");
            var (bgmCategoryVolumeSlider, bgmCategoryVolumeValueText) =
                CreateVolumeSlider(bgmPanel.transform, "BgmCategoryVolume", "Category Volume:");

            // --- SE & Voice Column ---
            var seVoicePanel = CreateColumnPanel(columnsPanel.transform, "SEVoicePanel");
            CreateUIText(seVoicePanel.transform, "SEVoiceTitle", "SE & Voice Conductor", 26, FontStyle.Bold,
                TextAnchor.MiddleCenter, 40);
            var (seVoiceMasterVolumeSlider, seVoiceMasterVolumeValueText) =
                CreateVolumeSlider(seVoicePanel.transform, "SeVoiceMasterVolume", "Master Volume:",
                    stackLabel: true);

            // SE / Voice sub-columns
            var subColumns = CreateUIPanel(seVoicePanel.transform, "SubColumns");
            var subColumnsLayout = subColumns.AddComponent<HorizontalLayoutGroup>();
            subColumnsLayout.childControlWidth = true;
            subColumnsLayout.childControlHeight = true;
            subColumnsLayout.childForceExpandWidth = true;
            subColumnsLayout.childForceExpandHeight = false;
            subColumnsLayout.spacing = 12;
            var subColumnsLe = subColumns.AddComponent<LayoutElement>();
            subColumnsLe.flexibleHeight = 1;

            // SE sub-panel
            var sePanel = CreateUIPanel(subColumns.transform, "SEPanel");
            var seLayout = sePanel.AddComponent<VerticalLayoutGroup>();
            seLayout.childControlWidth = true;
            seLayout.childControlHeight = true;
            seLayout.childForceExpandWidth = true;
            seLayout.childForceExpandHeight = false;
            seLayout.spacing = 8;
            sePanel.AddComponent<LayoutElement>().flexibleWidth = 1;

            CreateUIText(sePanel.transform, "SETitle", "SE", 22, FontStyle.Bold, TextAnchor.MiddleCenter, 30);
            var btnPlaySe = CreateUIButton(sePanel.transform, "PlaySEBtn", "Play SE", 22);
            CreateUIText(sePanel.transform, "SEInfo", "(Random / OneShot)", 20, FontStyle.Italic,
                TextAnchor.MiddleCenter, 30);
            var (seVolumeSlider, seVolumeValueText) =
                CreateVolumeSlider(sePanel.transform, "SeVolume", "Category Volume:");

            // Voice sub-panel
            var voicePanel = CreateUIPanel(subColumns.transform, "VoicePanel");
            var voiceLayout = voicePanel.AddComponent<VerticalLayoutGroup>();
            voiceLayout.childControlWidth = true;
            voiceLayout.childControlHeight = true;
            voiceLayout.childForceExpandWidth = true;
            voiceLayout.childForceExpandHeight = false;
            voiceLayout.spacing = 8;
            voicePanel.AddComponent<LayoutElement>().flexibleWidth = 1;

            CreateUIText(voicePanel.transform, "VoiceTitle", "Voice", 22, FontStyle.Bold, TextAnchor.MiddleCenter, 30);
            var btnPlayVoice = CreateUIButton(voicePanel.transform, "PlayVoiceBtn", "Play Voice", 22);
            var btnPauseVoice = CreateUIButton(voicePanel.transform, "PauseResumeVoiceBtn", "Pause / Resume", 22);
            var btnStopVoice = CreateUIButton(voicePanel.transform, "StopVoiceBtn", "Stop", 22);
            var voiceStatusText = CreateUIText(voicePanel.transform, "VoiceStatus", "Status: ---", 20, FontStyle.Normal,
                TextAnchor.MiddleLeft, 30);
            var (voiceVolumeSlider, voiceVolumeValueText) =
                CreateVolumeSlider(voicePanel.transform, "VoiceVolume", "Category Volume:");

            // --- Wire button onClick persistent listeners ---
            WireButtonOnClick(btnPlayField, component, sampleSceneType, "PlayFieldBGM");
            WireButtonOnClick(btnPlayBattle, component, sampleSceneType, "PlayBattleBGM");
            WireButtonOnClick(btnPauseBgm, component, sampleSceneType, "PauseResumeBGM");
            WireButtonOnClick(btnStopBgm, component, sampleSceneType, "StopBGM");
            WireButtonOnClick(btnPlaySe, component, sampleSceneType, "PlaySE");
            WireButtonOnClick(btnPlayVoice, component, sampleSceneType, "PlayVoice");
            WireButtonOnClick(btnPauseVoice, component, sampleSceneType, "PauseResumeVoice");
            WireButtonOnClick(btnStopVoice, component, sampleSceneType, "StopVoice");

            // --- Wire SampleScene serialized fields ---
            var so = new SerializedObject(component);

            so.FindProperty("_bgmSettings").objectReferenceValue = bgmSettings;
            so.FindProperty("_seVoiceSettings").objectReferenceValue = seVoiceSettings;
            so.FindProperty("_bgmFieldCueSheet").objectReferenceValue = bgmFieldSheet;
            so.FindProperty("_bgmBattleCueSheet").objectReferenceValue = bgmBattleSheet;
            so.FindProperty("_seCueSheet").objectReferenceValue = seSheet;

            so.FindProperty("_bgmStatusText").objectReferenceValue = bgmStatusText.GetComponent<Text>();
            so.FindProperty("_bgmCurrentText").objectReferenceValue = bgmCurrentText.GetComponent<Text>();
            so.FindProperty("_voiceStatusText").objectReferenceValue = voiceStatusText.GetComponent<Text>();
            so.FindProperty("_bgmMasterVolumeSlider").objectReferenceValue = bgmMasterVolumeSlider;
            so.FindProperty("_bgmMasterVolumeValueText").objectReferenceValue = bgmMasterVolumeValueText;
            so.FindProperty("_bgmCategoryVolumeSlider").objectReferenceValue = bgmCategoryVolumeSlider;
            so.FindProperty("_bgmCategoryVolumeValueText").objectReferenceValue = bgmCategoryVolumeValueText;
            so.FindProperty("_seVolumeSlider").objectReferenceValue = seVolumeSlider;
            so.FindProperty("_seVolumeValueText").objectReferenceValue = seVolumeValueText;
            so.FindProperty("_voiceVolumeSlider").objectReferenceValue = voiceVolumeSlider;
            so.FindProperty("_voiceVolumeValueText").objectReferenceValue = voiceVolumeValueText;
            so.FindProperty("_seVoiceMasterVolumeSlider").objectReferenceValue = seVoiceMasterVolumeSlider;
            so.FindProperty("_seVoiceMasterVolumeValueText").objectReferenceValue = seVoiceMasterVolumeValueText;

            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, scenePath);

            if (!string.IsNullOrEmpty(previousScenePath))
                EditorSceneManager.OpenScene(previousScenePath);
        }

        private static GameObject CreateUIPanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            return panel;
        }

        private static GameObject CreateColumnPanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);

            var image = panel.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.15f);

            var layout = panel.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 8;
            layout.padding = new RectOffset(12, 12, 12, 12);

            panel.AddComponent<LayoutElement>().flexibleWidth = 1;

            return panel;
        }

        private static GameObject CreateUIText(
            Transform parent,
            string name,
            string text,
            int fontSize,
            FontStyle fontStyle,
            TextAnchor alignment,
            float preferredHeight)
        {
            var textGo = DefaultControls.CreateText(new DefaultControls.Resources());
            textGo.name = name;
            textGo.transform.SetParent(parent, false);

            var textComp = textGo.GetComponent<Text>();
            textComp.text = text;
            textComp.fontSize = fontSize;
            textComp.fontStyle = fontStyle;
            textComp.alignment = alignment;
            textComp.color = Color.white;

            if (preferredHeight > 0)
            {
                var le = textGo.AddComponent<LayoutElement>();
                le.preferredHeight = preferredHeight;
            }

            return textGo;
        }

        private static GameObject CreateUIButton(Transform parent, string name, string label, int fontSize)
        {
            var buttonGo = DefaultControls.CreateButton(new DefaultControls.Resources());
            buttonGo.name = name;
            buttonGo.transform.SetParent(parent, false);

            var textComp = buttonGo.GetComponentInChildren<Text>();
            textComp.text = label;
            textComp.fontSize = fontSize;
            textComp.color = Color.black;

            var le = buttonGo.AddComponent<LayoutElement>();
            le.preferredHeight = 50;

            return buttonGo;
        }

        private static (Slider slider, Text valueText) CreateVolumeSlider(
            Transform parent,
            string name,
            string label,
            float labelWidth = 160f,
            bool stackLabel = false)
        {
            Transform sliderParent;
            if (stackLabel)
            {
                // Label above, slider row below — keeps label centered over the full width
                var outerPanel = CreateUIPanel(parent, name);
                var outerLayout = outerPanel.AddComponent<VerticalLayoutGroup>();
                outerLayout.childControlWidth = true;
                outerLayout.childControlHeight = true;
                outerLayout.childForceExpandWidth = true;
                outerLayout.childForceExpandHeight = false;
                outerLayout.spacing = 4;
                var outerLe = outerPanel.AddComponent<LayoutElement>();
                outerLe.preferredHeight = 60;
                outerLe.flexibleHeight = 0;

                CreateUIText(outerPanel.transform, name + "Label", label, 18, FontStyle.Normal,
                    TextAnchor.MiddleCenter, 24);

                var row = CreateUIPanel(outerPanel.transform, name + "Row");
                var rowLayout = row.AddComponent<HorizontalLayoutGroup>();
                rowLayout.childControlWidth = true;
                rowLayout.childControlHeight = false;
                rowLayout.childForceExpandWidth = false;
                rowLayout.childForceExpandHeight = false;
                rowLayout.spacing = 8;
                rowLayout.padding = new RectOffset(4, 4, 0, 0);
                var rowLe = row.AddComponent<LayoutElement>();
                rowLe.preferredHeight = 30;
                rowLe.flexibleHeight = 0;

                sliderParent = row.transform;
            }
            else
            {
                // Label and slider in one row
                var panel = CreateUIPanel(parent, name);
                var layout = panel.AddComponent<HorizontalLayoutGroup>();
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = false;
                layout.childForceExpandHeight = false;
                layout.spacing = 8;
                layout.padding = new RectOffset(4, 4, 2, 2);
                var panelLe = panel.AddComponent<LayoutElement>();
                panelLe.preferredHeight = 40;
                panelLe.flexibleHeight = 0;

                var labelGo = CreateUIText(panel.transform, name + "Label", label, 18, FontStyle.Normal,
                    TextAnchor.MiddleLeft, 0);
                labelGo.AddComponent<LayoutElement>().preferredWidth = labelWidth;

                sliderParent = panel.transform;
            }

            var sliderGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderGo.name = name + "Slider";
            sliderGo.transform.SetParent(sliderParent, false);
            var slider = sliderGo.GetComponent<Slider>();
            slider.value = 1f;

            // Slider visibility: DefaultControls creates Sliced images that render nothing without a sprite.
            // Switch to Simple type and apply colors.
            var bgImage = sliderGo.transform.Find("Background")?.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.type = Image.Type.Simple;
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            var fillImage = sliderGo.transform.Find("Fill Area/Fill")?.GetComponent<Image>();
            if (fillImage != null)
            {
                fillImage.type = Image.Type.Simple;
                fillImage.color = new Color(0.3f, 0.7f, 1.0f, 1f);
            }

            var handleImage = sliderGo.transform.Find("Handle Slide Area/Handle")?.GetComponent<Image>();
            if (handleImage != null)
            {
                handleImage.type = Image.Type.Simple;
                handleImage.color = Color.white;
            }

            var sliderLe = sliderGo.AddComponent<LayoutElement>();
            sliderLe.flexibleWidth = 1;
            sliderLe.preferredWidth = 200;

            var valueGo = CreateUIText(sliderParent, name + "Value", "1.00", 18, FontStyle.Normal,
                TextAnchor.MiddleLeft, 0);
            valueGo.AddComponent<LayoutElement>().preferredWidth = 50;

            return (slider, valueGo.GetComponent<Text>());
        }

        private static void WireButtonOnClick(GameObject buttonGo, MonoBehaviour target, Type targetType,
            string methodName)
        {
            var button = buttonGo.GetComponent<Button>();
            var method = targetType.GetMethod(methodName);
            if (button == null || method == null) return;

            var action = (UnityAction)Delegate.CreateDelegate(typeof(UnityAction), target, method);
            UnityEventTools.AddVoidPersistentListener(button.onClick, action);
        }

        private static void CreateReadme(string samplePath, SampleGenerationResult result)
        {
            var readmePath = Path.Combine(samplePath, "README.md");

            var content =
                @"# Audio Conductor Sample

This sample demonstrates AudioConductor v2 features:
two Conductors (BGM / SEVoice) with separate Settings,
MasterVolume and CategoryVolume control, crossfade BGM playback,
ResourcesCueSheetProvider for runtime loading,
and CueEnumDefinition for type-safe cue access.

## How to Use

1. Import this sample into your project
2. Open `SampleScene.unity`
3. Enter Play Mode
4. Use the on-screen UI to control BGM, SE, and Voice playback

## Folder Structure

```
AudioConductorSample/
├── AudioConductor.Samples.asmdef
├── Settings_BGM.asset                   # BGM-dedicated settings
├── Settings_SEVoice.asset               # Shared SE + Voice settings
├── AudioConductorEditorSettings.asset   # Editor settings (ColorDefine for CueSheet coloring)
├── BGM_Field.asset                      # Field BGM CueSheet
├── BGM_Battle.asset                     # Battle BGM CueSheet (loop)
├── SE.asset                             # SE CueSheet (random playback)
├── CueEnumDefinition.asset              # Enum code generation config
├── Sound/                               # Audio clips (7 WAV files)
├── Resources/
│   └── CueSheets/
│       └── Voice.asset                  # Voice CueSheet for ResourcesCueSheetProvider
├── Generated/                           # Generated enum code (after running codegen)
├── SampleScene.cs                       # MonoBehaviour demo script
├── SampleScene.unity                    # Demo scene with SampleScene component wired up
└── README.md
```

## Sample Structure

### Settings

This sample uses **two separate Settings** to demonstrate both dedicated and shared patterns:

| Settings | Used by | throttleType | managedPoolCapacity | oneShotPoolCapacity | deactivatePooledObjects |
|----------|---------|-------------|--------------------|--------------------|------------------------|
| `Settings_BGM.asset` | BGM Conductor | PriorityOrder | 2 | 0 | false |
| `Settings_SEVoice.asset` | SEVoice Conductor | FirstComeFirstServed | 1 | 8 | true |

**Key differences:**

- **throttleType**: BGM uses `PriorityOrder` (newer BGM replaces older); SE/Voice uses `FirstComeFirstServed` (first sound wins).
- **managedPoolCapacity**: BGM needs `2` for crossfade (fade-out + fade-in coexist simultaneously); Voice needs only `1`.
- **oneShotPoolCapacity**: BGM does not use PlayOneShot (`0`); SE uses PlayOneShot heavily (`8`).
- **deactivatePooledObjects**: BGM players stay active (`false`); SE/Voice deactivate when idle (`true`).
- **categoryList**: Each Settings defines its own Category IDs independently.

#### Why managedPoolCapacity = 2 for BGM?

When switching BGM (e.g., Field → Battle), the old BGM fades out while the new BGM fades in.
During this crossfade period, two managed players must exist simultaneously:
one for the fade-out track and one for the fade-in track.
The BGM Category's `throttleLimit = 2` also allows two sounds in the same category.

### Editor Settings

`AudioConductorEditorSettings.asset` contains three ColorDefine entries
(WIP / InGame / Cutscene) that color-code CueSheets in the editor.

### CueSheets

| CueSheet | Settings | Category (id) | Role |
|----------|----------|---------------|------|
| BGM_Field.asset | Settings_BGM | BGM (0) | Field BGM (one-shot or loop) |
| BGM_Battle.asset | Settings_BGM | BGM (0) | Battle BGM (loop) |
| SE.asset | Settings_SEVoice | SE (0) | Sound effects (random track selection) |
| Resources/CueSheets/Voice.asset | Settings_SEVoice | Voice (1) | Voice lines (loaded at runtime via ResourcesCueSheetProvider) |

> **Note:** Category IDs are scoped to each Settings. SE is `id=0` in Settings_SEVoice,
> while BGM is also `id=0` in Settings_BGM — they do not conflict.

### Scene

The scene uses two Conductors:

| Conductor | Settings | CueSheet(s) | Registration | Playback |
|-----------|----------|-------------|--------------|----------|
| BGM | Settings_BGM | BGM_Field + BGM_Battle | RegisterCueSheet (both at startup) | PlayFieldBGM / PlayBattleBGM with crossfade, Stop with fade |
| SEVoice | Settings_SEVoice | SE + Resources/CueSheets/Voice | RegisterCueSheet (SE) + RegisterCueSheetAsync (Voice) | PlayOneShot (SE), Play / Stop (Voice) |

BGM Conductor registers both Field and Battle sheets at startup.
Calling `PlayFieldBGM()` or `PlayBattleBGM()` fades out the current BGM and fades in the new one,
demonstrating typical scene-transition audio management.

SEVoice Conductor handles both SE and Voice in a single Conductor,
demonstrating MasterVolume (affects all sounds) and CategoryVolume (affects only SE or Voice).

## Operation UI (Canvas + UGUI)

The scene includes a Canvas-based control panel (auto-scaled via CanvasScaler).
Enter Play Mode to interact with it in the Game view:

| Section | Controls | Volume API |
|---------|----------|-----------|
| **BGM Conductor** | Play Field BGM / Play Battle BGM / Pause-Resume / Stop (Fade) | Master Volume → `SetMasterVolume()`, Category Volume → `SetCategoryVolume(0, volume)` |
| **SE & Voice Conductor** | (contains SE and Voice sub-panels) | Master Volume → `SetMasterVolume()` |
| SE | Play SE (Random / OneShot) | Category Volume → `SetCategoryVolume(0, volume)` |
| Voice | Play Voice / Pause-Resume / Stop | Category Volume → `SetCategoryVolume(1, volume)` |

- BGM and Voice sections show real-time playback status (Playing / Paused / ---).
- Each Conductor has both MasterVolume and CategoryVolume sliders, demonstrating the difference.
- SE and Voice share a single Conductor; individual volume is controlled via `SetCategoryVolume()`.
- UI scales automatically to any screen resolution via `CanvasScaler (Scale With Screen Size)`.
";

            File.WriteAllText(readmePath, content);
            result.CreatedFiles.Add(readmePath);
        }

        internal readonly struct PendingPhase2State
        {
            internal PendingPhase2State(string samplePath, bool postDeploy)
            {
                SamplePath = samplePath;
                PostDeploy = postDeploy;
            }

            internal string SamplePath { get; }
            internal bool PostDeploy { get; }
            internal bool HasPending => !string.IsNullOrEmpty(SamplePath);
        }
    }
}
