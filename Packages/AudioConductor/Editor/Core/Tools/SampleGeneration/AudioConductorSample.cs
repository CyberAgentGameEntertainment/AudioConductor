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

namespace AudioConductor.Editor.SampleGeneration
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
            if (!EditorPrefs.GetBool(PrefKeyPending, false))
                return;

            // Clear the flag first to prevent infinite loops on repeated domain reloads.
            var samplePath = EditorPrefs.GetString(PrefKeySamplePath, string.Empty);
            var postDeploy = EditorPrefs.GetBool(PrefKeyPostDeploy, false);
            EditorPrefs.DeleteKey(PrefKeyPending);
            EditorPrefs.DeleteKey(PrefKeySamplePath);
            EditorPrefs.DeleteKey(PrefKeyPostDeploy);

            if (string.IsNullOrEmpty(samplePath))
                return;

            // Defer execution to avoid running inside a constructor context.
            EditorApplication.delayCall += () => RunPhase2(samplePath, postDeploy);
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

                CreateSettings(samplePath, result);
                var (inGameColorId, cutsceneColorId) = CreateEditorSettings(samplePath, result);
                CreateBgmFieldCueSheet(samplePath, result, inGameColorId);
                CreateBgmBattleCueSheet(samplePath, result, inGameColorId);
                CreateSeCueSheet(samplePath, result, inGameColorId);
                var voiceResourcesSheet = CreateVoiceResourcesCueSheet(samplePath, result, cutsceneColorId);
                CreateCueEnumDefinition(samplePath, voiceResourcesSheet, result);
                CreateSampleScene(samplePath, result);
                CreateReadme(samplePath, result);

                // Store phase 2 state so it can resume after domain reload triggered by AssetDatabase.Refresh().
                EditorPrefs.SetBool(PrefKeyPending, true);
                EditorPrefs.SetString(PrefKeySamplePath, samplePath);
                EditorPrefs.SetBool(PrefKeyPostDeploy, postDeploy);

                AssetDatabase.Refresh();

                result.IsSuccess = true;
                result.SamplePath = samplePath;
            }
            catch (Exception ex)
            {
                EditorPrefs.DeleteKey(PrefKeyPending);
                EditorPrefs.DeleteKey(PrefKeySamplePath);
                EditorPrefs.DeleteKey(PrefKeyPostDeploy);
                result.IsSuccess = false;
                result.Errors.Add($"Failed to generate sample: {ex.Message}");
            }

            return result;
        }

        private static void RunPhase2(string samplePath, bool postDeploy)
        {
            try
            {
                var settingsPath = Path.Combine(samplePath, "AudioConductorSettings.asset").Replace('\\', '/');
                var settings = AssetDatabase.LoadAssetAtPath<AudioConductorSettings>(settingsPath);

                var bgmFieldSheetPath = Path.Combine(samplePath, "BGM_Field.asset").Replace('\\', '/');
                var bgmFieldSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(bgmFieldSheetPath);

                var bgmBattleSheetPath = Path.Combine(samplePath, "BGM_Battle.asset").Replace('\\', '/');
                var bgmBattleSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(bgmBattleSheetPath);

                var seSheetPath = Path.Combine(samplePath, "SE.asset").Replace('\\', '/');
                var seSheet = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(seSheetPath);

                if (settings == null || bgmFieldSheet == null || bgmBattleSheet == null || seSheet == null)
                {
                    Debug.LogError(
                        "[AudioConductorSample] Phase 2 failed: required assets not found. Re-run Generate.");
                    return;
                }

                CreateSampleSceneFile(samplePath, settings, bgmFieldSheet, bgmBattleSheet, seSheet);

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

        private static void CreateSettings(string samplePath, SampleGenerationResult result)
        {
            var settingsPath = Path.Combine(samplePath, "AudioConductorSettings.asset");

            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();

            // BGM category: throttleLimit=1 (only one BGM at a time)
            settings.categoryList.Add(new Category { id = 0, name = "BGM", throttleLimit = 1 });

            // SE category: throttleType=PriorityOrder
            settings.categoryList.Add(new Category { id = 1, name = "SE", throttleType = ThrottleType.PriorityOrder });

            // Voice category: throttleLimit=1 (only one voice at a time)
            settings.categoryList.Add(new Category { id = 2, name = "Voice", throttleLimit = 1 });

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
                categoryId = 1, // SE category
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
                categoryId = 2, // Voice category
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
    {{
        [SerializeField] private AudioConductorSettings? _bgmSettings;
        [SerializeField] private AudioConductorSettings? _seSettings;
        [SerializeField] private AudioConductorSettings? _voiceSettings;

        [SerializeField] private CueSheetAsset? _bgmFieldCueSheet;
        [SerializeField] private CueSheetAsset? _bgmBattleCueSheet;
        [SerializeField] private CueSheetAsset? _seCueSheet;

        [Header(""UI"")]
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
        private string _currentBgm = """";

        private async void Start()
        {{
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
            _voiceSheetHandle = await _voiceConductor.RegisterCueSheetAsync(""{VoiceResourcesPath}"");

            if (_masterVolumeSlider != null)
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
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
            _seConductor?.Dispose();
            _voiceConductor?.Dispose();
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
            _seConductor?.PlayOneShot(_seSheetHandle, ""SE"");
        }}

        /// <summary>
        ///     Plays Voice.
        /// </summary>
        public void PlayVoice()
        {{
            if (_voiceConductor == null) return;
            _voicePlayback = _voiceConductor.Play(_voiceSheetHandle, ""Voice"");
            _voicePaused = false;
        }}

        /// <summary>
        ///     Toggles pause/resume for Voice.
        /// </summary>
        public void PauseResumeVoice()
        {{
            if (_voiceConductor == null) return;
            if (_voicePaused)
                _voiceConductor.Resume(_voicePlayback);
            else
                _voiceConductor.Pause(_voicePlayback);
            _voicePaused = !_voicePaused;
        }}

        /// <summary>
        ///     Stops Voice.
        /// </summary>
        public void StopVoice()
        {{
            _voiceConductor?.Stop(_voicePlayback);
            _voicePaused = false;
        }}

        private void OnMasterVolumeChanged(float volume)
        {{
            _bgmConductor?.SetMasterVolume(volume);
            _seConductor?.SetMasterVolume(volume);
            _voiceConductor?.SetMasterVolume(volume);
            if (_volumeValueText != null)
                _volumeValueText.text = volume.ToString(""F2"");
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
            if (_voiceConductor != null && _voiceConductor.IsPlaying(_voicePlayback)) return ""Playing"";
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
            AudioConductorSettings settings,
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

            // --- SE Column ---
            var sePanel = CreateColumnPanel(columnsPanel.transform, "SEPanel");
            CreateUIText(sePanel.transform, "SETitle", "SE Conductor", 26, FontStyle.Bold, TextAnchor.MiddleCenter, 40);
            var btnPlaySe = CreateUIButton(sePanel.transform, "PlaySEBtn", "Play SE", 22);
            CreateUIText(sePanel.transform, "SEInfo", "(Random / OneShot)", 20, FontStyle.Italic,
                TextAnchor.MiddleCenter, 30);

            // --- Voice Column ---
            var voicePanel = CreateColumnPanel(columnsPanel.transform, "VoicePanel");
            CreateUIText(voicePanel.transform, "VoiceTitle", "Voice Conductor", 26, FontStyle.Bold,
                TextAnchor.MiddleCenter, 40);
            var btnPlayVoice = CreateUIButton(voicePanel.transform, "PlayVoiceBtn", "Play Voice", 22);
            var btnPauseVoice = CreateUIButton(voicePanel.transform, "PauseResumeVoiceBtn", "Pause / Resume", 22);
            var btnStopVoice = CreateUIButton(voicePanel.transform, "StopVoiceBtn", "Stop", 22);
            var voiceStatusText = CreateUIText(voicePanel.transform, "VoiceStatus", "Status: ---", 20, FontStyle.Normal,
                TextAnchor.MiddleLeft, 30);

            // --- Volume panel ---
            var volumePanel = CreateUIPanel(mainPanel.transform, "VolumePanel");
            var volumeLayout = volumePanel.AddComponent<HorizontalLayoutGroup>();
            volumeLayout.childControlWidth = false;
            volumeLayout.childControlHeight = true;
            volumeLayout.childForceExpandWidth = false;
            volumeLayout.childForceExpandHeight = false;
            volumeLayout.spacing = 16;
            volumeLayout.padding = new RectOffset(10, 10, 5, 5);
            var volumePanelLe = volumePanel.AddComponent<LayoutElement>();
            volumePanelLe.preferredHeight = 50;

            var volumeLabel = CreateUIText(volumePanel.transform, "VolumeLabel", "Master Volume:", 22, FontStyle.Normal,
                TextAnchor.MiddleLeft, 0);
            volumeLabel.AddComponent<LayoutElement>().preferredWidth = 220;

            var sliderGo = DefaultControls.CreateSlider(new DefaultControls.Resources());
            sliderGo.name = "VolumeSlider";
            sliderGo.transform.SetParent(volumePanel.transform, false);
            var slider = sliderGo.GetComponent<Slider>();
            slider.value = 1f;
            var sliderLe = sliderGo.AddComponent<LayoutElement>();
            sliderLe.flexibleWidth = 1;
            sliderLe.preferredWidth = 400;

            var volumeValueText = CreateUIText(volumePanel.transform, "VolumeValue", "1.00", 22, FontStyle.Normal,
                TextAnchor.MiddleLeft, 0);
            volumeValueText.AddComponent<LayoutElement>().preferredWidth = 80;

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

            so.FindProperty("_bgmSettings").objectReferenceValue = settings;
            so.FindProperty("_seSettings").objectReferenceValue = settings;
            so.FindProperty("_voiceSettings").objectReferenceValue = settings;
            so.FindProperty("_bgmFieldCueSheet").objectReferenceValue = bgmFieldSheet;
            so.FindProperty("_bgmBattleCueSheet").objectReferenceValue = bgmBattleSheet;
            so.FindProperty("_seCueSheet").objectReferenceValue = seSheet;

            so.FindProperty("_bgmStatusText").objectReferenceValue = bgmStatusText.GetComponent<Text>();
            so.FindProperty("_bgmCurrentText").objectReferenceValue = bgmCurrentText.GetComponent<Text>();
            so.FindProperty("_voiceStatusText").objectReferenceValue = voiceStatusText.GetComponent<Text>();
            so.FindProperty("_masterVolumeSlider").objectReferenceValue = slider;
            so.FindProperty("_volumeValueText").objectReferenceValue = volumeValueText.GetComponent<Text>();

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

This sample demonstrates AudioConductor v2 features using multiple Conductors.

## Folder Structure

```
AudioConductorSample/
├── AudioConductor.Samples.asmdef
├── AudioConductorSettings.asset   # Shared settings (BGM / SE / Voice categories)
├── BGM_Field.asset                # Field BGM CueSheet
├── BGM_Battle.asset               # Battle BGM CueSheet (loop)
├── SE.asset                       # SE CueSheet (random playback)
├── CueEnumDefinition.asset        # Enum code generation config
├── Sound/                         # Audio clips (7 WAV files)
├── Resources/
│   └── CueSheets/
│       └── Voice.asset            # Voice CueSheet for ResourcesCueSheetProvider
├── Generated/                     # Generated enum code (after running codegen)
├── SampleScene.cs                 # MonoBehaviour demo script
├── SampleScene.unity              # Demo scene with SampleScene component wired up
└── README.md
```

## Three Conductors

| Conductor | CueSheet(s) | Registration | Playback |
|-----------|-------------|--------------|----------|
| BGM | BGM_Field.asset + BGM_Battle.asset | RegisterCueSheet (both) | PlayFieldBGM / PlayBattleBGM with FadeTime, Stop with fade |
| SE | SE.asset | RegisterCueSheet | PlayOneShot (random track) |
| Voice | Resources/CueSheets/Voice.asset | RegisterCueSheetAsync (ResourcesCueSheetProvider) | Play |

## Scene-Switching BGM Demo

BGM Conductor registers both Field and Battle sheets at startup.
Calling `PlayFieldBGM()` or `PlayBattleBGM()` fades out the current BGM and fades in the new one,
demonstrating typical scene-transition audio management.

## ResourcesCueSheetProvider Usage

Voice Conductor loads its CueSheet from `Resources/` at runtime:

```csharp
var provider = new ResourcesCueSheetProvider();
var conductor = new Conductor(voiceSettings, provider);
var handle = await conductor.RegisterCueSheetAsync(""CueSheets/Voice"");
```

## Operation UI (Canvas + UGUI)

The scene includes a Canvas-based control panel (auto-scaled via CanvasScaler).
Enter Play Mode to interact with it in the Game view:

| Section | Controls |
|---------|----------|
| **BGM Conductor** | Play Field BGM / Play Battle BGM / Pause-Resume / Stop (Fade) |
| **SE Conductor** | Play SE (Random / OneShot) |
| **Voice Conductor** | Play Voice / Pause-Resume / Stop |
| **Master Volume** | Slider (0.0 – 1.0) applied to all Conductors |

- BGM and Voice sections show real-time playback status (Playing / Paused / ---).
- Master Volume slider calls `SetMasterVolume()` on all three Conductors simultaneously.
- UI scales automatically to any screen resolution via `CanvasScaler (Scale With Screen Size)`.

## CueEnumDefinition Usage

1. Select `CueEnumDefinition.asset` in the Project window
2. Open the CueEnumDefinition editor
3. Click **Generate** to create enum code in `Generated/`

The definition is configured as follows:
- **rootEntries**: SE CueSheet → generates `SE.cs` individually
- **fileEntries**: BGM_Field + BGM_Battle → generates `BGMCues.cs` (combined, scene-switching use case)
- **excludePathRule**: `**/Resources/**` → auto-excludes CueSheets under Resources/ on import
- **excludedEntries**: Voice CueSheet → excluded (loaded dynamically, no enum needed)
";

            File.WriteAllText(readmePath, content);
            result.CreatedFiles.Add(readmePath);
        }
    }
}
