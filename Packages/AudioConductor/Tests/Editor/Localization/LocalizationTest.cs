// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Localization;
using NUnit.Framework;
using L = AudioConductor.Editor.Localization.Localization;

namespace AudioConductor.Editor.Tests.Localization
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
    }
}
