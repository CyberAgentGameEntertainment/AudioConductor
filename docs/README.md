
<p align="center">
  <img width=500 src="./Images/logo_white.png#gh-dark-mode-only" alt="AudioConductor">
  <img width=500 src="./Images/logo_color.png#gh-light-mode-only" alt="AudioConductor">
</p>

# Audio Conductor

[![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
[![license](https://img.shields.io/badge/PR-welcome-green.svg)](https://github.com/CyberAgentGameEntertainment/AudioConductor/pulls)
[![license](https://img.shields.io/badge/Unity-2022.3-green.svg)](#Requirements)

**Docs** ([English](README.md), [日本語](README_JA.md))

Audio Conductor is a tool that enhances the usability of audio features (AudioClip/AudioSource) in Unity, providing greater convenience in managing and controlling audio assets.  
You can define AudioClip and related parameters in a cue-sheet/cue/track style.  
It uses an instance-based design with the `Conductor` class, allowing you to create multiple independent audio management instances.  

## Table of Contents

<details>
<summary>Details</summary>

- [Concept](#concept)
  - [Track](#track)
  - [Cue](#cue)
  - [Cue-sheet](#cue-sheet)
  - [Runtime settings](#runtime-settings)
  - [Category](#category)
  - [Volume](#volume)
  - [Pitch](#pitch)
  - [Throttle type/Throttle limit](#throttle-typethrottle-limit)
  - [Editor settings](#editor-settings)
- [Setup](#setup)
  - [Requirements](#requirements)
  - [Install](#install)
- [Create setting assets](#create-setting-assets)
  - [Create runtime settings asset](#create-runtime-settings-asset)
  - [Create editor settings asset](#create-editor-settings-asset)
  - [Create cue-sheet assets](#create-cue-sheet-assets)
- [Edit cue-sheet](#edit-cue-sheet)
  - [Edit cue-sheet parameters](#edit-cue-sheet-parameters)
  - [Edit cues/tracks](#edit-cuestracks)
  - [Other operations](#other-operations)
    - [Export/Import](#exportimport)
- [Playback](#playback)
  - [Initialize Conductor](#initialize-conductor)
  - [Register CueSheet](#register-cuesheet)
  - [Play / PlayOneShot](#play--playoneshot)
  - [Stop / Pause / Resume](#stop--pause--resume)
  - [Volume and Pitch control](#volume-and-pitch-control)
  - [Fade control](#fade-control)
  - [Query API](#query-api)
  - [CueSheet providers](#cuesheet-providers)
  - [Dispose](#dispose)
- [Cue Enum Definition](#cue-enum-definition)
  - [Overview](#overview)
  - [Open the editor window](#open-the-editor-window)
  - [Grouping with Sheet Group](#grouping-with-sheet-group)
  - [Default settings](#default-settings)
  - [Generate / Batch generation](#generate--batch-generation)
  - [Generated code example](#generated-code-example)
- [Editor Localization](#editor-localization)
- [Samples](#samples)
  - [Import sample resources](#import-sample-resources)
  - [Sample scene](#sample-scene)
- [Migration from v1](#migration-from-v1)
  - [API mapping](#api-mapping)
  - [CueSheet asset compatibility](#cuesheet-asset-compatibility)
  - [Addressables support](#addressables-support)
- [License](#license)

</details>

## Concept

### Track

The unit of play.  
It has the following parameters:  

- Name
- AudioClip
- Volume
- Volume range
- Pitch
- Pitch range
- Start sample
- End sample
- Loop start sample
- Loop
- Random weight
- Priority
- Pitch invert
- Fade-in/fade-out time

When loop is enabled, the track will play from the start sample to the end sample and then loop back from the loop start sample to the end sample. When loop is disabled, the track will stop after playing from the start sample to the end sample.  

### Cue

An object that groups tracks.  
Playback by specifying the "name" or "cue ID" of the cue.  
It has the following parameters:  

- Name
- Cue ID
- Category ID
- Throttle type
- Throttle limit
- Volume
- Volume range
- Pitch
- Pitch range
- Pitch invert
- Play type
- Track list

There are two play types: sequential play and random play.  
Sequential play plays the track list in order from the top.  
Random play plays randomly selected tracks depending on the weight of each track.  

### Cue-sheet

An object that groups cues.  
It has the following parameters:  

- Name
- Throttle type
- Throttle limit
- Volume
- Pitch
- Pitch invert
- Cue list

### Runtime settings

It has the following parameters:  

- Throttle type
- Throttle limit
- Managed pool capacity
- One-shot pool capacity
- Deactivate pooled objects
- Category list

### Category

Any category can be defined. (e.g. BGM/SE/Voice)  
It has the following parameters:  

- Name
- Throttle type
- Throttle limit
- AudioMixerGroup

Assigning `AudioMixerGroup` to a category will set it as the output for AudioSource.  

### Volume

The volume range can be set to randomly increase or decrease the volume.  
For example, if the volume is 0.5 and the volume range is 0.2, the actual volume will be randomly determined in the range of 0.4 to 0.6. (Value range 0.00 to 1.00)  
Volume range can be set to cue/track.  
The AudioSource volume is the value calculated by multiplying the following five factors: master volume, category volume, cue-sheet volume, cue actual volume, and track actual volume.  

Master volume applies to all sounds playing in the Conductor instance. Category volume applies to sounds of the specified category within the Conductor instance.  
Each `Conductor` instance manages its own master volume and category volumes independently, even when sharing the same Settings asset.  
Additionally, fade-in/fade-out and per-playback volume adjustments (via `SetVolume`) are applied as independent multiplicative factors.  

### Pitch

The Pitch range can be set to randomly increase or decrease the pitch.  
For example, if the pitch is 1 and the pitch range is 0.02, the actual pitch will be randomly determined in the range of 0.98 to 1.02. (Value range 0.01 to 3.00)  
Pitch range can be set for a cue/track.  
The AudioSource pitch is the value calculated by multiplying the cue-sheet/cue/track actual pitch.  
If pitch Invert is enabled, the value will be a negative number.  
Additionally, per-playback pitch adjustments (via `SetPitch`) are applied as an independent multiplicative factor.  

### Throttle type/Throttle limit

Throttle limit means "Limit of concurrent play". The number of audios that are allowed to be played at the same time. (0 is unlimited).  
If a new play request is made while the limit is reached, it will be handled depending on the throttle type.  

Throttle type means "Concurrent play control type". There are two throttle types: "priority order" and "first come, first served".  
In the case of "priority order," if the priority of the new request is greater than or equal to the priority of the currently playing track, the track with the lowest priority is stopped and the new request is played. In the case of "first come, first served", the new request will be rejected.  

The evaluation is made in order of cue, cue-sheet, category, and runtime settings. Each scope is checked independently, and playback is allowed only when all scopes permit it.  

- A cue "Footstep" with throttleLimit=3 ensures at most 3 simultaneous footstep sounds, while other cues in the same cue-sheet can play independently up to their own limits.
- A cue-sheet "BGM" with throttleLimit=1 and throttleType=PriorityOrder acts as an automatic BGM switcher — playing a new BGM cue stops the currently playing one.
- A category "SE" with throttleLimit=10 caps the total number of simultaneous sound effects across all SE cues, even if individual cues have higher limits.

### Editor settings

It has the following parameters:  

- Color definition list

Color definitions consist of name and color.  
It can be associated with a cue/track when editing a cue-sheet.  
For example, "Editing: red" and "Done: green" make edit status easier to understand.  

## Setup

### Requirements

* Unity 2022.3 or higher.

### Install

1. Open the Package Manager from **Window > Package Manager**
2. **"+" button > Add package from git URL**
3. Enter the following
    * https://github.com/CyberAgentGameEntertainment/AudioConductor.git?path=/Packages/AudioConductor

<p align="center">
  <img src="./Images/install_01.png" alt="Package Manager">
</p>

Or, open Packages/manifest.json and add the following to the dependencies block.  

```json
{
    "dependencies": {
        "jp.co.cyberagent.audioconductor": "https://github.com/CyberAgentGameEntertainment/AudioConductor.git?path=/Packages/AudioConductor"
    }
}
```

If you want to set the target version, write as follows.  

* https://github.com/CyberAgentGameEntertainment/AudioConductor.git?path=/Packages/AudioConductor#2.0.0

To update the version, rewrite the version as described above.  
If you don't want to specify a version, you can also update the version by editing the hash of this library in the package-lock.json file.  

```json
{
    "dependencies": {
        "jp.co.cyberagent.audioconductor": {
            "version": "https://github.com/CyberAgentGameEntertainment/AudioConductor.git?path=/Packages/AudioConductor",
            "depth": 0,
            "source": "git",
            "dependencies": {},
            "hash": "..."
        }
    }
}
```

## Create setting assets

You Create assets from **Assets > Create > Audio Conductor**.  
This menu can also be opened from the context menu of the project view.  

<p align="center">
  <img width="70%" src="./Images/create_assets_01.png" alt="Create Assets">
</p>

### Create runtime settings asset

You create a runtime settings asset by selecting **Settings**.  
Can create more than one of this asset, but only one can be used per `Conductor` instance.  
Multiple `Conductor` instances can share the same Settings asset.  
Runtime state such as master volume and category volumes is managed independently per instance.  
You edit it in the inspector.  

<p align="center">
  <img width="70%" src="./Images/create_assets_02.png" alt="Runtime Asset">
</p>

### Create editor settings asset

You create a editor settings asset by selecting **EditorSettings**.  
Only one of this asset should be created in a project.  
You edit it in the inspector.  

<p align="center">
  <img width="70%" src="./Images/create_assets_03.png" alt="Editor Asset">
</p>

### Create cue-sheet assets

You create a cue-sheet assets by selecting **CueSheetAsset**.  
This may be created as many times as needed.  
You edit it in the editor window that open from the inspector. See [Edit cue-sheet](#edit-cue-sheet) for more information.  

<p align="center">
  <img width="70%" src="./Images/create_assets_04.png" alt="CueSheet Asset">
</p>

## Edit cue-sheet

The operation selection buttons vertically aligned on the left side switch between panes.  
From the top: [Edit cue-sheet parameters](#edit-cue-sheet-parameters), [Edit cues/tracks](#edit-cuestracks), [Other operations](#other-operations).  

At the top of the window, there is a **Settings** dropdown where you select the `AudioConductorSettings` asset to associate with this cue-sheet. The selected Settings asset provides the category list used for assigning categories to cues.  

<p align="center">
  <img src="./Images/edit_cuesheet_01.png" alt="Select Pane">
</p>

### Edit cue-sheet parameters

In this pane, you edit the cue-sheet name, concurrent play control, volume, pitch, etc.  

<p align="center">
  <img width="70%" src="./Images/edit_cuesheet_02.png" alt="Edit CueSheet Parameters">
</p>

### Edit cues/tracks

This pane consists of a multi-column list and an inspector.  
At the top of the list is a toggle button to show/hide columns and a search field.  
In this pane, you can add, delete and edit cues/tracks.  
The "Cue ID" column is available in the list, and the Cue ID is also shown in the inspector when a cue is selected. Cue ID is an integer identifier used for type-safe playback. See [Cue Enum Definition](#cue-enum-definition) for details.  

<p align="center">
  <img width="70%" src="./Images/edit_cuesheet_03.png" alt="Edit Cues/Tracks">
</p>

#### Add cues/tracks

Add a cue/track from the context menu. Tracks can only be added with the parent cue selected.  
You can also add a cue/track by drag-and-drop an AudioClip in project onto the list.  

<p align="center">
  <img src="./Images/edit_cuelist_01.png" alt="Add Cues/Tracks">
</p>

#### Remove cues/tracks

Remove cues/tracks from the context menu.  
You can also remove them with the backspace key or delete key.  

<p align="center">
  <img src="./Images/edit_cuelist_02.png" alt="Remove Cues/Tracks">
</p>

#### Edit cue/track parameters

Some parameters of cues/tracks are shown on the list.  
You can edit Values from pull-down menus or input fields.  
When cues/tracks are selected, detailed parameters are displayed in the inspector. You can also preview the cue/track.  

<p align="center">
  <img width="50%" src="./Images/edit_cuelist_03.png" alt="Edit Cues/Tracks in List">
</p>

<p align="center">
  <img width="50%" src="./Images/edit_cuelist_04.png" alt="Edit Cues/Tracks in Inspector">
</p>

### Other operations

Operations currently provided are export/import.  

<p align="center">
  <img width="70%" src="./Images/edit_cuesheet_04.png" alt="Other Operations">
</p>

#### Export/Import

You can export a cue-sheet to a csv file or import from a csv file.  
An exported csv file will be named _[CueSheetName]_.csv.  
If each value over the value range when imported, it is rounded to within the value range. AudioClips are assigned if found by `AssetDatabase.FindAssets`.  

## Playback

### Initialize Conductor

You create a `Conductor` instance with a runtime settings asset. The constructor automatically creates a DontDestroyOnLoad GameObject with an internal MonoBehaviour, so no manual update call is required.  

```cs
var settings = Resources.Load<AudioConductorSettings>("Settings");
var conductor = new Conductor(settings);
```

### Register CueSheet

You register a cue-sheet asset to a `Conductor` instance. The return value is a `CueSheetHandle`, which is used to specify the cue-sheet in subsequent operations.  

```cs
var cueSheetAsset = Resources.Load<CueSheetAsset>("CueSheet");
var handle = conductor.RegisterCueSheet(cueSheetAsset);
```

### Play / PlayOneShot

`Play` starts playback of a cue specified by name or cue ID. The return value is a `PlaybackHandle`, which can be used to control the playing audio.  
You can pass `PlayOptions` to customize the playback behavior (e.g. loop, track selection, fade-in).  

```cs
// Play by cue name
var playback = conductor.Play(handle, "CueName");

// Play by cue ID
var playback = conductor.Play(handle, cueId);

// Play with options
var playback = conductor.Play(handle, "CueName", new PlayOptions
{
    IsLoop = true,
    FadeTime = 1.0f
});

// Play a specific track by index
var playback = conductor.Play(handle, "CueName", new PlayOptions
{
    TrackIndex = 2
});

// Play a specific track by name
var playback = conductor.Play(handle, "CueName", new PlayOptions
{
    TrackName = "TrackName"
});
```

You can also provide a custom `ITrackSelector` via `PlayOptions.Selector` to override the cue's configured track selection logic.  

`PlayOneShot` is a fire-and-forget method that plays the cue once without returning a handle.  

```cs
conductor.PlayOneShot(handle, "CueName");
conductor.PlayOneShot(handle, cueId);
```

### Stop / Pause / Resume

You control the playing audio by specifying the `PlaybackHandle`.  
`StopAll(fadeTime: ...)` applies fade-out only to managed playbacks started with `Play`; OneShot playbacks started with `PlayOneShot` are always stopped immediately.  

```cs
// Stop
conductor.Stop(playback);

// Stop with fade-out
conductor.Stop(playback, fadeTime: 1.0f);

// Stop all playing audios immediately
conductor.StopAll();

// Fade out all managed playbacks, then stop them
// OneShot playbacks are still stopped immediately
conductor.StopAll(fadeTime: 1.0f);

// Pause / Resume
conductor.Pause(playback);
conductor.Resume(playback);
```

### Volume and Pitch control

You can control volume and pitch at multiple levels:  
- Per-playback: Adjusts the volume/pitch of a single playing sound.
- Master volume: Adjusts the volume of all sounds in the Conductor instance.
- Category volume: Adjusts the volume of all sounds belonging to a specific category in the Conductor instance.

```cs
// Per-playback volume and pitch
conductor.SetVolume(playback, 0.5f);
conductor.SetPitch(playback, 1.2f);

// Master volume
conductor.SetMasterVolume(0.8f);
var masterVolume = conductor.GetMasterVolume();

// Category volume
conductor.SetCategoryVolume(categoryId, 0.5f);
var categoryVolume = conductor.GetCategoryVolume(categoryId);
```

### Fade control

You can specify fade-in/fade-out time and a custom fader when playing or stopping audio.  
The `IFader` interface allows you to define custom fade curves. A linear fader is provided as `Faders.Linear`.  

```cs
// Play with fade-in
var playback = conductor.Play(handle, "CueName", new PlayOptions
{
    FadeTime = 1.0f,
    Fader = Faders.Linear
});

// Stop with fade-out using a custom fader
conductor.Stop(playback, fadeTime: 2.0f, fader: Faders.Linear);
```

### Query API

You can query information about registered cue-sheets, cues, and tracks.  

```cs
// Get all registered cue-sheet infos
List<CueSheetInfo> sheetInfos = conductor.GetCueSheetInfos();

// Get cue infos for a cue-sheet
List<CueInfo> cueInfos = conductor.GetCueInfos(handle);

// Get track infos for a cue
List<TrackInfo> trackInfos = conductor.GetTrackInfos(handle, "CueName");

// Lookup cue ID by name (and vice versa)
int? cueId = conductor.GetCueId(handle, "CueName");
string? cueName = cueId.HasValue ? conductor.GetCueName(handle, cueId.Value) : null;

// Check if a playback is currently playing
bool isPlaying = conductor.IsPlaying(playback);

// Get the AudioMixerGroup assigned to a category
AudioMixerGroup? mixerGroup = conductor.GetAudioMixerGroup(categoryId);
```

Allocation-free overloads that fill a pre-allocated list are also available:  

```cs
var sheetInfos = new List<CueSheetInfo>();
conductor.GetCueSheetInfos(sheetInfos);

var cueInfos = new List<CueInfo>();
conductor.GetCueInfos(handle, cueInfos);

var trackInfos = new List<TrackInfo>();
conductor.GetTrackInfos(handle, "CueName", trackInfos);
```

### CueSheet providers

By passing an `ICueSheetProvider` to the `Conductor` constructor, you can delegate CueSheetAsset loading and releasing to the Conductor.
Implement the `ICueSheetProvider` interface to define how assets are loaded and released for your project's asset management strategy.

Simple built-in implementations are provided for common use cases:

```cs
// Using ResourcesCueSheetProvider
var provider = new ResourcesCueSheetProvider();
var conductor = new Conductor(settings, provider);
var handle = await conductor.RegisterCueSheetAsync("CueSheets/MyCueSheet");
```

```cs
// Using AddressableCueSheetProvider (requires Addressables package)
var provider = new AddressableCueSheetProvider();
var conductor = new Conductor(settings, provider);
var handle = await conductor.RegisterCueSheetAsync("address_key");
```

Addressables support is automatically enabled when the `com.unity.addressables` package is installed. No manual symbol definition is required.

### Dispose

When you no longer need a registered cue-sheet, call `UnregisterCueSheet` to remove that registration.  
This only unregisters the cue-sheet (and releases provider-managed resources when applicable); it does not stop audio that is already playing.  
If you want to stop active playbacks, call `Stop`, `StopAll`, or `Dispose` explicitly.  
When you are done with the entire `Conductor` instance, call `Dispose` to stop all audio and clean up all resources.  

```cs
// Unregister a specific cue-sheet registration
conductor.UnregisterCueSheet(handle);

// Stop all active playbacks explicitly if needed
conductor.StopAll();

// Dispose the entire conductor (stops all audio, unregisters all cue-sheets, and destroys the root GameObject)
conductor.Dispose();
```

## Cue Enum Definition

### Overview

The Cue Enum Definition feature generates type-safe C# enum code from the cue IDs defined in your CueSheetAssets.  
This allows you to reference cues by enum values instead of magic strings or numbers.  

### Open the editor window

Open the editor window from **Tools > Audio Conductor > Cue Enum Definition**.  
The window consists of a tree view on the left and an inspector on the right.  

<p align="center">
  <img width="70%" src="./Images/cue_enum_01.png" alt="Cue Enum Definition Window">
</p>

CueSheetAssets in the project are automatically registered in the tree view. When a new CueSheetAsset is created, it is automatically placed into the appropriate area based on path rules (see below), or added to the top level by default.  

The tree view organizes CueSheetAssets into three areas:  
- Top-level CueSheetAssets: CueSheetAssets listed directly at the root. Each one generates its own individual code file.
- Sheet Groups: A group that bundles multiple CueSheetAssets and generates a single combined code file for all of them. Click the **+ Sheet Group** button to create a new group, then drag CueSheetAssets into it.
- Excluded: A special group for CueSheetAssets you want to exclude from code generation. Assets placed here will not produce any output.

### Grouping with Sheet Group

A Sheet Group bundles multiple CueSheetAssets into a single generated file. Each Sheet Group has the following settings:  

- File Name: Output file name (without extension).
- Output Path: Output directory. Can use the default setting.
- Namespace: Namespace for the generated code. Can use the default setting.
- Class Suffix: Suffix appended to the enum type name. Can use the default setting.
- Path Rule: A glob pattern for automatic asset assignment. When a new CueSheetAsset is created at a path matching this pattern, it is automatically added to this Sheet Group. Supports `*`, `**`, `**/`, and `?` wildcards.

The Excluded group also has a **Path Rule** setting. When a new CueSheetAsset matches the Excluded path rule, it is automatically placed into the Excluded group. The Excluded path rule is evaluated before Sheet Group path rules.  

<p align="center">
  <img width="50%" src="./Images/cue_enum_02.png" alt="Sheet Group Inspector">
</p>

<p align="center">
  <img width="50%" src="./Images/cue_enum_03.png" alt="Excluded Inspector">
</p>

### Default settings

Default values for Output Path, Namespace, and Class Suffix can be configured at the top of the editor window. FileEntries can use these defaults by enabling the "Use Default" toggles.  

### Generate / Batch generation

- Click the **Generate** button at the bottom of the editor window to generate code.
- For batch generation (e.g. in CI), use **Tools > Audio Conductor > Generate Cue Enums** or call the following from the command line:

```bash
Unity -batchmode -projectPath . -executeMethod AudioConductor.Editor.Core.Tools.CodeGen.CueEnumBatchCommand.GenerateCueEnums
```

Code is also automatically generated before a build via `IPreprocessBuildWithReport`.  

### Generated code example

```cs
// <auto-generated/>
using System;

namespace AudioConductor.Generated
{
    public enum BGMAudioIds
    {
        Track1 = 1001,
        Track2 = 1002,
    }

    public static class BGMAudioIdsExtensions
    {
        public static string GetCueSheetName(this BGMAudioIds value)
        {
            const string cueSheetName = "BGM";
            return value switch
            {
                BGMAudioIds.Track1 => cueSheetName,
                BGMAudioIds.Track2 => cueSheetName,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
            };
        }
    }
}
```

## Editor Localization

The editor supports multiple languages for **tooltips** (the descriptions shown when hovering over fields). Labels such as column headers and field names remain in English.  

To change the language, open the Preferences window from the menu bar (**Unity > Settings...** on macOS / **Edit > Preferences...** on Windows), then select **AudioConductor** in the left pane.  

The following languages are supported:  
- Auto: Automatically detects the Unity Editor language setting.
- English
- Japanese

<p align="center">
  <img width="50%" src="./Images/preferences_01.png" alt="Settings Window">
</p>

## Samples

### Import sample resources

You import the sample resources by pressing the Import button from **Package Manager > Audio Conductor > Samples**.  
When import is complete, open and run the sample scene.  

```
Assets/Samples/AudioConductor/[VERSION]/Audio Conductor Sample/SampleScene.unity
```

<p align="center">
  <img width="80%" src="./Images/sample_01.png" alt="Import Sample">
</p>

### Sample scene

The sample uses two `Conductor` instances with separate Settings assets:  

| Conductor | Settings asset | Manages |
|-----------|---------------|---------|
| **BGM Conductor** | `Settings_BGM` | BGM (Field / Battle) |
| **SEVoice Conductor** | `Settings_SEVoice` | SE + Voice (shared via categories) |

**BGM Conductor** registers two CueSheetAssets (`BGM_Field` / `BGM_Battle`) on the same Conductor, demonstrating scene-switch crossfade with `PlayOptions.FadeTime`.  

**SEVoice Conductor** manages SE and Voice on a single Conductor, using categories to separate them. SE uses `PlayOneShot` for fire-and-forget sound effects with random track selection. Voice is loaded asynchronously via `ResourcesCueSheetProvider` and `RegisterCueSheetAsync`.  

The sample provides five volume sliders:  

| Slider | API |
|--------|-----|
| BGM Master Volume | `_bgmConductor.SetMasterVolume()` |
| BGM Category Volume | `_bgmConductor.SetCategoryVolume(0, ...)` |
| SE Category Volume | `_seVoiceConductor.SetCategoryVolume(0, ...)` |
| Voice Category Volume | `_seVoiceConductor.SetCategoryVolume(1, ...)` |
| SEVoice Master Volume | `_seVoiceConductor.SetMasterVolume()` |

The SEVoice master volume slider affects all sounds (both SE and Voice) playing in that Conductor instance, while the SE and Voice category volume sliders adjust each category independently.  

<p align="center">
  <img width="80%" src="./Images/sample_02.png" alt="Sample Scene">
</p>

Please see the following file for implementation details:  

- [SampleScene.cs](../Packages/AudioConductor/Samples~/AudioConductorSample/SampleScene.cs)

## Migration from v1

### API mapping

| v1 | v2 |
|---|---|
| `AudioConductorInterface.Setup(settings, callback)` | `new Conductor(settings)` |
| `AudioConductorInterface.CreateController(asset, index)` | `conductor.RegisterCueSheet(asset)` + `conductor.Play(handle, ...)` |
| `ICueController.Play(trackIndex)` | `conductor.Play(handle, cueName, options)` |
| `ICueController.Stop()` | `conductor.Stop(playbackHandle)` |
| `ICueController.Pause()` / `Resume()` | `conductor.Pause(playbackHandle)` / `Resume(playbackHandle)` |
| `ITrackController` (volume/pitch) | `conductor.SetVolume(playbackHandle, value)` / `conductor.SetPitch(playbackHandle, value)` |
| `ICueController.Dispose()` | `conductor.UnregisterCueSheet(handle)` |
| Callback on cue-sheet unused | Not needed; call `UnregisterCueSheet` explicitly |

### CueSheet asset compatibility

CueSheetAssets created in v1 are fully compatible with v2.  

- When a v1 asset is loaded in v2, the new `cueId` field is initialized to `0` (unassigned).
- The `CueSheetAssetImportChecker` automatically detects and resolves duplicate or missing cue IDs at Unity startup and on asset import.
- Name-based playback (`Play(handle, "CueName")`) works without cue IDs and requires no migration.
- CueId-based playback (`Play(handle, cueId)`) becomes available after automatic assignment.

No manual migration steps are required.  

### Addressables support

Addressables integration is automatically enabled when the `com.unity.addressables` package is installed.  
The `AUDIOCONDUCTOR_ADDRESSABLES` preprocessor directive is defined via `versionDefines` in the assembly definition file, so no manual symbol definition is needed.  

## License

This software is released under the MIT license.  
You are free to use it within the scope of the license, but the following copyright and license notices are required.  

* [LICENSE.md](/LICENSE.md)
