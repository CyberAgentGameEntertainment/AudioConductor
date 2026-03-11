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
        public void Setup()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            var model = new CueSheetAssetEditorWindowModel(asset);

            Assert.IsNull(model.CueSheetEditorModel);

            model.Setup();

            Assert.IsNotNull(model.CueSheetEditorModel);
        }
    }
}
