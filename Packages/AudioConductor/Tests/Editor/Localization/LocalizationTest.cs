// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using L = AudioConductor.Editor.Localization.Localization;

namespace AudioConductor.Editor.Localization.Tests
{
    internal class LocalizationTest
    {
        private EditorLanguage _savedLanguage;

        [SetUp]
        public void SetUp()
        {
            _savedLanguage = L.Language;
        }

        [TearDown]
        public void TearDown()
        {
            L.EnglishTable.Remove("test.key");
            L.JapaneseTable.Remove("test.key");
            L.Language = _savedLanguage;
        }

        [Test]
        public void Tr_English_KeyExists_ReturnsTranslation()
        {
            L.EnglishTable["test.key"] = "Hello";
            L.Language = EditorLanguage.English;

            var result = L.Tr("test.key");

            Assert.AreEqual("Hello", result);
        }

        [Test]
        public void Tr_English_KeyMissing_ReturnsKey()
        {
            L.Language = EditorLanguage.English;

            var result = L.Tr("test.key");

            Assert.AreEqual("test.key", result);
        }

        [Test]
        public void Tr_Japanese_KeyExists_ReturnsTranslation()
        {
            L.JapaneseTable["test.key"] = "こんにちは";
            L.Language = EditorLanguage.Japanese;

            var result = L.Tr("test.key");

            Assert.AreEqual("こんにちは", result);
        }

        [Test]
        public void Tr_Japanese_KeyMissing_ReturnsKey()
        {
            L.Language = EditorLanguage.Japanese;

            var result = L.Tr("test.key");

            Assert.AreEqual("test.key", result);
        }

        [Test]
        public void Tr_SwitchLanguage_ReturnsDifferentTranslation()
        {
            L.EnglishTable["test.key"] = "Hello";
            L.JapaneseTable["test.key"] = "こんにちは";

            L.Language = EditorLanguage.English;
            Assert.AreEqual("Hello", L.Tr("test.key"));

            L.Language = EditorLanguage.Japanese;
            Assert.AreEqual("こんにちは", L.Tr("test.key"));
        }

        [Test]
        public void LanguageChanged_RaisedWhenLanguageSet()
        {
            var raised = false;

            void OnLanguageChanged()
            {
                raised = true;
            }

            L.LanguageChanged += OnLanguageChanged;
            try
            {
                L.Language = EditorLanguage.English;
                Assert.IsTrue(raised);
            }
            finally
            {
                L.LanguageChanged -= OnLanguageChanged;
            }
        }

        [TestCase("settings.throttle_type", "Concurrent play control type.")]
        [TestCase("settings.throttle_limit", "Limit of concurrent play.")]
        [TestCase("settings.master_volume", "Master volume scale applied to all audio. (0.0 to 1.0)")]
        [TestCase("settings.managed_pool_size", "Number of managed AudioClipPlayers to pre-create on construction.")]
        [TestCase("settings.oneshot_pool_size", "Number of one-shot AudioClipPlayers to pre-create on construction.")]
        public void Tr_English_SettingsKeys_ReturnsExpectedText(string key, string expected)
        {
            L.Language = EditorLanguage.English;

            var result = L.Tr(key);

            Assert.AreEqual(expected, result);
        }

        [TestCase("settings.throttle_type", "同時発音の制御方式")]
        [TestCase("settings.throttle_limit", "同時発音数の上限")]
        [TestCase("settings.master_volume", "全オーディオに適用されるマスターボリューム (0.0 〜 1.0)")]
        [TestCase("settings.managed_pool_size", "構築時に事前生成するマネージド AudioClipPlayer の数")]
        [TestCase("settings.oneshot_pool_size", "構築時に事前生成するワンショット AudioClipPlayer の数")]
        public void Tr_Japanese_SettingsKeys_ReturnsExpectedText(string key, string expected)
        {
            L.Language = EditorLanguage.Japanese;

            var result = L.Tr(key);

            Assert.AreEqual(expected, result);
        }

