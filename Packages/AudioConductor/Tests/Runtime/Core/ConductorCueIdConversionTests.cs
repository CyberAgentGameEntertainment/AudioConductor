// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class ConductorCueIdConversionTests
    {
        private CueSheetAsset _cueSheetAsset = null!;
        private AudioConductorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _cueSheetAsset = ScriptableObject.CreateInstance<CueSheetAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
            Object.DestroyImmediate(_cueSheetAsset);
        }

        [Test]
        public void GetCueId_WithInvalidHandle_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);

            var result = conductor.GetCueId(default, "cue1");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCueId_WithUnregisteredHandle_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);

            var result = conductor.GetCueId(new CueSheetHandle(999), "cue1");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCueId_WithNonExistentCueName_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var result = conductor.GetCueId(sheetHandle, "nonexistent");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCueId_WithValidCueName_ReturnsCueId()
        {
            _cueSheetAsset.cueSheet.cueList.Add(new Cue { name = "cue1", cueId = 42 });

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var result = conductor.GetCueId(sheetHandle, "cue1");

            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void GetCueName_WithInvalidHandle_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);

            var result = conductor.GetCueName(default, 1);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCueName_WithUnregisteredHandle_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);

            var result = conductor.GetCueName(new CueSheetHandle(999), 1);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCueName_WithNonExistentCueId_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var result = conductor.GetCueName(sheetHandle, 999);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetCueName_WithValidCueId_ReturnsCueName()
        {
            _cueSheetAsset.cueSheet.cueList.Add(new Cue { name = "cue1", cueId = 42 });

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var result = conductor.GetCueName(sheetHandle, 42);

            Assert.That(result, Is.EqualTo("cue1"));
        }

        [Test]
        public void GetCueId_AndGetCueName_AreInverses()
        {
            _cueSheetAsset.cueSheet.cueList.Add(new Cue { name = "bgm_title", cueId = 1 });
            _cueSheetAsset.cueSheet.cueList.Add(new Cue { name = "bgm_battle", cueId = 2 });

            using var conductor = new Conductor(_settings);
            var sheetHandle = conductor.RegisterCueSheet(_cueSheetAsset);

            var id = conductor.GetCueId(sheetHandle, "bgm_title");
            var name = conductor.GetCueName(sheetHandle, id!.Value);

            Assert.That(name, Is.EqualTo("bgm_title"));
        }
    }
}
