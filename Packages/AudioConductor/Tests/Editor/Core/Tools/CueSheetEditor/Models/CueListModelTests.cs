// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal class CueListModelTests
    {
        private static readonly string[] TestNameSource =
        {
            Utility.RandomString,
            Utility.RandomString,
            Utility.RandomString
        };

        private static CueSheet CreateTestCueSheet() => new()
        {
            name = "TestCueSheet",
            cueList = new List<Cue>
            {
                new()
                {
                    name = "TestCue0",
                    colorId = "Color1",
                    trackList = new List<Track>
                    {
                        new()
                        {
                            name = "TestTrackA",
                            colorId = "Color1"
                        },
                        new()
                        {
                            name = "TestTrackB",
                            colorId = "Color2"
                        }
                    }
                },
                new()
                {
                    name = "TestCue1",
                    trackList = new List<Track>
                    {
                        new()
                        {
                            name = "TestTrackC"
                        },
                        new()
                        {
                            name = "TestTrackD",
                            colorId = "Color3"
                        },
                        new()
                        {
                            name = "TestTrackE"
                        }
                    }
                },
                new()
                {
                    name = "TestCue2",
                    colorId = "Color4",
                    trackList = new List<Track>
                    {
                        new()
                        {
                            name = "TestTrackF"
                        }
                    }
                }
            }
        };

        [Test]
        public void Properties()
        {
            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            Assert.IsNotNull(model.Root);
            Assert.IsNull(model.CueListTreeViewState);
            Assert.IsNotNull(model.MoveAsObservable);
            Assert.IsNotNull(model.AddAsObservable);
            Assert.IsNotNull(model.RemoveAsObservable);
        }

        [Test]
        public void MoveItem_Cue()
        {
            const int targetIndex = 2;
            const int newIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var initial = cueSheet.cueList.ToList();
            var expected = cueSheet.cueList.ToList();
            var target = expected[targetIndex];
            expected.Remove(target);
            expected.Insert(newIndex, target);

            model.MoveItem(new ItemMoveOperationRequestedEvent(targetIndex, newIndex, null,
                                                               (CueListItem)model.Root.children[targetIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(expected));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i].trackList, Is.EqualTo(expected[i].trackList));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i].trackList, Is.EqualTo(initial[i].trackList));
        }

        [Test]
        public void MoveItem_Track_SameParent()
        {
            const int cueIndex = 1;
            const int targetIndex = 1;
            const int newIndex = 2;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var beforeCueList = cueSheet.cueList.ToList();
            var initial = cueSheet.cueList[cueIndex].trackList.ToList();
            var expected = cueSheet.cueList[cueIndex].trackList.ToList();
            var target = expected[targetIndex];
            expected.Remove(target);
            expected.Insert(newIndex, target);

            model.MoveItem(new ItemMoveOperationRequestedEvent(targetIndex, newIndex, (CueListItem)model.Root.children[cueIndex],
                                                               (CueListItem)model.Root.children[cueIndex]
                                                                   .children[targetIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(expected));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(initial));
        }

        [Test]
        public void MoveItem_Track_DifferentParent()
        {
            const int fromCueIndex = 1;
            const int toCueIndex = 0;
            const int targetIndex = 1;
            const int newIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var beforeCueList = cueSheet.cueList.ToList();
            var fromInitial = cueSheet.cueList[fromCueIndex].trackList.ToList();
            var toInitial = cueSheet.cueList[toCueIndex].trackList.ToList();
            var fromExpected = cueSheet.cueList[fromCueIndex].trackList.ToList();
            var toExpected = cueSheet.cueList[toCueIndex].trackList.ToList();
            var target = fromExpected[targetIndex];
            fromExpected.Remove(target);
            toExpected.Insert(newIndex, target);

            model.MoveItem(new ItemMoveOperationRequestedEvent(targetIndex, newIndex, (CueListItem)model.Root.children[toCueIndex],
                                                               (CueListItem)model.Root.children[fromCueIndex]
                                                                   .children[targetIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[fromCueIndex].trackList, Is.EqualTo(fromExpected));
            Assert.That(cueSheet.cueList[toCueIndex].trackList, Is.EqualTo(toExpected));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[fromCueIndex].trackList, Is.EqualTo(fromInitial));
            Assert.That(cueSheet.cueList[toCueIndex].trackList, Is.EqualTo(toInitial));
        }

        [Test]
        public void AddCue()
        {
            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var initial = cueSheet.cueList.ToList();

            model.AddCue(new CueAddOperationRequestedEvent());

            Assert.That(cueSheet.cueList.Count, Is.EqualTo(initial.Count + 1));
            Assert.That(cueSheet.cueList.Take(initial.Count).ToList(), Is.EqualTo(initial));
            Assert.That(cueSheet.cueList.Last().name, Is.EqualTo("New Cue"));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
        }

        [Test]
        public void AddTrack()
        {
            const int cueIndex = 2;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var beforeCueList = cueSheet.cueList.ToList();
            var initial = cueSheet.cueList[cueIndex].trackList.ToList();

            model.AddTrack(new TrackAddOperationRequestedEvent((ItemCue)model.Root.children[cueIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList.Count, Is.EqualTo(initial.Count + 1));
            Assert.That(cueSheet.cueList[cueIndex].trackList.Take(initial.Count).ToList(), Is.EqualTo(initial));
            Assert.That(cueSheet.cueList[cueIndex].trackList.Last().name, Is.EqualTo("New Track"));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(initial));
        }

        [Test]
        public void RemoveItem_Cue()
        {
            const int cueIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var initial = cueSheet.cueList.ToList();
            var expected = cueSheet.cueList.ToList();
            expected.RemoveAt(cueIndex);

            model.RemoveItem(new ItemRemoveOperationRequestedEvent((ItemCue)model.Root.children[cueIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(expected));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
        }

        [Test]
        public void RemoveItem_Track()
        {
            const int cueIndex = 1;
            const int trackIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var beforeCueList = cueSheet.cueList.ToList();
            var initial = cueSheet.cueList[cueIndex].trackList.ToList();
            var expected = cueSheet.cueList[cueIndex].trackList.ToList();
            expected.RemoveAt(trackIndex);

            model.RemoveItem(new ItemRemoveOperationRequestedEvent((ItemTrack)model.Root.children[cueIndex]
                                                                       .children[trackIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(expected));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(initial));
        }

        [Test]
        public void DuplicateItem_Cue()
        {
            const int cueIndex = 1;
            const int insertIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var initial = cueSheet.cueList.ToList();

            model.DuplicateItem(new ItemDuplicateOperationRequestedEvent(insertIndex, null,
                                                                         (ItemCue)model.Root.children[cueIndex]));

            Assert.That(cueSheet.cueList.Count, Is.EqualTo(initial.Count + 1));
            Assert.That(cueSheet.cueList[insertIndex].Id, !Is.EqualTo(initial[cueIndex].Id));
            Assert.That(cueSheet.cueList[insertIndex].name, Is.EqualTo(initial[cueIndex].name));
            for (var i = 0; i < cueSheet.cueList[insertIndex].trackList.Count; ++i)
            {
                var track = cueSheet.cueList[insertIndex].trackList[i];
                Assert.That(track.Id, !Is.EqualTo(initial[cueIndex].trackList[i].Id));
                Assert.That(track.name, Is.EqualTo(initial[cueIndex].trackList[i].name));
            }

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
        }

        [Test]
        public void DuplicateItem_Track_SameParent()
        {
            const int cueIndex = 2;
            const int trackIndex = 0;
            const int insertIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var beforeCueList = cueSheet.cueList.ToList();
            var initial = cueSheet.cueList[cueIndex].trackList.ToList();

            model.DuplicateItem(new ItemDuplicateOperationRequestedEvent(insertIndex,
                                                                         (ItemCue)model.Root.children[cueIndex],
                                                                         (ItemTrack)model.Root.children[cueIndex]
                                                                             .children[trackIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList.Count, Is.EqualTo(initial.Count + 1));
            Assert.That(cueSheet.cueList[cueIndex].trackList[insertIndex].Id, !Is.EqualTo(initial[trackIndex].Id));
            Assert.That(cueSheet.cueList[cueIndex].trackList[insertIndex].name, Is.EqualTo(initial[trackIndex].name));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(initial));
        }

        [Test]
        public void DuplicateItem_Track_DifferentParent()
        {
            const int cueIndex = 0;
            const int trackIndex = 1;
            const int parentIndex = 1;
            const int insertIndex = 2;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var beforeCueList = cueSheet.cueList.ToList();
            var fromInitial = cueSheet.cueList[cueIndex].trackList.ToList();
            var toInitial = cueSheet.cueList[parentIndex].trackList.ToList();

            model.DuplicateItem(new ItemDuplicateOperationRequestedEvent(insertIndex,
                                                                         (ItemCue)model.Root.children[parentIndex],
                                                                         (ItemTrack)model.Root.children[cueIndex]
                                                                             .children[trackIndex]));

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(fromInitial));
            Assert.That(cueSheet.cueList[parentIndex].trackList.Count, Is.EqualTo(toInitial.Count + 1));
            Assert.That(cueSheet.cueList[parentIndex].trackList[insertIndex].Id,
                        !Is.EqualTo(fromInitial[trackIndex].Id));
            Assert.That(cueSheet.cueList[parentIndex].trackList[insertIndex].name,
                        Is.EqualTo(fromInitial[trackIndex].name));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(beforeCueList));
            Assert.That(cueSheet.cueList[cueIndex].trackList, Is.EqualTo(fromInitial));
            Assert.That(cueSheet.cueList[parentIndex].trackList, Is.EqualTo(toInitial));
        }

        [Test]
        public void AddAsset_ParentRoot()
        {
            const int insertIndex = 0;
            var asset =
                AssetDatabase.LoadAssetAtPath<AudioClip>(TestAssetPaths.CreateAbsoluteAssetPath("testClip1.wav"));

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var initial = cueSheet.cueList.ToList();

            model.AddAsset(new AssetAddOperationRequestedEvent(insertIndex, (CueListItem)model.Root, asset));

            Assert.That(cueSheet.cueList.Count, Is.EqualTo(initial.Count + 1));
            Assert.That(cueSheet.cueList[insertIndex].name, Is.EqualTo(asset.name));
            Assert.That(cueSheet.cueList[insertIndex].trackList.Count, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[insertIndex].trackList[0].name, Is.EqualTo(asset.name));
            Assert.That(cueSheet.cueList[insertIndex].trackList[0].audioClip, Is.EqualTo(asset));
            Assert.That(cueSheet.cueList[insertIndex].trackList[0].endSample, Is.EqualTo(asset.samples));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
        }

        [Test]
        public void AddAsset_ParentCue()
        {
            const int insertIndex = 0;
            const int parentIndex = 2;
            var asset =
                AssetDatabase.LoadAssetAtPath<AudioClip>(TestAssetPaths.CreateAbsoluteAssetPath("testClip1.wav"));

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var initial = cueSheet.cueList.ToList();
            var parentInitial = cueSheet.cueList[parentIndex].trackList.ToList();

            model.AddAsset(new AssetAddOperationRequestedEvent(insertIndex, (ItemCue)model.Root.children[parentIndex],
                                                               asset));

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
            Assert.That(cueSheet.cueList[parentIndex].trackList.Count, Is.EqualTo(parentInitial.Count + 1));
            Assert.That(cueSheet.cueList[parentIndex].trackList[insertIndex].name, Is.EqualTo(asset.name));
            Assert.That(cueSheet.cueList[parentIndex].trackList[insertIndex].audioClip, Is.EqualTo(asset));
            Assert.That(cueSheet.cueList[parentIndex].trackList[insertIndex].endSample, Is.EqualTo(asset.samples));

            history.Undo();

            Assert.That(cueSheet.cueList, Is.EqualTo(initial));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_ContainsInspector(string testValue)
        {
            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue0 = (CueListItem)model.Root.children[0];
            var initial0 = targetCue0.Name;
            var targetCue1 = (CueListItem)model.Root.children[2];
            var initial1 = targetCue1.Name;
            var targetCues = new[] { targetCue0, targetCue1 };

            model.CreateInspectorModel(targetCues);
            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Name, testValue, targetCue1.id));

            Assert.That(targetCue0.Name, Is.EqualTo(testValue));
            Assert.That(cueSheet.cueList[0].name, Is.EqualTo(testValue));
            Assert.That(cueSheet.cueList[1].name, !Is.EqualTo(testValue));
            Assert.That(targetCue1.Name, Is.EqualTo(testValue));
            Assert.That(cueSheet.cueList[2].name, Is.EqualTo(testValue));

            history.Undo();

            Assert.That(targetCue0.Name, Is.EqualTo(initial0));
            Assert.That(cueSheet.cueList[0].name, Is.EqualTo(initial0));
            Assert.That(targetCue1.Name, Is.EqualTo(initial1));
            Assert.That(cueSheet.cueList[2].name, Is.EqualTo(initial1));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Cue_Name(string testValue)
        {
            const int cueIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].name;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Name, testValue, targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].name, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList)
            {
                if (cue != targetCue.RawData)
                    Assert.That(cue.name, !Is.EqualTo(testValue));
                foreach (var track in cue.trackList)
                    Assert.That(track.name, !Is.EqualTo(testValue));
            }

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].name, Is.EqualTo(initial));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Cue_Color(string testValue)
        {
            const int cueIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].colorId;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Color, testValue, targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].colorId, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList)
            {
                if (cue != targetCue.RawData)
                    Assert.That(cue.colorId, !Is.EqualTo(testValue));
                foreach (var track in cue.trackList)
                    Assert.That(track.colorId, !Is.EqualTo(testValue));
            }

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].colorId, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Cue_CategoryId([Random(3)] int testValue)
        {
            const int cueIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].categoryId;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Category, testValue,
                                                          targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].categoryId, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList.Where(cue => cue != targetCue.RawData))
                Assert.That(cue.categoryId, !Is.EqualTo(testValue));

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].categoryId, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Cue_ThrottleType()
        {
            const int cueIndex = 2;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].throttleType;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.ThrottleType,
                                                          ThrottleType.FirstComeFirstServed, targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].throttleType, Is.EqualTo(ThrottleType.FirstComeFirstServed));
            foreach (var cue in cueSheet.cueList.Where(cue => cue != targetCue.RawData))
                Assert.That(cue.throttleType, Is.EqualTo(default(ThrottleType)));

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].throttleType, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Cue_ThrottleLimit(
            [Random(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Max, 3)] int testValue)
        {
            const int cueIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].throttleLimit;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.ThrottleLimit, testValue,
                                                          targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].throttleLimit, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList.Where(cue => cue != targetCue.RawData))
                Assert.That(cue.throttleLimit, !Is.EqualTo(testValue));

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].throttleLimit, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Cue_Volume(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)] float testValue)
        {
            const int cueIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].volume;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Volume, testValue, targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].volume, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList.Where(cue => cue != targetCue.RawData))
                Assert.That(cue.volume, !Is.EqualTo(testValue));

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].volume, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Cue_VolumeRange(
            [Random(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max, 3)] float testValue)
        {
            const int cueIndex = 2;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].volumeRange;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.VolumeRange, testValue,
                                                          targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].volumeRange, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList.Where(cue => cue != targetCue.RawData))
                Assert.That(cue.volumeRange, !Is.EqualTo(testValue));

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].volumeRange, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Cue_PlayType()
        {
            const int cueIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetCue = (ItemCue)model.Root.children[cueIndex];
            var initial = cueSheet.cueList[cueIndex].playType;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.PlayType, CuePlayType.Random,
                                                          targetCue.id));

            Assert.That(cueSheet.cueList[cueIndex].playType, Is.EqualTo(CuePlayType.Random));
            foreach (var cue in cueSheet.cueList.Where(cue => cue != targetCue.RawData))
                Assert.That(cue.playType, Is.EqualTo(default(CuePlayType)));

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].playType, Is.EqualTo(initial));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Track_Name(string testValue)
        {
            const int cueIndex = 0;
            const int trackIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetTrack = (ItemTrack)model.Root.children[cueIndex].children[trackIndex];
            var initial = targetTrack.Name;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Name, testValue, targetTrack.id));

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].name, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList)
            {
                Assert.That(cue.name, !Is.EqualTo(testValue));
                foreach (var track in cue.trackList.Where(track => track != targetTrack.RawData))
                    Assert.That(track.name, !Is.EqualTo(testValue));
            }

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].name, Is.EqualTo(initial));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeValue_Track_Color(string testValue)
        {
            const int cueIndex = 0;
            const int trackIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetTrack = (ItemTrack)model.Root.children[cueIndex].children[trackIndex];
            var initial = targetTrack.ColorId;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Color, testValue, targetTrack.id));

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].colorId, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList)
            {
                Assert.That(cue.colorId, !Is.EqualTo(testValue));
                foreach (var track in cue.trackList.Where(track => track != targetTrack.RawData))
                    Assert.That(track.colorId, !Is.EqualTo(testValue));
            }

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].colorId, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Track_Volume(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)] float testValue)
        {
            const int cueIndex = 1;
            const int trackIndex = 1;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetTrack = (ItemTrack)model.Root.children[cueIndex].children[trackIndex];
            var initial = targetTrack.Volume;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.Volume, testValue,
                                                          targetTrack.id));

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].volume, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList)
            {
                Assert.That(cue.name, !Is.EqualTo(testValue));
                foreach (var track in cue.trackList.Where(track => track != targetTrack.RawData))
                    Assert.That(track.volume, !Is.EqualTo(testValue));
            }

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].volume, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Track_VolumeRange(
            [Random(ValueRangeConst.VolumeRange.Min, ValueRangeConst.VolumeRange.Max, 3)] float testValue)
        {
            const int cueIndex = 2;
            const int trackIndex = 0;

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetTrack = (ItemTrack)model.Root.children[cueIndex].children[trackIndex];
            var initial = targetTrack.VolumeRange;

            model.ChangeValue(new ColumnValueChangedEvent(CueListTreeView.ColumnType.VolumeRange, testValue,
                                                          targetTrack.id));

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].volumeRange, Is.EqualTo(testValue));
            foreach (var cue in cueSheet.cueList)
            {
                Assert.That(cue.name, !Is.EqualTo(testValue));
                foreach (var track in cue.trackList.Where(track => track != targetTrack.RawData))
                    Assert.That(track.volumeRange, !Is.EqualTo(testValue));
            }

            history.Undo();

            Assert.That(cueSheet.cueList[cueIndex].trackList[trackIndex].volumeRange, Is.EqualTo(initial));
        }

        [Test]
        public void ChangeValue_Track_Exception([Values] CueListTreeView.ColumnType columnType)
        {
            if (columnType is CueListTreeView.ColumnType.Name
                              or CueListTreeView.ColumnType.Color
                              or CueListTreeView.ColumnType.Volume
                              or CueListTreeView.ColumnType.VolumeRange)
                Assert.Pass();

            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new CueListModel(cueSheet, history, new AssetSaveService(), null);

            var targetTrack = model.Root.children[0].children[0];

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                model.ChangeValue(new ColumnValueChangedEvent(columnType, 0,
                                                              targetTrack.id));
            });
        }
    }
}