        [TestCase("category.name", "Category name.")]
        [TestCase("category.throttle_type", "Concurrent play control type.")]
        [TestCase("category.throttle_limit", "Limit of concurrent play.")]
        [TestCase("category.audio_mixer_group", "Output AudioMixerGroup.")]
        [TestCase("cue_sheet.tab_parameter", "Edit CueSheet parameter")]
        [TestCase("cue_sheet.tab_cue_list", "Edit Cue&Track")]
        [TestCase("cue_sheet.tab_other_operation", "Other operations")]
        [TestCase("cue_list.toggle_volume", "Show/Hide \"Volume\" & \"Volume Range\" columns")]
        [TestCase("cue_list.toggle_play_info", "Show/Hide \"Category\" & \"Play Type\" columns")]
        [TestCase("cue_list.toggle_throttle", "Show/Hide \"Throttle Type\" & \"Throttle Limit\" columns")]
        [TestCase("cue_list.toggle_memo", "Show/Hide \"Color\" column")]
        [TestCase("cue_list.toggle_inspector", "Show/Hide inspector")]
        [TestCase("cue_list.column_name", "Cue or Track name.")]
        [TestCase("cue_list.column_color", "Color label assigned to the Cue or Track.")]
        [TestCase("cue_list.column_category", "Category assigned to the Cue.")]
        [TestCase("cue_list.column_throttle_type", "Concurrent play control type for the Cue.")]
        [TestCase("cue_list.column_throttle_limit", "Limit of concurrent play for the Cue.")]
        [TestCase("cue_list.column_volume", "Volume scale for the Cue or Track. (0.0 to 1.0)")]
        [TestCase("cue_list.column_volume_range", "Random volume variation width for the Cue or Track. (0.0 to 1.0)")]
        [TestCase("cue_list.column_play_type", "Cue playback rule for selecting Track order.")]
        [TestCase("cue_inspector.name", "Cue name.")]
        [TestCase("cue_inspector.cue_id", "Cue ID.")]
        [TestCase("cue_inspector.color", "Cue color label.")]
        [TestCase("cue_inspector.category", "Category applied to this cue.")]
        [TestCase("cue_inspector.throttle_type", "Concurrent play control type for this cue.")]
        [TestCase("cue_inspector.throttle_limit", "Limit of concurrent play for this cue.")]
        [TestCase("cue_inspector.volume", "Cue volume scale. (0.0 to 1.0)")]
        [TestCase("cue_inspector.volume_range", "Random volume variation width. (0.0 to 1.0)")]
        [TestCase("cue_inspector.pitch", "Cue pitch scale. (0.01 to 3.0)")]
        [TestCase("cue_inspector.pitch_range", "Random pitch variation width. (0.0 to 1.0)")]
        [TestCase("cue_inspector.pitch_invert", "Invert pitch variation direction.")]
        [TestCase("cue_inspector.play_type", "Cue playback mode.")]
        [TestCase("cue_inspector.play", "Play the selected cue using its play type and track selection rules.")]
        [TestCase("cue_inspector.pause", "Pause or resume cue preview.")]
        [TestCase("cue_inspector.stop", "Stop cue preview.")]
        [TestCase("track_inspector.name", "Track name.")]
        [TestCase("track_inspector.color", "Track color label.")]
        [TestCase("track_inspector.audio_clip", "AudioClip assigned to this track.")]
        [TestCase("track_inspector.volume", "Track volume scale. (0.0 to 1.0)")]
        [TestCase("track_inspector.volume_range", "Random volume variation width. (0.0 to 1.0)")]
        [TestCase("track_inspector.pitch", "Track pitch scale. (0.01 to 3.0)")]
        [TestCase("track_inspector.pitch_range", "Random pitch variation width. (0.0 to 1.0)")]
        [TestCase("track_inspector.pitch_invert", "Invert pitch variation direction.")]
        [TestCase("track_inspector.random_weight", "Weight used for random track selection.")]
        [TestCase("track_inspector.priority", "Playback priority. Smaller values are higher priority.")]
        [TestCase("track_inspector.fade_time", "Fade time in seconds.")]
        [TestCase("track_inspector.start_sample", "Playback start sample.")]
        [TestCase("track_inspector.end_sample", "Playback end sample.")]
        [TestCase("track_inspector.loop", "Enable looping for this track.")]
        [TestCase("track_inspector.loop_start_sample", "Sample position where looping restarts.")]
        [TestCase("track_inspector.analyze", "Analyze the AudioClip and set loop metadata from wav chunks.")]
        [TestCase("track_inspector.play", "Preview the selected track.")]
        [TestCase("track_inspector.pause", "Pause or resume track preview.")]
        [TestCase("track_inspector.stop", "Stop track preview.")]
        [TestCase("cue_sheet_parameter.name", "CueSheet name.")]
        [TestCase("cue_sheet_parameter.throttle_type",
            "Default concurrent play control type for cues in this CueSheet.")]
        [TestCase("cue_sheet_parameter.throttle_limit", "Default limit of concurrent play for cues in this CueSheet.")]
        [TestCase("cue_sheet_parameter.volume", "Default CueSheet volume scale. (0.0 to 1.0)")]
        [TestCase("cue_sheet_parameter.pitch", "Default CueSheet pitch scale. (0.01 to 3.0)")]
        [TestCase("cue_sheet_parameter.pitch_invert", "Invert CueSheet pitch variation direction.")]
        [TestCase("other_operation.export_csv", "Export CueSheet data to CSV.")]
        [TestCase("other_operation.import_csv", "Import CueSheet data from CSV.")]
        [TestCase("cue_enum_definition.default_output_path", "Default output directory for generated enum files.")]
        [TestCase("cue_enum_definition.default_namespace",
            "Default namespace for generated enums. Empty means no namespace.")]
        [TestCase("cue_enum_definition.default_class_suffix",
            "Default suffix appended to enum type names. e.g. BGM + Cues = BGMCues.")]
        [TestCase("cue_enum_definition.generate", "Generate enum files from the current definition.")]
        [TestCase("cue_enum_definition.file_entry.file_name", "Output file name (without extension).")]
        [TestCase("cue_enum_definition.file_entry.use_default_output_path",
            "Use the default output path from definition settings.")]
        [TestCase("cue_enum_definition.file_entry.output_path", "Output directory for this sheet group.")]
        [TestCase("cue_enum_definition.file_entry.use_default_namespace",
            "Use the default namespace from definition settings.")]
        [TestCase("cue_enum_definition.file_entry.namespace", "Namespace for this sheet group.")]
        [TestCase("cue_enum_definition.file_entry.use_default_class_suffix",
            "Use the default class suffix from definition settings.")]
        [TestCase("cue_enum_definition.file_entry.class_suffix",
            "Suffix appended to enum type names in this sheet group.")]
        [TestCase("cue_enum_definition.file_entry.path_rule",
            "Glob pattern to auto-assign CueSheetAssets to this sheet group.")]
        [TestCase("cue_enum_definition.asset.asset", "The CueSheetAsset reference.")]
        [TestCase("cue_enum_definition.asset.cue_sheet_name", "Name of the CueSheet.")]
        [TestCase("cue_enum_definition.asset.cue_count", "Number of cues in this CueSheet.")]
        [TestCase("cue_enum_definition.excluded.path_rule", "Glob pattern to auto-exclude CueSheetAssets.")]
        public void Tr_English_RemainingKeys_ReturnsExpectedText(string key, string expected)
        {
            L.Language = EditorLanguage.English;

            var result = L.Tr(key);

            Assert.AreEqual(expected, result);
        }

