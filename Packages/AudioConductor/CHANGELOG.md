# Changelog

## v2.0.0 - YYYY/MM/DD

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
