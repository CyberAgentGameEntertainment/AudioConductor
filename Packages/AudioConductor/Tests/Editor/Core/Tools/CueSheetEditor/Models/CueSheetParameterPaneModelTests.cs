// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueSheetParameterPaneModelTests
    {
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
            => ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Min - 1, ValueRangeConst.ThrottleLimit.Min);

        [Test]
        public void ChangeThrottleLimit_EqualMin()
            => ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Min);

        [Test]
        public void ChangeThrottleLimit_GreaterThanMax()
            => ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Max + 1, ValueRangeConst.ThrottleLimit.Max);

        [Test]
        public void ChangeThrottleLimit_EqualThanMax()
            => ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Max, ValueRangeConst.ThrottleLimit.Max);

        [Test]
        public void ChangeThrottleLimit_InRange(
            [Random(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Max, 3)] int testValue)
            => ChangeThrottleLimit(testValue, testValue);

        private static void ChangeThrottleLimit(int testValue, int expected)
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
            => ChangeVolume(ValueRangeConst.Volume.Min - 1, ValueRangeConst.Volume.Min);

        [Test]
        public void ChangeVolume_EqualMin()
            => ChangeVolume(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Min);

        [Test]
        public void ChangeVolume_GreaterThanMax()
            => ChangeVolume(ValueRangeConst.Volume.Max + 1, ValueRangeConst.Volume.Max);

        [Test]
        public void ChangeVolume_EqualMax()
            => ChangeVolume(ValueRangeConst.Volume.Max, ValueRangeConst.Volume.Max);

        [Test]
        public void ChangeVolume_InRange(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)] float testValue)
            => ChangeVolume(testValue, testValue);

        private static void ChangeVolume(float testValue, float expected)
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
            => ChangePitch(ValueRangeConst.Pitch.Min - 1, ValueRangeConst.Pitch.Min);

        [Test]
        public void ChangePitch_EqualMin()
            => ChangePitch(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Min);

        [Test]
        public void ChangePitch_GreaterThanMax()
            => ChangePitch(ValueRangeConst.Pitch.Max + 1, ValueRangeConst.Pitch.Max);

        [Test]
        public void ChangePitch_EqualThanMax()
            => ChangePitch(ValueRangeConst.Pitch.Max, ValueRangeConst.Pitch.Max);

        [Test]
        public void ChangePitch_InRange(
            [Random(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Max, 3)] float testValue)
            => ChangePitch(testValue, testValue);

        private static void ChangePitch(float testValue, float expected)
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
