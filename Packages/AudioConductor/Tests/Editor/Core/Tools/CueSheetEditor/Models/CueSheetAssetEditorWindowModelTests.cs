// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
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
