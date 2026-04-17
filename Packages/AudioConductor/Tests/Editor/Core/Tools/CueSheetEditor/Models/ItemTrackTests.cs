// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Enums;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Tests
{
    internal class ItemTrackTests
    {
        [Test]
        public void Properties([Random(3)] int itemId)
        {
            var track = new Track
            {
                name = "TestTrack"
            };
            var item = new ItemTrack(itemId, track);

            Assert.That(item.id, Is.EqualTo(itemId));
            Assert.That(item.RawData, Is.EqualTo(track));
            Assert.That(item.depth, Is.EqualTo(1));
            Assert.That(item.displayName, Is.EqualTo(track.name));
            Assert.That(item.Type, Is.EqualTo(ItemType.Track));
            Assert.That(item.TargetId, Is.EqualTo(track.Id));
            Assert.That(item.Name, Is.EqualTo(track.name));
            Assert.That(item.CategoryId, Is.Null);
            Assert.That(item.ThrottleType, Is.Null);
            Assert.That(item.ThrottleLimit, Is.Null);
            Assert.That(item.Volume, Is.EqualTo(track.volume));
            Assert.That(item.VolumeRange, Is.EqualTo(track.volumeRange));
            Assert.That(item.CuePlayType, Is.Null);
        }
    }
}
