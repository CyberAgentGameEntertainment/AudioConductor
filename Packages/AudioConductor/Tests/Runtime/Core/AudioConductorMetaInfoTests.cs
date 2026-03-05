// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using UnityEngine;
using CoreAudioConductor = AudioConductor.Runtime.Core.AudioConductor;
using AudioConductorSettings = AudioConductor.Runtime.Core.Models.AudioConductorSettings;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public class AudioConductorMetaInfoTests
    {
        private AudioConductorSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void GetCueSheetInfos_WithNoRegistrations_ReturnsEmpty()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var infos = conductor.GetCueSheetInfos();

            Assert.That(infos, Is.Empty);
        }

        [Test]
        public void GetCueSheetInfos_ReturnsRegisteredCueSheets()
        {
            using var conductor = new CoreAudioConductor(_settings);
            var asset1 = CreateCueSheetAsset("BGM");
            var asset2 = CreateCueSheetAsset("SE");
            var handle1 = conductor.RegisterCueSheet(asset1);
            var handle2 = conductor.RegisterCueSheet(asset2);

            var infos = conductor.GetCueSheetInfos();

            Assert.That(infos.Count, Is.EqualTo(2));
            Assert.That(infos[0].Handle, Is.EqualTo(handle1));
            Assert.That(infos[0].Name, Is.EqualTo("BGM"));
            Assert.That(infos[1].Handle, Is.EqualTo(handle2));
            Assert.That(infos[1].Name, Is.EqualTo("SE"));

            Object.DestroyImmediate(asset1);
            Object.DestroyImmediate(asset2);
        }

        [Test]
        public void GetCueSheetInfos_AfterUnregister_ExcludesUnregistered()
        {
            using var conductor = new CoreAudioConductor(_settings);
            var asset = CreateCueSheetAsset("BGM");
            var handle = conductor.RegisterCueSheet(asset);
            conductor.UnregisterCueSheet(handle);

            var infos = conductor.GetCueSheetInfos();

            Assert.That(infos, Is.Empty);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void GetCueInfos_WithInvalidHandle_ReturnsEmpty()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var infos = conductor.GetCueInfos(default);

            Assert.That(infos, Is.Empty);
        }

        [Test]
        public void GetCueInfos_ReturnsCueNamesAndCategoryIds()
        {
            using var conductor = new CoreAudioConductor(_settings);
            var asset = CreateCueSheetAsset("Sheet");
            asset.cueSheet.cueList.Add(new Cue { name = "cue_a", categoryId = 1 });
            asset.cueSheet.cueList.Add(new Cue { name = "cue_b", categoryId = 2 });
            var handle = conductor.RegisterCueSheet(asset);

            var infos = conductor.GetCueInfos(handle);

            Assert.That(infos.Count, Is.EqualTo(2));
            Assert.That(infos[0].Name, Is.EqualTo("cue_a"));
            Assert.That(infos[0].CategoryId, Is.EqualTo(1));
            Assert.That(infos[1].Name, Is.EqualTo("cue_b"));
            Assert.That(infos[1].CategoryId, Is.EqualTo(2));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void GetCueInfos_WithUnknownHandle_ReturnsEmpty()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var infos = conductor.GetCueInfos(new CueSheetHandle(999));

            Assert.That(infos, Is.Empty);
        }

        [Test]
        public void GetTrackInfos_WithInvalidHandle_ReturnsEmpty()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var infos = conductor.GetTrackInfos(default, "cue");

            Assert.That(infos, Is.Empty);
        }

        [Test]
        public void GetTrackInfos_WithUnknownCueName_ReturnsEmpty()
        {
            using var conductor = new CoreAudioConductor(_settings);
            var asset = CreateCueSheetAsset("Sheet");
            var handle = conductor.RegisterCueSheet(asset);

            var infos = conductor.GetTrackInfos(handle, "nonexistent");

            Assert.That(infos, Is.Empty);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void GetTrackInfos_ReturnsTrackDetails()
        {
            using var conductor = new CoreAudioConductor(_settings);
            var clip = AudioClip.Create("test_clip", 1000, 1, 44100, false);
            var asset = CreateCueSheetAsset("Sheet");
            var cue = new Cue { name = "cue_a" };
            cue.trackList.Add(new Track { name = "track_1", audioClip = clip, isLoop = true, priority = 5 });
            cue.trackList.Add(new Track { name = "track_2", audioClip = null, isLoop = false, priority = 3 });
            asset.cueSheet.cueList.Add(cue);
            var handle = conductor.RegisterCueSheet(asset);

            var infos = conductor.GetTrackInfos(handle, "cue_a");

            Assert.That(infos.Count, Is.EqualTo(2));
            Assert.That(infos[0].Name, Is.EqualTo("track_1"));
            Assert.That(infos[0].AudioClip, Is.EqualTo(clip));
            Assert.That(infos[0].IsLoop, Is.True);
            Assert.That(infos[0].Priority, Is.EqualTo(5));
            Assert.That(infos[1].Name, Is.EqualTo("track_2"));
            Assert.That(infos[1].AudioClip, Is.Null);
            Assert.That(infos[1].IsLoop, Is.False);
            Assert.That(infos[1].Priority, Is.EqualTo(3));

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        private static CueSheetAsset CreateCueSheetAsset(string sheetName)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.name = sheetName;
            return asset;
        }
    }
}
