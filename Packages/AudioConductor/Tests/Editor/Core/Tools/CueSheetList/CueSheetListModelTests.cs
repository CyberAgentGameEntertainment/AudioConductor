// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tests;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Tests
{
    internal sealed class CueSheetListModelTests
    {
        private string _assetPath1 = string.Empty;
        private string _assetPath2 = string.Empty;
        private CueSheetListModel? _model;

        [SetUp]
        public void SetUp()
        {
            Utility.CreateFolderRecursively(GlobalSetUpFixture.GenFolder);

            var asset1 = ScriptableObject.CreateInstance<CueSheetAsset>();
            _assetPath1 = $"{GlobalSetUpFixture.GenFolder}/ModelTestBGM.asset";
            AssetDatabase.CreateAsset(asset1, _assetPath1);

            var asset2 = ScriptableObject.CreateInstance<CueSheetAsset>();
            _assetPath2 = $"{GlobalSetUpFixture.GenFolder}/ModelTestSE.asset";
            AssetDatabase.CreateAsset(asset2, _assetPath2);

            _model = new CueSheetListModel(CueSheetAssetRepository.instance);
        }

        [TearDown]
        public void TearDown()
        {
            _model?.Dispose();
            _model = null;

            if (!string.IsNullOrEmpty(_assetPath1))
            {
                AssetDatabase.DeleteAsset(_assetPath1);
                _assetPath1 = string.Empty;
            }

            if (!string.IsNullOrEmpty(_assetPath2))
            {
                AssetDatabase.DeleteAsset(_assetPath2);
                _assetPath2 = string.Empty;
            }
        }

        [Test]
        public void Items_ContainsCreatedAssets()
        {
            var items = _model!.Items.Value;

            Assert.That(items, Has.Some.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestBGM"));
            Assert.That(items, Has.Some.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestSE"));
        }

        [Test]
        public void Items_SearchFilter_MatchesPartialName()
        {
            _model!.SearchFilter.Value = "BGM";
            var items = _model.Items.Value;

            Assert.That(items, Has.Some.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestBGM"));
            Assert.That(items, Has.None.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestSE"));
        }

        [Test]
        public void Items_SearchFilter_IsCaseInsensitive()
        {
            _model!.SearchFilter.Value = "bgm";
            var items = _model.Items.Value;

            Assert.That(items, Has.Some.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestBGM"));
        }

        [Test]
        public void Items_SearchFilter_EmptyReturnsAll()
        {
            _model!.SearchFilter.Value = "BGM";
            _model.SearchFilter.Value = string.Empty;
            var items = _model.Items.Value;

            Assert.That(items, Has.Some.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestBGM"));
            Assert.That(items, Has.Some.Matches<CueSheetListItem>(i => i.DisplayName == "ModelTestSE"));
        }

        [Test]
        public void Items_UpdatedWhenRepositoryChanges()
        {
            var initialCount = _model!.Items.Value.Length;

            var asset3 = ScriptableObject.CreateInstance<CueSheetAsset>();
            var path3 = $"{GlobalSetUpFixture.GenFolder}/ModelTestNewSheet.asset";
            AssetDatabase.CreateAsset(asset3, path3);

            try
            {
                Assert.That(_model.Items.Value.Length, Is.GreaterThan(initialCount));
            }
            finally
            {
                AssetDatabase.DeleteAsset(path3);
            }
        }

        [Test]
        public void OpenRequested_FiresWhenRequestOpenCalled()
        {
            CueSheetAsset? opened = null;
            _model!.OpenRequested.Subscribe(asset => opened = asset);

            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _model.RequestOpen(asset);

            Assert.That(opened, Is.SameAs(asset));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Item_CueCount_MatchesCueListCount()
        {
            var items = _model!.Items.Value;
            var item = Array.Find(items, i => i.DisplayName == "ModelTestBGM");

            Assert.That(item, Is.Not.Null);
            var loaded = AssetDatabase.LoadAssetAtPath<CueSheetAsset>(_assetPath1);
            Assert.That(item!.CueCount, Is.EqualTo(loaded.cueSheet.cueList?.Count ?? 0));
        }
    }
}
