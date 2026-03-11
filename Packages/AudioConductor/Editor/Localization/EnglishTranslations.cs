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
            { "settings.oneshot_pool_size", "Number of one-shot AudioClipPlayers to pre-create on construction." },
            { "category.name", "Category name." },
            { "category.throttle_type", "Concurrent play control type." },
            { "category.throttle_limit", "Limit of concurrent play." },
            { "category.audio_mixer_group", "Output AudioMixerGroup." },
            { "cue_sheet.tab_parameter", "Edit CueSheet parameter" },
            { "cue_sheet.tab_cue_list", "Edit Cue&Track" },
            { "cue_sheet.tab_other_operation", "Other operations" },
            { "cue_list.toggle_volume", "Show/Hide \"Volume\" & \"Volume Range\" columns" },
            { "cue_list.toggle_play_info", "Show/Hide \"Category\" & \"Play Type\" columns" },
            { "cue_list.toggle_throttle", "Show/Hide \"Throttle Type\" & \"Throttle Limit\" columns" },
            { "cue_list.toggle_memo", "Show/Hide \"Color\" column" },
            { "cue_list.toggle_inspector", "Show/Hide inspector" }
        };
    }
}
