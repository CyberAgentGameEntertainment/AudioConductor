// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal class InspectorModelTests
    {
        [Test]
        public void InspectorType_None()
        {
            var items = Array.Empty<CueListItem>();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.That(model.InspectorType, Is.EqualTo(InspectorType.None));
            Assert.Null(model.CueInspectorModel);
            Assert.Null(model.TrackInspectorModel);
        }

        [Test]
        public void InspectorType_CueTrack()
        {
            var items = new CueListItem[] { new ItemCue(0, new Cue()), new ItemTrack(1, new Track()) };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.That(model.InspectorType, Is.EqualTo(InspectorType.CueAndTrack));
            Assert.Null(model.CueInspectorModel);
            Assert.Null(model.TrackInspectorModel);
        }

        [Test]
        public void InspectorType_Cue()
        {
            var items = new CueListItem[] { new ItemCue(0, new Cue()), new ItemCue(1, new Cue()) };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.That(model.InspectorType, Is.EqualTo(InspectorType.Cue));
            Assert.NotNull(model.CueInspectorModel);
            Assert.Null(model.TrackInspectorModel);
        }

        [Test]
        public void InspectorType_Track()
        {
            var items = new CueListItem[] { new ItemTrack(0, new Track()), new ItemTrack(1, new Track()) };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.That(model.InspectorType, Is.EqualTo(InspectorType.Track));
            Assert.Null(model.CueInspectorModel);
            Assert.NotNull(model.TrackInspectorModel);
        }

        [Test]
        public void ContainsItemId_None([Random(3)] int itemId)
        {
            var items = Array.Empty<CueListItem>();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.False(model.Contains(itemId));
            Assert.False(model.Contains(itemId + 1));
            Assert.False(model.Contains(itemId - 1));
        }

        [Test]
        public void ContainsItemId_CueTrack([Random(3)] int itemId)
        {
            var items = new CueListItem[] { new ItemCue(itemId, new Cue()), new ItemTrack(itemId + 1, new Track()) };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.False(model.Contains(itemId));
            Assert.False(model.Contains(itemId + 1));
            Assert.False(model.Contains(itemId - 1));
        }

        [Test]
        public void ContainsItemId_Cue([Random(3)] int itemId)
        {
            var items = new CueListItem[] { new ItemCue(itemId, new Cue()), new ItemCue(itemId + 1, new Cue()) };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.True(model.Contains(itemId));
            Assert.True(model.Contains(itemId + 1));
            Assert.False(model.Contains(itemId + 2));
            Assert.False(model.Contains(itemId - 1));
        }

        [Test]
        public void ContainsItemId_Track([Random(3)] int itemId)
        {
            var items = new CueListItem[]
                { new ItemTrack(itemId, new Track()), new ItemTrack(itemId + 1, new Track()) };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();

            var model = new InspectorModel(items, history, assetSaveService);

            Assert.True(model.Contains(itemId));
            Assert.True(model.Contains(itemId + 1));
            Assert.False(model.Contains(itemId + 2));
            Assert.False(model.Contains(itemId - 1));
        }
    }
}
