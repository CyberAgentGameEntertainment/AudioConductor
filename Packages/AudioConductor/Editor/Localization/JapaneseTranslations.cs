// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;

namespace AudioConductor.Editor.Localization
{
    internal static class JapaneseTranslations
    {
        public static Dictionary<string, string> Table { get; } = new()
        {
            { "settings.throttle_type", "同時発音の制御方式" },
            { "settings.throttle_limit", "同時発音数の上限" },
            { "settings.master_volume", "全オーディオに適用されるマスターボリューム (0.0 〜 1.0)" },
            { "settings.managed_pool_size", "構築時に事前生成するマネージド AudioClipPlayer の数" },
            { "settings.oneshot_pool_size", "構築時に事前生成するワンショット AudioClipPlayer の数" },
            { "category.name", "カテゴリ名" },
            { "category.throttle_type", "同時発音の制御方式" },
            { "category.throttle_limit", "同時発音数の上限" },
            { "category.audio_mixer_group", "出力先の AudioMixerGroup" },
            { "cue_sheet.tab_parameter", "CueSheet パラメータを編集" },
            { "cue_sheet.tab_cue_list", "Cue&Track を編集" },
            { "cue_sheet.tab_other_operation", "その他の操作" },
            { "cue_list.toggle_volume", "\"Volume\" & \"Volume Range\" 列の表示切替" },
            { "cue_list.toggle_play_info", "\"Category\" & \"Play Type\" 列の表示切替" },
            { "cue_list.toggle_throttle", "\"Throttle Type\" & \"Throttle Limit\" 列の表示切替" },
            { "cue_list.toggle_memo", "\"Color\" 列の表示切替" },
            { "cue_list.toggle_inspector", "インスペクタの表示切替" }
        };
    }
}
