// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Models.Tests
{
    internal sealed class CueSheetAssetTests
    {
        private CueSheetAsset? _asset;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_asset != null)
                Object.DestroyImmediate(_asset);
        }

        [Test]
        public void CreateInstance_DefaultUseDefaultFlags_AreTrue()
        {
            Assert.That(_asset!.useDefaultCodeGenOutputPath, Is.True);
            Assert.That(_asset.useDefaultCodeGenNamespace, Is.True);
            Assert.That(_asset.useDefaultCodeGenClassSuffix, Is.True);
        }
    }
}
