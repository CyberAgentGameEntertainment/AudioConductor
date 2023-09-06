// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal class ItemCueSheetTests
    {
        private const int ChildCount = 5;

        [Test]
        public void Properties([Random(3)] int itemId)
        {
            var cueSheet = new CueSheet
            {
                name = "TestCueSheet"
            };
            var item = new ItemCueSheet(itemId, cueSheet);

            Assert.That(item.id, Is.EqualTo(itemId));
            Assert.That(item.RawData, Is.EqualTo(cueSheet));
            Assert.That(item.depth, Is.EqualTo(-1));
            Assert.That(item.displayName, Is.EqualTo(cueSheet.name));
            Assert.That(item.Type, Is.EqualTo(ItemType.CueSheet));
            Assert.That(item.TargetId, Is.EqualTo(cueSheet.Id));
            Assert.That(item.Name, Is.EqualTo(cueSheet.name));
            Assert.That(item.CategoryId, Is.Null);
            Assert.That(item.ThrottleType, Is.EqualTo(cueSheet.throttleType));
            Assert.That(item.ThrottleLimit, Is.EqualTo(cueSheet.throttleLimit));
            Assert.That(item.Volume, Is.EqualTo(cueSheet.volume));
            Assert.That(item.VolumeRange, Is.Null);
            Assert.That(item.CuePlayType, Is.Null);
        }

        [Test]
        public void MoveChild()
        {
            var cues = new List<Cue>();
            for (var i = 0; i < ChildCount; ++i)
                cues.Add(new Cue());

            var expected = cues.ToList();

            var cueSheet = new CueSheet
            {
                name = "TestCueSheet",
                cueList = cues.ToList()
            };
            var item = new ItemCueSheet(-1, cueSheet);
            foreach (var cue in cueSheet.cueList)
                item.AddChild(new ItemCue(0, cue));

            item.MoveChild(ChildCount - 1, 1);
            expected.RemoveAt(ChildCount - 1);
            expected.Insert(1, cues[ChildCount - 1]);

            Assert.That(item.children.Count, Is.EqualTo(ChildCount));
            Assert.That(cueSheet.cueList, Is.EqualTo(expected));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i].Id, Is.EqualTo(expected[i].Id));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i], Is.EqualTo(((ItemCue)item.children[i]).RawData));
        }

        [Test]
        public void RemoveChild([Range(1, ChildCount - 1)] int index)
        {
            var cues = new List<Cue>();
            for (var i = 0; i < ChildCount; ++i)
                cues.Add(new Cue());

            var expected = cues.ToList();

            var cueSheet = new CueSheet
            {
                name = "TestCueSheet",
                cueList = cues.ToList()
            };
            var item = new ItemCueSheet(-1, cueSheet);
            foreach (var cue in cueSheet.cueList)
                item.AddChild(new ItemCue(0, cue));

            item.RemoveChild(index);
            expected.RemoveAt(index);

            Assert.That(item.children.Count, Is.EqualTo(ChildCount - 1));
            Assert.That(cueSheet.cueList, Is.EqualTo(expected));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i].Id, Is.EqualTo(expected[i].Id));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i], Is.EqualTo(((ItemCue)item.children[i]).RawData));
        }

        public void InsertChild([Range(1, ChildCount - 1)] int index)
        {
            var cues = new List<Cue>();
            for (var i = 0; i < ChildCount; ++i)
                cues.Add(new Cue());

            var expected = cues.ToList();

            var cueSheet = new CueSheet
            {
                name = "TestCueSheet",
                cueList = cues.ToList()
            };
            var item = new ItemCueSheet(-1, cueSheet);
            foreach (var cue in cueSheet.cueList)
                item.AddChild(new ItemCue(0, cue));

            var newItem = new ItemCue(1, new Cue());

            item.InsertChild(index, newItem);
            expected.Insert(index, newItem.RawData);

            Assert.That(item.children.Count, Is.EqualTo(ChildCount + 1));
            Assert.That(cueSheet.cueList, Is.EqualTo(expected));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i].Id, Is.EqualTo(expected[i].Id));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
                Assert.That(cueSheet.cueList[i], Is.EqualTo(((ItemCue)item.children[i]).RawData));
        }
    }
}
