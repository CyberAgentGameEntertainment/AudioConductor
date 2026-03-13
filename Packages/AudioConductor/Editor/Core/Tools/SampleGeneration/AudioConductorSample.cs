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
using UnityEngine;

namespace AudioConductor.Editor.SampleGeneration
{
    /// <summary>
    ///     Generates an AudioConductor v2 sample demonstrating multiple Conductors,
    ///     ICueSheetProvider, CueEnumDefinition, and various playback patterns.
    /// </summary>
    internal class AudioConductorSample : ISample
    {
        private const string SoundSourcePath =
            "Packages/jp.co.cyberagent.audioconductor/Editor/PackageResources/SoundAssets";

        private const string VoiceResourcesPath = "CueSheets/Voice";

        /// <inheritdoc />
        public string SampleName => "AudioConductorSample";

        /// <inheritdoc />
        public string DisplayName => "Audio Conductor Sample";

        /// <inheritdoc />
        public SampleGenerationResult Generate()
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
                var bgmFieldSheet = CreateBgmFieldCueSheet(samplePath, result);
                var bgmBattleSheet = CreateBgmBattleCueSheet(samplePath, result);
                var seSheet = CreateSeCueSheet(samplePath, result);
                var voiceResourcesSheet = CreateVoiceResourcesCueSheet(samplePath, result);
                CreateCueEnumDefinition(samplePath, bgmFieldSheet, bgmBattleSheet, seSheet, voiceResourcesSheet,
                    result);
                CreateSampleScene(samplePath, result);
                CreateReadme(samplePath, result);

                AssetDatabase.Refresh();

                result.IsSuccess = true;
                result.SamplePath = samplePath;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Errors.Add($"Failed to generate sample: {ex.Message}");
            }

            return result;
        }

        /// <inheritdoc />
        public void Clean()
        {
            var samplePath = Path.Combine(SampleRegistry.SamplesRootPath, SampleName);
            if (AssetDatabase.IsValidFolder(samplePath))
                AssetDatabase.DeleteAsset(samplePath);
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
        ""AudioConductor""
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

        private static CueSheetAsset CreateBgmFieldCueSheet(string samplePath, SampleGenerationResult result)
        {
            var sheetPath = Path.Combine(samplePath, "BGM_Field.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "BGM_Field";

            var cue = new Cue
            {
                name = "FieldBGM",
                categoryId = 0 // BGM category
            };
            var track = new Track { name = "FieldBGM" };
            cue.trackList.Add(track);
            asset.cueSheet.cueList.Add(cue);

            AssetDatabase.CreateAsset(asset, sheetPath);
            result.CreatedFiles.Add(sheetPath);

            AssignAudioClip(asset, cue, track, samplePath, "sample_bgm_01.wav");

            return asset;
        }

        private static CueSheetAsset CreateBgmBattleCueSheet(string samplePath, SampleGenerationResult result)
        {
            var sheetPath = Path.Combine(samplePath, "BGM_Battle.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "BGM_Battle";

            var cue = new Cue
            {
                name = "BattleBGM",
                categoryId = 0 // BGM category
            };
            var track = new Track { name = "BattleBGM", isLoop = true };
            cue.trackList.Add(track);
            asset.cueSheet.cueList.Add(cue);

            AssetDatabase.CreateAsset(asset, sheetPath);
            result.CreatedFiles.Add(sheetPath);

            AssignAudioClip(asset, cue, track, samplePath, "sample_bgm_loop_01.wav");

            return asset;
        }

        private static CueSheetAsset CreateSeCueSheet(string samplePath, SampleGenerationResult result)
        {
            var sheetPath = Path.Combine(samplePath, "SE.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "SE";

            var cue = new Cue
            {
                name = "SE",
                categoryId = 1, // SE category
                playType = CuePlayType.Random
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

        private static CueSheetAsset CreateVoiceResourcesCueSheet(string samplePath, SampleGenerationResult result)
        {
            var resourcesDir = Path.Combine(samplePath, "Resources", "CueSheets");
            EnsureDirectoryExists(resourcesDir);

            var sheetPath = Path.Combine(resourcesDir, "Voice.asset");
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            asset.cueSheet.name = "Voice";

            var cue = new Cue
            {
                name = "Voice",
                categoryId = 2 // Voice category
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
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }

        private static void CreateCueEnumDefinition(
            string samplePath,
            CueSheetAsset bgmFieldSheet,
            CueSheetAsset bgmBattleSheet,
            CueSheetAsset seSheet,
            CueSheetAsset voiceResourcesSheet,
            SampleGenerationResult result)
        {
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

            // excludedEntries: Voice CueSheet excluded (loaded dynamically, no enum needed)
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

using System.Threading.Tasks;
using AudioConductor.Core;
using AudioConductor.Core.Models;
using AudioConductor.Core.Providers;
using UnityEngine;

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

        private Conductor? _bgmConductor;
        private Conductor? _seConductor;
        private Conductor? _voiceConductor;

        private CueSheetHandle _bgmFieldSheetHandle;
        private CueSheetHandle _bgmBattleSheetHandle;
        private CueSheetHandle _seSheetHandle;
        private CueSheetHandle _voiceSheetHandle;

        private PlaybackHandle _bgmPlayback;

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
        }}

        private void Update()
        {{
            _bgmConductor?.Update();
            _seConductor?.Update();
            _voiceConductor?.Update();
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
        }}

        /// <summary>
        ///     Plays battle BGM (looping) with fade-in.
        /// </summary>
        public void PlayBattleBGM()
        {{
            if (_bgmConductor == null) return;
            _bgmConductor.Stop(_bgmPlayback, fadeTime: 0.5f);
            var options = new PlayOptions {{ IsLoop = true, FadeTime = 1.0f }};
            _bgmPlayback = _bgmConductor.Play(_bgmBattleSheetHandle, ""BattleBGM"", options);
        }}

        /// <summary>
        ///     Stops BGM with fade-out.
        /// </summary>
        public void StopBGM()
        {{
            _bgmConductor?.Stop(_bgmPlayback, fadeTime: 1.0f);
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
            _voiceConductor?.Play(_voiceSheetHandle, ""Voice"");
        }}
    }}
}}
";

            File.WriteAllText(scriptPath, content);
            result.CreatedFiles.Add(scriptPath);
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

## CueEnumDefinition Usage

1. Select `CueEnumDefinition.asset` in the Project window
2. Open the CueEnumDefinition editor
3. Click **Generate** to create enum code in `Generated/`

The definition is configured as follows:
- **rootEntries**: SE CueSheet → generates `SE.cs` individually
- **fileEntries**: BGM_Field + BGM_Battle → generates `BGMCues.cs` (combined, scene-switching use case)
- **excludedEntries**: Voice CueSheet → excluded (loaded dynamically, no enum needed)
";

            File.WriteAllText(readmePath, content);
            result.CreatedFiles.Add(readmePath);
        }
    }
}
