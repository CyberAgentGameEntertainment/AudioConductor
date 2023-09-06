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
    internal class ItemCueTests
    {
        private const int ChildCount = 5;

        [Test]
        public void Properties([Random(3)] int itemId)
        {
            var cue = new Cue
            {
                name = "TestCue"
            };
            var item = new ItemCue(itemId, cue);

            Assert.That(item.id, Is.EqualTo(itemId));
            Assert.That(item.RawData, Is.EqualTo(cue));
            Assert.That(item.depth, Is.EqualTo(0));
            Assert.That(item.displayName, Is.EqualTo(cue.name));
            Assert.That(item.Type, Is.EqualTo(ItemType.Cue));
            Assert.That(item.TargetId, Is.EqualTo(cue.Id));
            Assert.That(item.Name, Is.EqualTo(cue.name));
            Assert.That(item.CategoryId, Is.EqualTo(cue.categoryId));
            Assert.That(item.ThrottleType, Is.EqualTo(cue.throttleType));
            Assert.That(item.ThrottleLimit, Is.EqualTo(cue.throttleLimit));
            Assert.That(item.Volume, Is.EqualTo(cue.volume));
            Assert.That(item.VolumeRange, Is.EqualTo(cue.volumeRange));
            Assert.That(item.CuePlayType, Is.EqualTo(cue.playType));
        }

        [Test]
        public void RemoveChild([Range(1, ChildCount - 1)] int index)
        {
            var tracks = new List<Track>();
            for (var i = 0; i < ChildCount; ++i)
                tracks.Add(new Track());

            var expected = tracks.ToList();

            var cue = new Cue
            {
                name = "TestCue",
                trackList = tracks.ToList()
            };
            var item = new ItemCue(0, cue);
            foreach (var track in cue.trackList)
                item.AddChild(new ItemTrack(1, track));

            item.RemoveChild(index);
            expected.RemoveAt(index);

            Assert.That(item.children.Count, Is.EqualTo(ChildCount - 1));
            Assert.That(cue.trackList, Is.EqualTo(expected));
            for (var i = 0; i < cue.trackList.Count; i++)
                Assert.That(cue.trackList[i].Id, Is.EqualTo(expected[i].Id));
            for (var i = 0; i < cue.trackList.Count; i++)
                Assert.That(cue.trackList[i], Is.EqualTo(((ItemTrack)item.children[i]).RawData));
        }

        [Test]
        public void InsertChild([Range(1, ChildCount - 1)] int index)
        {
            var tracks = new List<Track>();
            for (var i = 0; i < ChildCount; ++i)
                tracks.Add(new Track());

            var expected = tracks.ToList();

            var cue = new Cue
            {
                name = "TestCue",
                trackList = tracks.ToList()
            };
            var item = new ItemCue(0, cue);
            foreach (var track in cue.trackList)
                item.AddChild(new ItemTrack(1, track));

            var newItem = new ItemTrack(2, new Track());

            item.InsertChild(index, newItem);
            expected.Insert(index, newItem.RawData);

            Assert.That(item.children.Count, Is.EqualTo(ChildCount + 1));
            Assert.That(cue.trackList, Is.EqualTo(expected));
            for (var i = 0; i < cue.trackList.Count; i++)
                Assert.That(cue.trackList[i].Id, Is.EqualTo(expected[i].Id));
            for (var i = 0; i < cue.trackList.Count; i++)
                Assert.That(cue.trackList[i], Is.EqualTo(((ItemTrack)item.children[i]).RawData));
        }
    }
}
