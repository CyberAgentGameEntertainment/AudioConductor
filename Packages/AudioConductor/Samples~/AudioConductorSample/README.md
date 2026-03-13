# Audio Conductor Sample

This sample demonstrates AudioConductor v2 features:
multiple Conductors (BGM / SE / Voice) with separate Settings,
crossfade BGM playback, ResourcesCueSheetProvider for runtime loading,
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
| `Settings_SEVoice.asset` | SE + Voice Conductors | FirstComeFirstServed | 1 | 8 | true |

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

The scene uses three Conductors:

| Conductor | Settings | CueSheet(s) | Registration | Playback |
|-----------|----------|-------------|--------------|----------|
| BGM | Settings_BGM | BGM_Field + BGM_Battle | RegisterCueSheet (both at startup) | PlayFieldBGM / PlayBattleBGM with crossfade, Stop with fade |
| SE | Settings_SEVoice | SE | RegisterCueSheet | PlayOneShot (random track) |
| Voice | Settings_SEVoice | Resources/CueSheets/Voice | RegisterCueSheetAsync (ResourcesCueSheetProvider) | Play / Stop |

BGM Conductor registers both Field and Battle sheets at startup.
Calling `PlayFieldBGM()` or `PlayBattleBGM()` fades out the current BGM and fades in the new one,
demonstrating typical scene-transition audio management.

SE and Voice Conductors share `Settings_SEVoice`, demonstrating how multiple Conductors
can reference the same Settings when they have compatible requirements.

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
