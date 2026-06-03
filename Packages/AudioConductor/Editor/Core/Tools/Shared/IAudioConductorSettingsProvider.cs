// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal interface IAudioConductorSettingsProvider
    {
        AudioConductorSettings[] AllSettings { get; }
        AudioConductorSettings? GetByGuid(string guid);
        string GetGuid(AudioConductorSettings settings);
    }
}
