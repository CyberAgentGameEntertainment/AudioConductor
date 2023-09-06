// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
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
    internal class CueInspectorModelTests
    {
        private const int CueCount = 5;

        private static readonly string[] TestNameSource =
        {
            Utility.RandomString,
            Utility.RandomString,
            Utility.RandomString
        };

        private static string CreateDefaultName(int index) => $"defaultName-{index}";
        private static string CreateDefaultColor(int index) => $"defaultColor-{index}";
        private static int CreateDefaultCategoryId(int index) => index;

        private static ThrottleType CreateDefaultThrottleType(int index) =>
            index % 2 == 0 ? ThrottleType.PriorityOrder : ThrottleType.FirstComeFirstServed;

        private static int CreateDefaultThrottleLimit(int index) => index * 2;
        private static float CreateDefaultVolume(int index) => -index * 11.1f;
        private static float CreateDefaultVolumeRange(int index) => index * 22.2f;
        private static float CreateDefaultPitch(int index) => -index * 33.3f;
        private static float CreateDefaultPitchRange(int index) => index * 44.4f;
        private static bool CreateDefaultPitchInvert(int index) => index % 2 != 0;

        private static CuePlayType CreateDefaultPlayType(int index)
            => index % 2 != 0 ? CuePlayType.Sequential : CuePlayType.Random;

        private static Cue[] CreateTestTargetCues()
        {
            var cues = new Cue[CueCount];
            for (var i = 0; i < CueCount; ++i)
                cues[i] = new Cue
                {
                    name = CreateDefaultName(i),
                    colorId = CreateDefaultColor(i),
                    categoryId = CreateDefaultCategoryId(i),
                    throttleType = CreateDefaultThrottleType(i),
                    throttleLimit = CreateDefaultThrottleLimit(i),
                    volume = CreateDefaultVolume(i),
                    volumeRange = CreateDefaultVolumeRange(i),
                    pitch = CreateDefaultPitch(i),
                    pitchRange = CreateDefaultPitchRange(i),
                    pitchInvert = CreateDefaultPitchInvert(i),
                    playType = CreateDefaultPlayType(i)
                };
            return cues;
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeName(string testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(CreateDefaultName(i)));
            Assert.True(model.NameObservable.Value.HasMultipleDifferentValues);

            using (model.NameObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.Name = testValue;
                Assert.That(model.Name, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.name, Is.EqualTo(testValue));
                Assert.False(model.NameObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void NameHistory()
        {
            var sameValue = Utility.RandomString;
            var lastValue = Utility.RandomString;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.Name = testValue;

            Assert.That(model.Name, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(CreateDefaultName(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(CreateDefaultName(i)));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(lastValue));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeColor(string testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(CreateDefaultColor(i)));
            Assert.True(model.ColorObservable.Value.HasMultipleDifferentValues);

            using (model.ColorObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.Color = testValue;
                Assert.That(model.Color, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.colorId, Is.EqualTo(testValue));
                Assert.False(model.ColorObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ColorHistory()
        {
            var sameValue = Utility.RandomString;
            var lastValue = Utility.RandomString;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.Color = testValue;

            Assert.That(model.Color, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Color, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Color, Is.EqualTo(CreateDefaultColor(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(CreateDefaultColor(i)));

            history.Redo();

            Assert.That(model.Color, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Color, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeCategoryId([Random(3)] int testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(CreateDefaultCategoryId(i)));
            Assert.True(model.CategoryIdObservable.Value.HasMultipleDifferentValues);

            using (model.CategoryIdObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.CategoryId = testValue;
                Assert.That(model.CategoryId, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.categoryId, Is.EqualTo(testValue));
                Assert.False(model.CategoryIdObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void CategoryIdHistory()
        {
            var sameValue = Random.Range(20, 29);
            var lastValue = Random.Range(30, 39);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.CategoryId = testValue;

            Assert.That(model.CategoryId, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.CategoryId, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.CategoryId, Is.EqualTo(CreateDefaultCategoryId(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(CreateDefaultCategoryId(i)));

            history.Redo();

            Assert.That(model.CategoryId, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.CategoryId, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeThrottleType([Values] ThrottleType testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(CreateDefaultThrottleType(i)));
            Assert.True(model.ThrottleTypeObservable.Value.HasMultipleDifferentValues);

            using (model.ThrottleTypeObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ThrottleType = testValue;
                Assert.That(model.ThrottleType, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.throttleType, Is.EqualTo(testValue));
                Assert.False(model.ThrottleTypeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ThrottleTypeHistory()
        {
            const ThrottleType sameValue = ThrottleType.FirstComeFirstServed;
            const ThrottleType lastValue = ThrottleType.PriorityOrder;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.ThrottleType = testValue;

            Assert.That(model.ThrottleType, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.ThrottleType, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.ThrottleType, Is.EqualTo(CreateDefaultThrottleType(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(CreateDefaultThrottleType(i)));

            history.Redo();

            Assert.That(model.ThrottleType, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.ThrottleType, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeThrottleLimit([Random(3)] int testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(CreateDefaultThrottleLimit(i)));
            Assert.True(model.ThrottleLimitObservable.Value.HasMultipleDifferentValues);

            using (model.ThrottleLimitObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ThrottleLimit = testValue;
                Assert.That(model.ThrottleLimit, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.throttleLimit, Is.EqualTo(testValue));
                Assert.False(model.ThrottleLimitObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ThrottleLimitHistory()
        {
            var sameValue = Random.Range(100, 199);
            var lastValue = Random.Range(300, 399);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.ThrottleLimit = testValue;

            Assert.That(model.ThrottleLimit, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(CreateDefaultThrottleLimit(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(CreateDefaultThrottleLimit(i)));

            history.Redo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(lastValue));
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
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(CreateDefaultVolume(i)));
            Assert.True(model.VolumeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.Volume = testValue;
                Assert.That(model.Volume, Is.EqualTo(expected));
                foreach (var cue in cues)
                    Assert.That(cue.volume, Is.EqualTo(expected));
                Assert.False(model.VolumeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void VolumeHistory()
        {
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.Volume = testValue;

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(CreateDefaultVolume(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(CreateDefaultVolume(i)));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeVolumeRange_LessThanMin()
            => ChangeVolumeRange(ValueRangeConst.VolumeRange.Min - 1, ValueRangeConst.VolumeRange.Min);

        [Test]
        public void ChangeVolumeRange_EqualMin()
            => ChangeVolumeRange(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Min);

        [Test]
        public void ChangeVolumeRange_GreaterThanMax()
            => ChangeVolumeRange(ValueRangeConst.VolumeRange.Max + 1, ValueRangeConst.VolumeRange.Max);

        [Test]
        public void ChangeVolumeRange_EqualMax()
            => ChangeVolumeRange(ValueRangeConst.VolumeRange.Max, ValueRangeConst.VolumeRange.Max);

        [Test]
        public void ChangeVolumeRange_InRange(
            [Random(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max, 3)] float testValue)
            => ChangeVolumeRange(testValue, testValue);

        private static void ChangeVolumeRange(float testValue, float expected)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(CreateDefaultVolumeRange(i)));
            Assert.True(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeRangeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.VolumeRange = testValue;
                Assert.That(model.VolumeRange, Is.EqualTo(expected));
                foreach (var cue in cues)
                    Assert.That(cue.volumeRange, Is.EqualTo(expected));
                Assert.False(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void VolumeRangeHistory()
        {
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.VolumeRange = testValue;

            Assert.That(model.VolumeRange, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.VolumeRange, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.VolumeRange, Is.EqualTo(CreateDefaultVolumeRange(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(CreateDefaultVolumeRange(i)));

            history.Redo();

            Assert.That(model.VolumeRange, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.VolumeRange, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(lastValue));
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
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitch, Is.EqualTo(CreateDefaultPitch(i)));
            Assert.True(model.PitchObservable.Value.HasMultipleDifferentValues);

            using (model.PitchObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.Pitch = testValue;
                Assert.That(model.Pitch, Is.EqualTo(expected));
                foreach (var cue in cues)
                    Assert.That(cue.pitch, Is.EqualTo(expected));
                Assert.False(model.PitchObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void PitchHistory()
        {
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.Pitch = testValue;

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitch, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitch, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(CreateDefaultPitch(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitch, Is.EqualTo(CreateDefaultPitch(i)));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitch, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitch, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePitchRange_LessThanMin()
            => ChangePitchRange(ValueRangeConst.PitchRange.Min - 1, ValueRangeConst.PitchRange.Min);

        [Test]
        public void ChangePitchRange_EqualMin()
            => ChangePitchRange(ValueRangeConst.PitchRange.Min, ValueRangeConst.PitchRange.Min);

        [Test]
        public void ChangePitchRange_GreaterThanMax()
            => ChangePitchRange(ValueRangeConst.PitchRange.Max + 1, ValueRangeConst.PitchRange.Max);

        [Test]
        public void ChangePitchRange_EqualMax()
            => ChangePitchRange(ValueRangeConst.PitchRange.Max, ValueRangeConst.PitchRange.Max);

        [Test]
        public void ChangePitchRange_InRange(
            [Random(ValueRangeConst.PitchRange.Min, ValueRangeConst.PitchRange.Max, 3)] float testValue)
            => ChangePitchRange(testValue, testValue);

        private static void ChangePitchRange(float testValue, float expected)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchRange, Is.EqualTo(CreateDefaultPitchRange(i)));
            Assert.True(model.PitchRangeObservable.Value.HasMultipleDifferentValues);

            using (model.PitchRangeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.PitchRange = testValue;
                Assert.That(model.PitchRange, Is.EqualTo(expected));
                foreach (var cue in cues)
                    Assert.That(cue.pitchRange, Is.EqualTo(expected));
                Assert.False(model.PitchRangeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void PitchRangeHistory()
        {
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.PitchRange = testValue;

            Assert.That(model.PitchRange, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchRange, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PitchRange, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchRange, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PitchRange, Is.EqualTo(CreateDefaultPitchRange(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchRange, Is.EqualTo(CreateDefaultPitchRange(i)));

            history.Redo();

            Assert.That(model.PitchRange, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchRange, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PitchRange, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchRange, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePitchInvert([Values] bool testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchInvert, Is.EqualTo(CreateDefaultPitchInvert(i)));
            Assert.True(model.PitchInvertObservable.Value.HasMultipleDifferentValues);

            using (model.PitchInvertObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.PitchInvert = testValue;
                Assert.That(model.PitchInvert, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.pitchInvert, Is.EqualTo(testValue));
                Assert.False(model.PitchInvertObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void History_DifferentValue_PitchInvert()
        {
            const bool sameValue = true;
            const bool lastValue = false;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.PitchInvert = testValue;

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchInvert, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchInvert, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(CreateDefaultPitchInvert(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchInvert, Is.EqualTo(CreateDefaultPitchInvert(i)));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchInvert, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].pitchInvert, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePlayType([Values] CuePlayType testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(CreateDefaultPlayType(i)));
            Assert.True(model.PlayTypeObservable.Value.HasMultipleDifferentValues);

            using (model.PlayTypeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.PlayType = testValue;
                Assert.That(model.PlayType, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.playType, Is.EqualTo(testValue));
                Assert.False(model.PlayTypeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void PlayTypeHistory()
        {
            const CuePlayType sameValue = CuePlayType.Random;
            const CuePlayType lastValue = CuePlayType.Sequential;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            foreach (var testValue in testValues)
                model.PlayType = testValue;

            Assert.That(model.PlayType, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PlayType, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PlayType, Is.EqualTo(CreateDefaultPlayType(0)));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(CreateDefaultPlayType(i)));

            history.Redo();

            Assert.That(model.PlayType, Is.EqualTo(sameValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PlayType, Is.EqualTo(lastValue));
            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(lastValue));
        }

        [Test]
        public void ContainsItemId([Random(3)] int itemId)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(itemId, cue)).ToArray(), history,
                                              new AssetSaveService());

            Assert.True(model.Contains(itemId));
            Assert.False(model.Contains(itemId + 1));
            Assert.False(model.Contains(itemId - 1));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Name(string testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].name, Is.EqualTo(CreateDefaultName(i)));
            Assert.True(model.NameObservable.Value.HasMultipleDifferentValues);

            using (model.NameObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Name, testValue);
                Assert.That(model.Name, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.name, Is.EqualTo(testValue));
                Assert.False(model.NameObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Color(string testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].colorId, Is.EqualTo(CreateDefaultColor(i)));
            Assert.True(model.ColorObservable.Value.HasMultipleDifferentValues);

            using (model.ColorObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Color, testValue);
                Assert.That(model.Color, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.colorId, Is.EqualTo(testValue));
                Assert.False(model.ColorObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_CategoryId([Random(3)] int testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].categoryId, Is.EqualTo(CreateDefaultCategoryId(i)));
            Assert.True(model.CategoryIdObservable.Value.HasMultipleDifferentValues);

            using (model.CategoryIdObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Category, testValue);
                Assert.That(model.CategoryId, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.categoryId, Is.EqualTo(testValue));
                Assert.False(model.CategoryIdObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_ThrottleType([Values] ThrottleType testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleType, Is.EqualTo(CreateDefaultThrottleType(i)));
            Assert.True(model.ThrottleTypeObservable.Value.HasMultipleDifferentValues);

            using (model.ThrottleTypeObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.ThrottleType, testValue);
                Assert.That(model.ThrottleType, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.throttleType, Is.EqualTo(testValue));
                Assert.False(model.ThrottleTypeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_ThrottleLimit([Random(3)] int testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].throttleLimit, Is.EqualTo(CreateDefaultThrottleLimit(i)));
            Assert.True(model.ThrottleLimitObservable.Value.HasMultipleDifferentValues);

            using (model.ThrottleLimitObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.ThrottleLimit, testValue);
                Assert.That(model.ThrottleLimit, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.throttleLimit, Is.EqualTo(testValue));
                Assert.False(model.ThrottleLimitObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_Volume(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)] float testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volume, Is.EqualTo(CreateDefaultVolume(i)));
            Assert.True(model.VolumeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Volume, testValue);
                Assert.That(model.Volume, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.volume, Is.EqualTo(testValue));
                Assert.False(model.VolumeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_VolumeRange(
            [Random(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max, 3)] float testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].volumeRange, Is.EqualTo(CreateDefaultVolumeRange(i)));
            Assert.True(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeRangeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.VolumeRange, testValue);
                Assert.That(model.VolumeRange, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.volumeRange, Is.EqualTo(testValue));
                Assert.False(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_PlayType([Values] CuePlayType testValue)
        {
            var history = new AutoIncrementHistory();
            var cues = CreateTestTargetCues();
            var model = new CueInspectorModel(cues.Select(cue => new ItemCue(0, cue)).ToArray(), history,
                                              new AssetSaveService());

            for (var i = 0; i < cues.Length; ++i)
                Assert.That(cues[i].playType, Is.EqualTo(CreateDefaultPlayType(i)));
            Assert.True(model.PlayTypeObservable.Value.HasMultipleDifferentValues);

            using (model.PlayTypeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.PlayType, testValue);
                Assert.That(model.PlayType, Is.EqualTo(testValue));
                foreach (var cue in cues)
                    Assert.That(cue.playType, Is.EqualTo(testValue));
                Assert.False(model.PlayTypeObservable.Value.HasMultipleDifferentValues);
            }
        }
    }
}
