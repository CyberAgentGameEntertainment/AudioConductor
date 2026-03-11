// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Editor.Localization
{
    internal static class EnglishTranslations
    {
        public static Dictionary<string, string> Table { get; } = new()
        {
            { "settings.throttle_type", "Concurrent play control type." },
            { "settings.throttle_limit", "Limit of concurrent play." },
            { "settings.master_volume", "Master volume scale applied to all audio. (0.0 to 1.0)" },
            { "settings.managed_pool_size", "Number of managed AudioClipPlayers to pre-create on construction." },
            { "settings.oneshot_pool_size", "Number of one-shot AudioClipPlayers to pre-create on construction." }
        };
    }
}