        [TestCase("category.name", "カテゴリ名")]
        [TestCase("category.throttle_type", "同時発音の制御方式")]
        [TestCase("category.throttle_limit", "同時発音数の上限")]
        [TestCase("category.audio_mixer_group", "出力先の AudioMixerGroup")]
        [TestCase("cue_sheet.tab_parameter", "CueSheet パラメータを編集")]
        [TestCase("cue_sheet.tab_cue_list", "Cue&Track を編集")]
        [TestCase("cue_sheet.tab_other_operation", "その他の操作")]
        [TestCase("cue_list.toggle_volume", "\"Volume\" & \"Volume Range\" 列の表示切替")]
        [TestCase("cue_list.toggle_play_info", "\"Category\" & \"Play Type\" 列の表示切替")]
        [TestCase("cue_list.toggle_throttle", "\"Throttle Type\" & \"Throttle Limit\" 列の表示切替")]
        [TestCase("cue_list.toggle_memo", "\"Color\" 列の表示切替")]
        [TestCase("cue_list.toggle_inspector", "インスペクタの表示切替")]
        [TestCase("cue_list.column_name", "Cue または Track の名前")]
        [TestCase("cue_list.column_color", "Cue または Track に割り当てる色ラベル")]
        [TestCase("cue_list.column_category", "Cue に割り当てるカテゴリ")]
        [TestCase("cue_list.column_throttle_type", "Cue の同時発音制御方式")]
        [TestCase("cue_list.column_throttle_limit", "Cue の同時発音数上限")]
        [TestCase("cue_list.column_volume", "Cue または Track の音量スケール (0.0 〜 1.0)")]
        [TestCase("cue_list.column_volume_range", "Cue または Track の音量ランダム変動幅 (0.0 〜 1.0)")]
        [TestCase("cue_list.column_play_type", "Track の選択順を決める Cue の再生ルール")]
        [TestCase("cue_inspector.name", "Cue 名")]
        [TestCase("cue_inspector.cue_id", "Cue ID")]
        [TestCase("cue_inspector.color", "Cue に付与する色ラベル")]
        [TestCase("cue_inspector.category", "この Cue に適用するカテゴリ")]
        [TestCase("cue_inspector.throttle_type", "この Cue の同時発音の制御方式")]
        [TestCase("cue_inspector.throttle_limit", "この Cue の同時発音数の上限")]
        [TestCase("cue_inspector.volume", "Cue の音量スケール (0.0 〜 1.0)")]
        [TestCase("cue_inspector.volume_range", "音量のランダム変動幅 (0.0 〜 1.0)")]
        [TestCase("cue_inspector.pitch", "Cue のピッチスケール (0.01 〜 3.0)")]
        [TestCase("cue_inspector.pitch_range", "ピッチのランダム変動幅 (0.0 〜 1.0)")]
        [TestCase("cue_inspector.pitch_invert", "ピッチ変動の向きを反転")]
        [TestCase("cue_inspector.play_type", "Cue の再生方式")]
        [TestCase("cue_inspector.play", "選択中の Cue を、その Play Type と Track 選択ルールに従って再生")]
        [TestCase("cue_inspector.pause", "Cue の試聴を一時停止 / 再開")]
        [TestCase("cue_inspector.stop", "Cue の試聴を停止")]
        [TestCase("track_inspector.name", "Track 名")]
        [TestCase("track_inspector.color", "Track に付与する色ラベル")]
        [TestCase("track_inspector.audio_clip", "この Track に割り当てる AudioClip")]
        [TestCase("track_inspector.volume", "Track の音量スケール (0.0 〜 1.0)")]
        [TestCase("track_inspector.volume_range", "音量のランダム変動幅 (0.0 〜 1.0)")]
        [TestCase("track_inspector.pitch", "Track のピッチスケール (0.01 〜 3.0)")]
        [TestCase("track_inspector.pitch_range", "ピッチのランダム変動幅 (0.0 〜 1.0)")]
        [TestCase("track_inspector.pitch_invert", "ピッチ変動の向きを反転")]
        [TestCase("track_inspector.random_weight", "ランダム再生時の選択ウェイト")]
        [TestCase("track_inspector.priority", "再生優先度。値が小さいほど高優先度")]
        [TestCase("track_inspector.fade_time", "フェード時間 (秒)")]
        [TestCase("track_inspector.start_sample", "再生開始サンプル位置")]
        [TestCase("track_inspector.end_sample", "再生終了サンプル位置")]
        [TestCase("track_inspector.loop", "この Track のループ再生を有効化")]
        [TestCase("track_inspector.loop_start_sample", "ループ時に戻るサンプル位置")]
        [TestCase("track_inspector.analyze", "AudioClip を解析し、wav chunk からループ情報を設定")]
        [TestCase("track_inspector.play", "選択中の Track を試聴")]
        [TestCase("track_inspector.pause", "Track の試聴を一時停止 / 再開")]
        [TestCase("track_inspector.stop", "Track の試聴を停止")]
        [TestCase("cue_sheet_parameter.name", "CueSheet 名")]
        [TestCase("cue_sheet_parameter.throttle_type", "この CueSheet 配下の Cue に対するデフォルトの同時発音制御方式")]
        [TestCase("cue_sheet_parameter.throttle_limit", "この CueSheet 配下の Cue に対するデフォルトの同時発音数上限")]
        [TestCase("cue_sheet_parameter.volume", "CueSheet のデフォルト音量スケール (0.0 〜 1.0)")]
        [TestCase("cue_sheet_parameter.pitch", "CueSheet のデフォルトピッチスケール (0.01 〜 3.0)")]
        [TestCase("cue_sheet_parameter.pitch_invert", "CueSheet のピッチ変動方向を反転")]
        [TestCase("other_operation.export_csv", "CueSheet データを CSV としてエクスポート")]
        [TestCase("other_operation.import_csv", "CSV から CueSheet データをインポート")]
        [TestCase("cue_enum_definition.default_output_path", "生成する enum ファイルのデフォルト出力先ディレクトリ")]
        [TestCase("cue_enum_definition.default_namespace",
            "生成する enum のデフォルト namespace。空の場合は namespace なし")]
        [TestCase("cue_enum_definition.default_class_suffix",
            "enum 型名に付与するデフォルトの suffix。例: BGM + Cues = BGMCues")]
        [TestCase("cue_enum_definition.generate", "現在の定義から enum ファイルを生成")]
        [TestCase("cue_enum_definition.file_entry.file_name", "出力ファイル名 (拡張子なし)")]
        [TestCase("cue_enum_definition.file_entry.use_default_output_path",
            "定義設定のデフォルト出力先ディレクトリを使用")]
        [TestCase("cue_enum_definition.file_entry.output_path", "このシートグループの出力先ディレクトリ")]
        [TestCase("cue_enum_definition.file_entry.use_default_namespace",
            "定義設定のデフォルト namespace を使用")]
        [TestCase("cue_enum_definition.file_entry.namespace", "このシートグループの namespace")]
        [TestCase("cue_enum_definition.file_entry.use_default_class_suffix",
            "定義設定のデフォルト class suffix を使用")]
        [TestCase("cue_enum_definition.file_entry.class_suffix",
            "このシートグループの enum 型名に付与する suffix")]
        [TestCase("cue_enum_definition.file_entry.path_rule",
            "CueSheetAsset を自動振り分けする glob パターン")]
        [TestCase("cue_enum_definition.asset.asset", "CueSheetAsset の参照")]
        [TestCase("cue_enum_definition.asset.cue_sheet_name", "CueSheet の名前")]
        [TestCase("cue_enum_definition.asset.cue_count", "この CueSheet の Cue 数")]
        [TestCase("cue_enum_definition.excluded.path_rule", "CueSheetAsset を自動除外する glob パターン")]
        public void Tr_Japanese_RemainingKeys_ReturnsExpectedText(string key, string expected)
        {
            L.Language = EditorLanguage.Japanese;

            var result = L.Tr(key);

            Assert.AreEqual(expected, result);
        }
    }
}
