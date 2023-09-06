// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
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
