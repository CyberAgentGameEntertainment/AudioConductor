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
            { "settings.oneshot_pool_size", "構築時に事前生成するワンショット AudioClipPlayer の数" }
        };
    }
}
