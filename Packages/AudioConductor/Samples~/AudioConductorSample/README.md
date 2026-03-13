# Audio Conductor Sample

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
var handle = await conductor.RegisterCueSheetAsync("CueSheets/Voice");
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
