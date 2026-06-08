// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Tests
{
    internal class CueSheetAssetEditorWindowModelTests
    {
        [Test]
        public void CueSheetId()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            var model = new CueSheetAssetEditorWindowModel(asset);

            Assert.That(model.CueSheetId, Is.EqualTo(asset.cueSheet.Id));
        }

        [Test]
        public void Asset_ReturnsSameCueSheetAsset()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            var model = new CueSheetAssetEditorWindowModel(asset);

            Assert.That(model.Asset, Is.SameAs(asset));
        }

        [Test]
        public void Setup()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            var model = new CueSheetAssetEditorWindowModel(asset);

            Assert.IsNull(model.CueSheetEditorModel);

            model.Setup();

            Assert.IsNotNull(model.CueSheetEditorModel);
            Assert.IsNotNull(model.CueSheetEditorModel.CueSheetParameterPaneModel);
            Assert.IsNotNull(model.CueSheetEditorModel.CueListEditorPaneModel);
            Assert.IsNotNull(model.CueSheetEditorModel.OtherOperationPaneModel);
            Assert.IsNotNull(model.CueSheetEditorModel.ObservablePaneState);
        }

        [Test]
        public void Setup_WithSettingsProvider_CreatesEditorModel()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            var model = new CueSheetAssetEditorWindowModel(asset);

            model.Setup(() => null);

            Assert.That(model.CueSheetEditorModel, Is.Not.Null);
        }
    }
}
