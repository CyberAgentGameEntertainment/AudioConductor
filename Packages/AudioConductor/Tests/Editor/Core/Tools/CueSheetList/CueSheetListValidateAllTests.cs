// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Tests
{
    internal sealed class CueSheetListValidateAllTests
    {
        [Test]
        public void ValidateAllRequested_FiresWhenRequestValidateAllCalled()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var repo = new FakeRepository(new[] { asset });
            using var model = new CueSheetListModel(repo);

            IReadOnlyList<CueSheetAsset>? received = null;
            model.ValidateAllRequested.Subscribe(assets => received = assets);

            model.RequestValidateAll();

            Assert.That(received, Is.EqualTo(new[] { asset }));

            Object.DestroyImmediate(asset);
        }

        private sealed class FakeRepository : ICueSheetAssetRepository
        {
            private readonly CueSheetAsset[] _assets;

            internal FakeRepository(CueSheetAsset[] assets)
            {
                _assets = assets;
            }

#pragma warning disable CS0067
            public event Action? Changed;
#pragma warning restore CS0067

            public CueSheetAsset[] GetAll()
            {
                return _assets;
            }
        }
    }
}
