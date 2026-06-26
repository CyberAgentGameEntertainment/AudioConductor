# Changelog

## v2.4.0 - 2026/06/26

- New Features
  - Add `referenceSampleRate` to CueSheet: sample positions are automatically scaled at runtime when the clip's actual decoding frequency differs from the authored rate, preventing playback drift on platforms that resample audio (e.g. when Audio Import Settings do not use Preserve Sample Rate); the editor warns when `referenceSampleRate` is unset (Inspector, validation window, and build) and shows a one-time migration dialog on startup to help set it on existing CueSheets
  - Seamless BGM loop and reliable scheduled playback on WebGL: Unity's `AudioSource.SetScheduledEndTime` does not work on WebGL even in versions where the reported bug is marked as fixed, and Unity exposes no API to correlate a C# AudioSource with its underlying WebAudio channel. AudioConductor intercepts Unity's internal channel-creation hook to establish the binding and manages loop points and scheduled stops directly via WebAudio. DecompressOnLoad uses native WebAudio loop; CompressedInMemory and Streaming detect their backend at runtime and apply native loop or a crossover strategy accordingly. Pause/resume on app focus loss and system interruptions is also handled under this layer.
- Fix Issues
  - Fix out-of-range sample position assignment when setting up a cue with loop points

## v2.3.1 - 2026/06/11

- Fix Issues
  - Fix loop sample range not being applied during track preview playback (regression in v2.0.0)

## v2.3.0 - 2026/06/05

- New Features
  - CueSheet validation window for detecting asset configuration issues

## v2.2.0 - 2026/05/19

- New Features
  - Open AudioClip property inspector from TrackInspector

- Fix Issues
  - Fix CueSheet assets not found when asset cache was initialized as empty

## v2.1.1 - 2026/05/14

- Fix Issues
  - Add Unity 6000.x API compatibility for generic TreeView types and InstanceID-to-EntityId transition

## v2.1.0 - 2026/05/13

- New Features
  - CueSheet List window to browse and open CueSheetAssets

## v2.0.1 - 2026/04/22

- Fix Issues
  - Restored playback stop/end callbacks that were inadvertently removed in v2.0.0.
- New API
  - `PlayOptions.OnStop` / `PlayOptions.OnEnd`
  - `PlayOneShotOptions` (new struct with `OnStop` / `OnEnd`)
  - `Conductor.PlayOneShot(CueSheetHandle, string, PlayOneShotOptions?)` overload
  - `Conductor.PlayOneShot(CueSheetHandle, int, PlayOneShotOptions?)` overload

## v2.0.0 - 2026/03/25

- New Features
  - Instance-based `Conductor` class replacing static `AudioConductorInterface`
  - `CueSheetHandle` / `PlaybackHandle` for safe resource management
  - `PlayOptions` for customizable playback (loop, track selection, fade)
  - `PlayOneShot` for fire-and-forget playback
  - Category volume control without relying on AudioMixer
  - `IFader` interface with `Faders.Linear` for custom fade curves
  - `ICueSheetProvider` for async CueSheet loading (`ResourcesCueSheetProvider`, `AddressableCueSheetProvider`)
  - Query API (`GetCueSheetInfos`, `GetCueInfos`, `GetTrackInfos`, etc.)
  - Cue ID system for type-safe cue references
  - Cue Enum Definition — auto-generates C# enum code from CueSheet assets (batch generation, build-time auto-generation)
  - Editor tooltip localization (Auto/English/Japanese) via Preferences
- Improvements
  - AudioSource pool split into managed and one-shot pools with user-configurable capacity
  - `StopAll` now supports optional fade time and custom fader
  - Master volume is now per-instance instead of a global settings field
- Breaking Changes
  - Removed `AudioConductorInterface` static API
  - Removed `ICueController` / `ITrackController`
  - Unity minimum version raised to 2022.3

## v1.0.2 - 2024/07/18

- Fix Issues 
  - Issue #7; fixes an audio end time discrepancy when repeating Pause and Resume multiple times.

## v1.0.1 - 2024/07/16

- New Features 🚀
  - Feature to check if a cue has a track with a specified name.

## v1.0.0 - 2023/09/06

- Initial submission for package distribution
