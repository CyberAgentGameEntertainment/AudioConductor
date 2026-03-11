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
        public void Tr_Japanese_RemainingKeys_ReturnsExpectedText(string key, string expected)
        {
            L.Language = EditorLanguage.Japanese;

            var result = L.Tr(key);

            Assert.AreEqual(expected, result);
        }
    }
}
