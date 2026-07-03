// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;
using AudioConductor.Editor.Core.Tests;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Tests
{
    internal sealed class CueSheetParameterPaneModelTests
    {
        private readonly List<AudioClip> _clips = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var clip in _clips)
                Object.DestroyImmediate(clip);
            _clips.Clear();
        }

        private AudioClip CreateClip(int frequency)
        {
            var clip = AudioClip.Create("test", frequency, 1, frequency, false);
            _clips.Add(clip);
            return clip;
        }

        private static CueSheet BuildCueSheet(params AudioClip?[] clipsPerTrack)
        {
            var cue = new Cue { name = "cue" };
            foreach (var clip in clipsPerTrack)
                cue.trackList.Add(new Track { name = "track", audioClip = clip });
            return new CueSheet { name = "sheet", cueList = { cue } };
        }

        // --- CanApplyReferenceSampleRate ---

        [Test]
        public void CanApplyReferenceSampleRate_NoCues_ReturnsFalse()
        {
            var cueSheet = new CueSheet { name = "sheet" };
            var model = new CueSheetParameterPaneModel(cueSheet, new AutoIncrementHistory(), new AssetSaveService());

            Assert.That(model.CanApplyReferenceSampleRate, Is.False);
        }

        [Test]
        public void CanApplyReferenceSampleRate_TracksWithNoClips_ReturnsFalse()
        {
            var cueSheet = BuildCueSheet(null, null);
            var model = new CueSheetParameterPaneModel(cueSheet, new AutoIncrementHistory(), new AssetSaveService());

            Assert.That(model.CanApplyReferenceSampleRate, Is.False);
        }

        [Test]
        public void CanApplyReferenceSampleRate_AllTracksHaveSameFrequency_ReturnsTrue()
        {
            var clip1 = CreateClip(44100);
            var clip2 = CreateClip(44100);
            var cueSheet = BuildCueSheet(clip1, clip2);
            var model = new CueSheetParameterPaneModel(cueSheet, new AutoIncrementHistory(), new AssetSaveService());

            Assert.That(model.CanApplyReferenceSampleRate, Is.True);
        }

        [Test]
        public void CanApplyReferenceSampleRate_TracksHaveDifferentFrequencies_ReturnsFalse()
        {
            var clip1 = CreateClip(44100);
            var clip2 = CreateClip(48000);
            var cueSheet = BuildCueSheet(clip1, clip2);
            var model = new CueSheetParameterPaneModel(cueSheet, new AutoIncrementHistory(), new AssetSaveService());

            Assert.That(model.CanApplyReferenceSampleRate, Is.False);
        }

        // --- ApplyReferenceSampleRate ---

        [Test]
        public void ApplyReferenceSampleRate_SetsClipFrequency()
        {
            var clip = CreateClip(44100);
            var cueSheet = BuildCueSheet(clip);
            var model = new CueSheetParameterPaneModel(cueSheet, new AutoIncrementHistory(), new AssetSaveService());

            model.ApplyReferenceSampleRate();

            Assert.That(model.ReferenceSampleRateObservable.Value, Is.EqualTo(44100));
            Assert.That(cueSheet.referenceSampleRate, Is.EqualTo(44100));
        }

        [Test]
        public void ApplyReferenceSampleRate_InconsistentFrequencies_DoesNothing()
        {
            var clip1 = CreateClip(44100);
            var clip2 = CreateClip(48000);
            var cueSheet = BuildCueSheet(clip1, clip2);
            var model = new CueSheetParameterPaneModel(cueSheet, new AutoIncrementHistory(), new AssetSaveService());

            model.ApplyReferenceSampleRate();

            Assert.That(model.ReferenceSampleRateObservable.Value, Is.EqualTo(0));
            Assert.That(cueSheet.referenceSampleRate, Is.EqualTo(0));
        }

        [Test]
        public void ApplyReferenceSampleRate_History()
        {
            var clip = CreateClip(44100);
            var cueSheet = BuildCueSheet(clip);
            var history = new AutoIncrementHistory();
            var model = new CueSheetParameterPaneModel(cueSheet, history, new AssetSaveService());

            model.ApplyReferenceSampleRate();

            Assert.That(model.ReferenceSampleRateObservable.Value, Is.EqualTo(44100));
            Assert.That(cueSheet.referenceSampleRate, Is.EqualTo(44100));

            history.Undo();

            Assert.That(model.ReferenceSampleRateObservable.Value, Is.EqualTo(0));
            Assert.That(cueSheet.referenceSampleRate, Is.EqualTo(0));

            history.Redo();

            Assert.That(model.ReferenceSampleRateObservable.Value, Is.EqualTo(44100));
            Assert.That(cueSheet.referenceSampleRate, Is.EqualTo(44100));
        }

        [Test]
        public void NameHistory()
        {
            var defaultValue = Utility.RandomString;
            var sameValue = Utility.RandomString;
            var lastValue = Utility.RandomString;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                name = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            foreach (var testValue in testValues)
                model.Name = testValue;

            Assert.That(model.Name, Is.EqualTo(lastValue));
            Assert.That(cueSheet.name, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            Assert.That(cueSheet.name, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.name, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            Assert.That(cueSheet.name, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(lastValue));
            Assert.That(cueSheet.name, Is.EqualTo(lastValue));
        }

        [Test]
        public void ThrottleTypeHistory()
        {
            const ThrottleType defaultValue = ThrottleType.PriorityOrder;
            const ThrottleType sameValue = ThrottleType.FirstComeFirstServed;
            const ThrottleType lastValue = ThrottleType.PriorityOrder;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                throttleType = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            foreach (var testValue in testValues)
                model.ThrottleType = testValue;

            Assert.That(model.ThrottleType, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.ThrottleType, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.ThrottleType, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.ThrottleType, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.ThrottleType, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeThrottleLimit_LessThanMin()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Min - 1, ValueRangeConst.ThrottleLimit.Min);
        }

        [Test]
        public void ChangeThrottleLimit_EqualMin()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Min);
        }

        [Test]
        public void ChangeThrottleLimit_GreaterThanMax()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Max + 1, ValueRangeConst.ThrottleLimit.Max);
        }

        [Test]
        public void ChangeThrottleLimit_EqualThanMax()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Max, ValueRangeConst.ThrottleLimit.Max);
        }

        [Test]
        public void ChangeThrottleLimit_InRange(
            [Random(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Max, 3)]
            int testValue)
        {
            ChangeThrottleLimit(testValue, testValue);
        }

        private void ChangeThrottleLimit(int testValue, int expected)
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            using (model.ThrottleLimitObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(expected)); }))
            {
                model.ThrottleLimit = testValue;
                Assert.That(model.ThrottleLimit, Is.EqualTo(expected));
                Assert.That(cueSheet.throttleLimit, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ThrottleLimitHistory()
        {
            const int defaultValue = 1;
            var sameValue = Random.Range(100, 199);
            var lastValue = Random.Range(300, 399);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                throttleLimit = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            foreach (var testValue in testValues)
                model.ThrottleLimit = testValue;

            Assert.That(model.ThrottleLimit, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeVolume_LessThanMin()
        {
            ChangeVolume(ValueRangeConst.Volume.Min - 1, ValueRangeConst.Volume.Min);
        }

        [Test]
        public void ChangeVolume_EqualMin()
        {
            ChangeVolume(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Min);
        }

        [Test]
        public void ChangeVolume_GreaterThanMax()
        {
            ChangeVolume(ValueRangeConst.Volume.Max + 1, ValueRangeConst.Volume.Max);
        }

        [Test]
        public void ChangeVolume_EqualMax()
        {
            ChangeVolume(ValueRangeConst.Volume.Max, ValueRangeConst.Volume.Max);
        }

        [Test]
        public void ChangeVolume_InRange(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)]
            float testValue)
        {
            ChangeVolume(testValue, testValue);
        }

        private void ChangeVolume(float testValue, float expected)
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            using (model.VolumeObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(expected)); }))
            {
                model.Volume = testValue;
                Assert.That(model.Volume, Is.EqualTo(expected));
                Assert.That(cueSheet.volume, Is.EqualTo(expected));
            }
        }

        [Test]
        public void VolumeHistory()
        {
            const float defaultValue = 1;
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                volume = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            foreach (var testValue in testValues)
                model.Volume = testValue;

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            Assert.That(cueSheet.volume, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            Assert.That(cueSheet.volume, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.volume, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            Assert.That(cueSheet.volume, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            Assert.That(cueSheet.volume, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePitch_LessThanMin()
        {
            ChangePitch(ValueRangeConst.Pitch.Min - 1, ValueRangeConst.Pitch.Min);
        }

        [Test]
        public void ChangePitch_EqualMin()
        {
            ChangePitch(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Min);
        }

        [Test]
        public void ChangePitch_GreaterThanMax()
        {
            ChangePitch(ValueRangeConst.Pitch.Max + 1, ValueRangeConst.Pitch.Max);
        }

        [Test]
        public void ChangePitch_EqualThanMax()
        {
            ChangePitch(ValueRangeConst.Pitch.Max, ValueRangeConst.Pitch.Max);
        }

        [Test]
        public void ChangePitch_InRange(
            [Random(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Max, 3)]
            float testValue)
        {
            ChangePitch(testValue, testValue);
        }

        private void ChangePitch(float testValue, float expected)
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            using (model.PitchObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(expected)); }))
            {
                model.Pitch = testValue;
                Assert.That(model.Pitch, Is.EqualTo(expected));
                Assert.That(cueSheet.pitch, Is.EqualTo(expected));
            }
        }

        [Test]
        public void PitchHistory()
        {
            const float defaultValue = 1;
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                pitch = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            foreach (var testValue in testValues)
                model.Pitch = testValue;

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_PitchInvert()
        {
            const bool defaultValue = false;
            const bool sameValue = true;
            const bool lastValue = false;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                pitchInvert = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService);

            foreach (var testValue in testValues)
                model.PitchInvert = testValue;

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(lastValue));
        }
    }
}
