// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Linq;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal class TrackInspectorModelTests
    {
        private const int TrackCount = 5;

        private static readonly string[] TestNameSource =
        {
            Utility.RandomString,
            Utility.RandomString,
            Utility.RandomString
        };

        private static string CreateDefaultName(int index) => $"defaultName-{index}";
        private static string CreateDefaultColor(int index) => $"defaultColor-{index}";

        private static AudioClip CreateDefaultAudioClip(int index)
            => index % 2 == 0
                ? AssetDatabase.LoadAssetAtPath<AudioClip>(TestAssetPaths.CreateAbsoluteAssetPath("testClip1.wav"))
                : AssetDatabase.LoadAssetAtPath<AudioClip>(TestAssetPaths.CreateAbsoluteAssetPath("testClip2.wav"));

        private static float CreateDefaultVolume(int index) => -index * 11.1f;
        private static float CreateDefaultVolumeRange(int index) => index * 22.2f;
        private static float CreateDefaultPitch(int index) => -index * 33.3f;
        private static float CreateDefaultPitchRange(int index) => index * 44.4f;
        private static bool CreateDefaultPitchInvert(int index) => index % 2 != 0;
        private static int CreateDefaultStartSample(int index) => index * 1000000;
        private static int CreateDefaultEndSample(int index) => index * -1000000;
        private static int CreateDefaultLoopStartSample(int index) => index * index * 1000000;
        private static bool CreateDefaultIsLoop(int index) => index % 2 == 0;
        private static int CreateDefaultRandomWeight(int index) => index * index;
        private static int CreateDefaultPriority(int index) => index + index;
        private static float CreateDefaultFadeTime(int index) => index * 55.5f;

        private static Track[] CreateTestTargetTracks()
        {
            var tracks = new Track[TrackCount];
            for (var i = 0; i < TrackCount; ++i)
                tracks[i] = new Track
                {
                    name = CreateDefaultName(i),
                    colorId = CreateDefaultColor(i),
                    audioClip = CreateDefaultAudioClip(i),
                    volume = CreateDefaultVolume(i),
                    volumeRange = CreateDefaultVolumeRange(i),
                    pitch = CreateDefaultPitch(i),
                    pitchRange = CreateDefaultPitchRange(i),
                    pitchInvert = CreateDefaultPitchInvert(i),
                    startSample = CreateDefaultStartSample(i),
                    endSample = CreateDefaultEndSample(i),
                    loopStartSample = CreateDefaultLoopStartSample(i),
                    isLoop = CreateDefaultIsLoop(i),
                    randomWeight = CreateDefaultRandomWeight(i),
                    priority = CreateDefaultPriority(i),
                    fadeTime = CreateDefaultFadeTime(i)
                };
            return tracks;
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeName(string testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(CreateDefaultName(i)));
            Assert.True(model.NameObservable.Value.HasMultipleDifferentValues);

            using (model.NameObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.Name = testValue;
                Assert.That(model.Name, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.name, Is.EqualTo(testValue));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.Name = testValue;

            Assert.That(model.Name, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(CreateDefaultName(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(CreateDefaultName(i)));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(lastValue));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeColor(string testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(CreateDefaultColor(i)));
            Assert.True(model.ColorObservable.Value.HasMultipleDifferentValues);

            using (model.ColorObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.Color = testValue;
                Assert.That(model.Color, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.colorId, Is.EqualTo(testValue));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.Color = testValue;

            Assert.That(model.Color, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Color, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Color, Is.EqualTo(CreateDefaultColor(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(CreateDefaultColor(i)));

            history.Redo();

            Assert.That(model.Color, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Color, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(lastValue));
        }

        [TestCase("testClip3.wav")]
        [TestCase("testClip4.wav")]
        [TestCase(null)]
        public void ChangeAudioClip(string assetPath)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            var testValue = assetPath == null
                ? null
                : AssetDatabase.LoadAssetAtPath<AudioClip>(TestAssetPaths.CreateAbsoluteAssetPath(assetPath));
            var samples = testValue == null ? 0 : testValue.samples;
            var startMax = ValueRangeConst.StartSample.Clamp(int.MaxValue, samples);
            var loopStartMax = ValueRangeConst.LoopStartSample.Clamp(int.MaxValue, samples);

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].audioClip, Is.EqualTo(CreateDefaultAudioClip(i)));
            Assert.True(model.AudioClipObservable.Value.HasMultipleDifferentValues);

            using (model.AudioClipObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            using (model.StartSampleObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.InRange(0, startMax)); }))
            using (model.EndSampleObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.InRange(0, samples)); }))
            using (model.LoopStartSampleObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.InRange(0, loopStartMax)); }))
            {
                model.AudioClip = testValue;
                Assert.That(model.AudioClip, Is.EqualTo(testValue));
                foreach (var track in tracks)
                {
                    Assert.That(track.audioClip, Is.EqualTo(testValue));
                    Assert.That(track.startSample, Is.EqualTo(0));
                    Assert.That(track.endSample, Is.EqualTo(samples));
                    Assert.That(track.loopStartSample, Is.InRange(0, loopStartMax));
                }

                Assert.False(model.AudioClipObservable.Value.HasMultipleDifferentValues);
            }

            history.Undo();
            for (var i = 0; i < tracks.Length; ++i)
            {
                Assert.That(tracks[i].audioClip, Is.EqualTo(CreateDefaultAudioClip(i)));
                Assert.That(tracks[i].startSample, Is.EqualTo(CreateDefaultStartSample(i)));
                Assert.That(tracks[i].endSample, Is.EqualTo(CreateDefaultEndSample(i)));
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(CreateDefaultLoopStartSample(i)));
            }

            Assert.True(model.AudioClipObservable.Value.HasMultipleDifferentValues);
            history.Redo();
            foreach (var track in tracks)
            {
                Assert.That(track.audioClip, Is.EqualTo(testValue));
                Assert.That(track.startSample, Is.InRange(0, startMax));
                Assert.That(track.endSample, Is.InRange(0, samples));
                Assert.That(track.loopStartSample, Is.InRange(0, loopStartMax));
            }

            Assert.False(model.AudioClipObservable.Value.HasMultipleDifferentValues);
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

        private void ChangeVolume(float testValue, float expected)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(CreateDefaultVolume(i)));
            Assert.True(model.VolumeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.Volume = testValue;
                Assert.That(model.Volume, Is.EqualTo(expected));
                foreach (var track in tracks)
                    Assert.That(track.volume, Is.EqualTo(expected));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.Volume = testValue;

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(CreateDefaultVolume(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(CreateDefaultVolume(i)));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(lastValue));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(CreateDefaultVolumeRange(i)));
            Assert.True(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeRangeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.VolumeRange = testValue;
                Assert.That(model.VolumeRange, Is.EqualTo(expected));
                foreach (var track in tracks)
                    Assert.That(track.volumeRange, Is.EqualTo(expected));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.VolumeRange = testValue;

            Assert.That(model.VolumeRange, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.VolumeRange, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.VolumeRange, Is.EqualTo(CreateDefaultVolumeRange(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(CreateDefaultVolumeRange(i)));

            history.Redo();

            Assert.That(model.VolumeRange, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.VolumeRange, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(lastValue));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitch, Is.EqualTo(CreateDefaultPitch(i)));
            Assert.True(model.PitchObservable.Value.HasMultipleDifferentValues);

            using (model.PitchObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.Pitch = testValue;
                Assert.That(model.Pitch, Is.EqualTo(expected));
                foreach (var track in tracks)
                    Assert.That(track.pitch, Is.EqualTo(expected));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.Pitch = testValue;

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitch, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitch, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(CreateDefaultPitch(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitch, Is.EqualTo(CreateDefaultPitch(i)));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitch, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitch, Is.EqualTo(lastValue));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchRange, Is.EqualTo(CreateDefaultPitchRange(i)));
            Assert.True(model.PitchRangeObservable.Value.HasMultipleDifferentValues);

            using (model.PitchRangeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expected)); }))
            {
                model.PitchRange = testValue;
                Assert.That(model.PitchRange, Is.EqualTo(expected));
                foreach (var track in tracks)
                    Assert.That(track.pitchRange, Is.EqualTo(expected));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.PitchRange = testValue;

            Assert.That(model.PitchRange, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchRange, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PitchRange, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchRange, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PitchRange, Is.EqualTo(CreateDefaultPitchRange(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchRange, Is.EqualTo(CreateDefaultPitchRange(i)));

            history.Redo();

            Assert.That(model.PitchRange, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchRange, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PitchRange, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchRange, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePitchInvert([Values] bool testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchInvert, Is.EqualTo(CreateDefaultPitchInvert(i)));
            Assert.True(model.PitchInvertObservable.Value.HasMultipleDifferentValues);

            using (model.PitchInvertObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.PitchInvert = testValue;
                Assert.That(model.PitchInvert, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.pitchInvert, Is.EqualTo(testValue));
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.PitchInvert = testValue;

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchInvert, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchInvert, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(CreateDefaultPitchInvert(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchInvert, Is.EqualTo(CreateDefaultPitchInvert(i)));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchInvert, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].pitchInvert, Is.EqualTo(lastValue));
        }

        [TestCase(-1, false)]
        [TestCase(1, false)]
        [TestCase(int.MaxValue, true)]
        public void ChangeStartSample(int testValue, bool mixed)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].startSample, Is.EqualTo(CreateDefaultStartSample(i)));
            Assert.True(model.StartSampleObservable.Value.HasMultipleDifferentValues);

            using (model.StartSampleObservable.Skip(1).Subscribe(v =>
                   {
                       Assert.That(v.Value,
                                   Is.InRange(0, tracks[0].audioClip.samples));
                   }))
            {
                model.StartSample = testValue;
                Assert.That(model.StartSample, Is.EqualTo(tracks[0].startSample));
                foreach (var track in tracks)
                {
                    var startMax = ValueRangeConst.StartSample.Clamp(int.MaxValue, track.audioClip.samples);
                    if (testValue < 0)
                        Assert.That(track.startSample, Is.EqualTo(0));
                    else if (startMax < testValue)
                        Assert.That(track.startSample, Is.EqualTo(startMax));
                    else
                        Assert.That(track.startSample, Is.EqualTo(testValue));
                }

                Assert.That(model.StartSampleObservable.Value.HasMultipleDifferentValues, Is.EqualTo(mixed));
            }
        }

        [Test]
        public void StartSampleHistory()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.StartSample = testValue;

            Assert.That(model.StartSample, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].startSample, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.StartSample, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].startSample, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.StartSample, Is.EqualTo(CreateDefaultStartSample(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].startSample, Is.EqualTo(CreateDefaultStartSample(i)));

            history.Redo();

            Assert.That(model.StartSample, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].startSample, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.StartSample, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].startSample, Is.EqualTo(lastValue));
        }

        [TestCase(-1, false)]
        [TestCase(1, false)]
        [TestCase(int.MaxValue, true)]
        public void ChangeEndSample(int testValue, bool mixed)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].endSample, Is.EqualTo(CreateDefaultEndSample(i)));
            Assert.True(model.EndSampleObservable.Value.HasMultipleDifferentValues);

            using (model.EndSampleObservable.Skip(1).Subscribe(v =>
                   {
                       Assert.That(v.Value,
                                   Is.InRange(0,
                                              tracks[0].audioClip.samples));
                   }))
            {
                model.EndSample = testValue;
                Assert.That(model.EndSample, Is.EqualTo(tracks[0].endSample));
                foreach (var track in tracks)
                {
                    var samples = track.audioClip.samples;
                    if (testValue < 0)
                        Assert.That(track.endSample, Is.EqualTo(0));
                    else if (samples < testValue)
                        Assert.That(track.endSample, Is.EqualTo(samples));
                    else
                        Assert.That(track.endSample, Is.EqualTo(testValue));
                }

                Assert.That(model.EndSampleObservable.Value.HasMultipleDifferentValues, Is.EqualTo(mixed));
            }
        }

        [Test]
        public void EndSampleHistory()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.EndSample = testValue;

            Assert.That(model.EndSample, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].endSample, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.EndSample, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].endSample, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.EndSample, Is.EqualTo(CreateDefaultEndSample(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].endSample, Is.EqualTo(CreateDefaultEndSample(i)));

            history.Redo();

            Assert.That(model.EndSample, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].endSample, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.EndSample, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].endSample, Is.EqualTo(lastValue));
        }

        [TestCase(-1, false)]
        [TestCase(1, false)]
        [TestCase(int.MaxValue, true)]
        public void ChangeLoopStartSample(int testValue, bool mixed)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(CreateDefaultLoopStartSample(i)));
            Assert.True(model.LoopStartSampleObservable.Value.HasMultipleDifferentValues);

            using (model.LoopStartSampleObservable.Skip(1).Subscribe(v =>
                   {
                       Assert.That(v.Value,
                                   Is.InRange(0,
                                              tracks[0].audioClip
                                                       .samples));
                   }))
            {
                model.LoopStartSample = testValue;
                Assert.That(model.LoopStartSample, Is.EqualTo(tracks[0].loopStartSample));
                foreach (var track in tracks)
                {
                    var startMax = ValueRangeConst.LoopStartSample.Clamp(int.MaxValue, track.audioClip.samples);
                    if (testValue < 0)
                        Assert.That(track.loopStartSample, Is.EqualTo(0));
                    else if (startMax < testValue)
                        Assert.That(track.loopStartSample, Is.EqualTo(startMax));
                    else
                        Assert.That(track.loopStartSample, Is.EqualTo(testValue));
                }

                Assert.That(model.LoopStartSampleObservable.Value.HasMultipleDifferentValues, Is.EqualTo(mixed));
            }
        }

        [Test]
        public void LoopStartSampleHistory()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.LoopStartSample = testValue;

            Assert.That(model.LoopStartSample, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.LoopStartSample, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.LoopStartSample, Is.EqualTo(CreateDefaultLoopStartSample(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(CreateDefaultLoopStartSample(i)));

            history.Redo();

            Assert.That(model.LoopStartSample, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.LoopStartSample, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].loopStartSample, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeIsLoop([Values] bool testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].isLoop, Is.EqualTo(CreateDefaultIsLoop(i)));
            Assert.True(model.IsLoopObservable.Value.HasMultipleDifferentValues);

            using (model.IsLoopObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.IsLoop = testValue;
                Assert.That(model.IsLoop, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.isLoop, Is.EqualTo(testValue));
                Assert.False(model.IsLoopObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void History_DifferentValue_IsLoop()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.IsLoop = testValue;

            Assert.That(model.IsLoop, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].isLoop, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.IsLoop, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].isLoop, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.IsLoop, Is.EqualTo(CreateDefaultIsLoop(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].isLoop, Is.EqualTo(CreateDefaultIsLoop(i)));

            history.Redo();

            Assert.That(model.IsLoop, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].isLoop, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.IsLoop, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].isLoop, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeRandomWeight_LessThanMin()
            => ChangeRandomWeight(ValueRangeConst.RandomWeight.Min - 1, ValueRangeConst.RandomWeight.Min);

        [Test]
        public void ChangeRandomWeight_EqualMin()
            => ChangeRandomWeight(ValueRangeConst.RandomWeight.Min, ValueRangeConst.RandomWeight.Min);

        [Test]
        public void ChangeRandomWeight_EqualMax()
            => ChangeRandomWeight(ValueRangeConst.RandomWeight.Max, ValueRangeConst.RandomWeight.Max);

        [Test]
        public void ChangeRandomWeight_InRange(
            [Random(ValueRangeConst.RandomWeight.Min, ValueRangeConst.RandomWeight.Max, 3)] int testValue)
            => ChangeRandomWeight(testValue, testValue);

        private static void ChangeRandomWeight(int testValue, int expectedValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].randomWeight, Is.EqualTo(CreateDefaultRandomWeight(i)));
            Assert.True(model.RandomWeightObservable.Value.HasMultipleDifferentValues);

            using (model.RandomWeightObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expectedValue)); }))
            {
                model.RandomWeight = testValue;
                Assert.That(model.RandomWeight, Is.EqualTo(expectedValue));
                foreach (var track in tracks)
                    Assert.That(track.randomWeight, Is.EqualTo(expectedValue));
                Assert.False(model.RandomWeightObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void RandomWeightHistory()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.RandomWeight = testValue;

            Assert.That(model.RandomWeight, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].randomWeight, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.RandomWeight, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].randomWeight, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.RandomWeight, Is.EqualTo(CreateDefaultRandomWeight(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].randomWeight, Is.EqualTo(CreateDefaultRandomWeight(i)));

            history.Redo();

            Assert.That(model.RandomWeight, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].randomWeight, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.RandomWeight, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].randomWeight, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePriority([Random(3)] int testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].priority, Is.EqualTo(CreateDefaultPriority(i)));
            Assert.True(model.PriorityObservable.Value.HasMultipleDifferentValues);

            using (model.PriorityObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.Priority = testValue;
                Assert.That(model.Priority, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.priority, Is.EqualTo(testValue));
                Assert.False(model.PriorityObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void PriorityHistory()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.Priority = testValue;

            Assert.That(model.Priority, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].priority, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Priority, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].priority, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Priority, Is.EqualTo(CreateDefaultPriority(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].priority, Is.EqualTo(CreateDefaultPriority(i)));

            history.Redo();

            Assert.That(model.Priority, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].priority, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Priority, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].priority, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeFadeTime_LessThanMin()
            => ChangeFadeTime(ValueRangeConst.FadeTime.Min - 1, ValueRangeConst.FadeTime.Min);

        [Test]
        public void ChangeFadeTime_EqualMin()
            => ChangeFadeTime(ValueRangeConst.FadeTime.Min, ValueRangeConst.FadeTime.Min);

        [Test]
        public void ChangeFadeTime_EqualMax()
            => ChangeFadeTime(ValueRangeConst.FadeTime.Max, ValueRangeConst.FadeTime.Max);

        [Test]
        public void ChangeFadeTime_InRange(
            [Random(ValueRangeConst.FadeTime.Min, ValueRangeConst.FadeTime.Max, 3)] float testValue)
            => ChangeFadeTime(testValue, testValue);

        private static void ChangeFadeTime(float testValue, float expectedValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].fadeTime, Is.EqualTo(CreateDefaultFadeTime(i)));
            Assert.True(model.FadeTimeObservable.Value.HasMultipleDifferentValues);

            using (model.FadeTimeObservable.Skip(1)
                        .Subscribe(v => { Assert.That(v.Value, Is.EqualTo(expectedValue)); }))
            {
                model.FadeTime = testValue;
                Assert.That(model.FadeTime, Is.EqualTo(expectedValue));
                foreach (var track in tracks)
                    Assert.That(track.fadeTime, Is.EqualTo(expectedValue));
                Assert.False(model.FadeTimeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void FadeTimeHistory()
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            foreach (var testValue in testValues)
                model.FadeTime = testValue;

            Assert.That(model.FadeTime, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].fadeTime, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.FadeTime, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].fadeTime, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.FadeTime, Is.EqualTo(CreateDefaultFadeTime(0)));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].fadeTime, Is.EqualTo(CreateDefaultFadeTime(i)));

            history.Redo();

            Assert.That(model.FadeTime, Is.EqualTo(sameValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].fadeTime, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.FadeTime, Is.EqualTo(lastValue));
            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].fadeTime, Is.EqualTo(lastValue));
        }

        [Test]
        public void ContainsItemId([Random(3)] int itemId)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(itemId, track)).ToArray(), history,
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
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].name, Is.EqualTo(CreateDefaultName(i)));
            Assert.True(model.NameObservable.Value.HasMultipleDifferentValues);

            using (model.NameObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Name, testValue);
                Assert.That(model.Name, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.name, Is.EqualTo(testValue));
                Assert.False(model.NameObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Color(string testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].colorId, Is.EqualTo(CreateDefaultColor(i)));
            Assert.True(model.ColorObservable.Value.HasMultipleDifferentValues);

            using (model.ColorObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Color, testValue);
                Assert.That(model.Color, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.colorId, Is.EqualTo(testValue));
                Assert.False(model.ColorObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_Volume(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)] float testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volume, Is.EqualTo(CreateDefaultVolume(i)));
            Assert.True(model.VolumeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.Volume, testValue);
                Assert.That(model.Volume, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.volume, Is.EqualTo(testValue));
                Assert.False(model.VolumeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_VolumeRange(
            [Random(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max, 3)] float testValue)
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            for (var i = 0; i < tracks.Length; ++i)
                Assert.That(tracks[i].volumeRange, Is.EqualTo(CreateDefaultVolumeRange(i)));
            Assert.True(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);

            using (model.VolumeRangeObservable.Skip(1).Subscribe(v => { Assert.That(v.Value, Is.EqualTo(testValue)); }))
            {
                model.ChangeValue(CueListTreeView.ColumnType.VolumeRange, testValue);
                Assert.That(model.VolumeRange, Is.EqualTo(testValue));
                foreach (var track in tracks)
                    Assert.That(track.volumeRange, Is.EqualTo(testValue));
                Assert.False(model.VolumeRangeObservable.Value.HasMultipleDifferentValues);
            }
        }

        [Test]
        public void ChangeValue_Exception([Values] CueListTreeView.ColumnType columnType)
        {
            if (columnType is CueListTreeView.ColumnType.Name or CueListTreeView.ColumnType.Color
                                                              or CueListTreeView.ColumnType.Volume
                                                              or CueListTreeView.ColumnType.VolumeRange)
                Assert.Pass();

            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            Assert.Throws<ArgumentOutOfRangeException>(() => model.ChangeValue(columnType, 0));
        }

        [Test]
        public void PlayClip_MultipleItems()
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();
            var model = new TrackInspectorModel(tracks.Select(track => new ItemTrack(0, track)).ToArray(), history,
                                                new AssetSaveService());

            Assert.IsNull(model.PlayClip(0));
        }

        [Test]
        public void PlayClip_SingleItems()
        {
            var history = new AutoIncrementHistory();
            var tracks = CreateTestTargetTracks();

            var cueSheet = new CueSheet();
            var cue = new Cue();
            cueSheet.cueList.Add(cue);
            var track = tracks[0];
            cue.trackList.Add(track);

            var itemCueSheet = new ItemCueSheet(-1, cueSheet);
            var itemCue = new ItemCue(0, cue);
            var itemTrack = new ItemTrack(1, track);
            itemCueSheet.AddChild(itemCue);
            itemCue.AddChild(itemTrack);

            var model = new TrackInspectorModel(new[] { itemTrack }, history, new AssetSaveService());

            var controller = model.PlayClip(0);
            Assert.IsNotNull(controller);
            controller.Dispose();
        }
    }
}
