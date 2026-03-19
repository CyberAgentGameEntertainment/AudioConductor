// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal sealed class CueSheetAssetImportCheckerTests
    {
        [Test]
        public void ShouldDuplicateCueSheet_WhenIdAlreadyExists_ReturnsTrue()
        {
            var existingIds = new HashSet<string> { "sheet-1" };

            var result = CueSheetAssetImportChecker.ShouldDuplicateCueSheet("sheet-1", existingIds);

            Assert.That(result, Is.True);
        }

        [Test]
        public void ShouldDuplicateCueSheet_WhenIdDoesNotExist_ReturnsFalse()
        {
            var existingIds = new HashSet<string> { "sheet-1" };

            var result = CueSheetAssetImportChecker.ShouldDuplicateCueSheet("sheet-2", existingIds);

            Assert.That(result, Is.False);
        }

        [Test]
        public void NormalizeCueIdsIfNeeded_WhenCueIdsAreUnique_ReturnsFalseAndKeepsIds()
        {
            var cueSheet = new CueSheet
            {
                cueList = new List<Cue>
                {
                    new() { cueId = 1 },
                    new() { cueId = 2 }
                }
            };

            var changed = CueSheetAssetImportChecker.NormalizeCueIdsIfNeeded(cueSheet);

            Assert.That(changed, Is.False);
            Assert.That(cueSheet.cueList[0].cueId, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].cueId, Is.EqualTo(2));
        }

        [Test]
        public void NormalizeCueIdsIfNeeded_WhenCueIdsContainZero_ReassignsSequentialIds()
        {
            var cueSheet = new CueSheet
            {
                cueList = new List<Cue>
                {
                    new() { cueId = 0 },
                    new() { cueId = 0 }
                }
            };

            var changed = CueSheetAssetImportChecker.NormalizeCueIdsIfNeeded(cueSheet);

            Assert.That(changed, Is.True);
            Assert.That(cueSheet.cueList[0].cueId, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].cueId, Is.EqualTo(2));
        }

        [Test]
        public void NormalizeCueIdsIfNeeded_WhenCueIdsContainDuplicateNonZero_ReassignsSequentialIds()
        {
            var cueSheet = new CueSheet
            {
                cueList = new List<Cue>
                {
                    new() { cueId = 7 },
                    new() { cueId = 7 },
                    new() { cueId = 9 }
                }
            };

            var changed = CueSheetAssetImportChecker.NormalizeCueIdsIfNeeded(cueSheet);

            Assert.That(changed, Is.True);
            Assert.That(cueSheet.cueList[0].cueId, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].cueId, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[2].cueId, Is.EqualTo(3));
        }

        [Test]
        public void ShouldDuplicateCueSheet_WhenIdIsNull_ReturnsFalse()
        {
            var existingIds = new HashSet<string> { "sheet-1" };

            var result = CueSheetAssetImportChecker.ShouldDuplicateCueSheet(null!, existingIds);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldDuplicateCueSheet_WhenIdIsEmpty_ReturnsFalse()
        {
            var existingIds = new HashSet<string> { "sheet-1" };

            var result = CueSheetAssetImportChecker.ShouldDuplicateCueSheet(string.Empty, existingIds);

            Assert.That(result, Is.False);
        }

        [Test]
        public void ShouldDuplicateCueSheet_WhenExistingIdsIsEmpty_ReturnsFalse()
        {
            var existingIds = new HashSet<string>();

            var result = CueSheetAssetImportChecker.ShouldDuplicateCueSheet("sheet-1", existingIds);

            Assert.That(result, Is.False);
        }

        [Test]
        public void NormalizeCueIdsIfNeeded_WhenCueSheetIsNull_ReturnsFalse()
        {
            var changed = CueSheetAssetImportChecker.NormalizeCueIdsIfNeeded(null!);

            Assert.That(changed, Is.False);
        }

        [Test]
        public void NormalizeCueIdsIfNeeded_WhenSingleCue_ReturnsFalse()
        {
            var cueSheet = new CueSheet
            {
                cueList = new List<Cue>
                {
                    new() { cueId = 1 }
                }
            };

            var changed = CueSheetAssetImportChecker.NormalizeCueIdsIfNeeded(cueSheet);

            Assert.That(changed, Is.False);
        }

        // --- ProcessDeletedAssets ---

        [Test]
        public void ProcessDeletedAssets_NullDeletedAssets_NoDictionaryChanges()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "path", asset } };
            var cueSheetIds = new HashSet<string> { asset.cueSheet.Id };

            CueSheetAssetImportChecker.ProcessDeletedAssets(null, cueSheetAssets, cueSheetIds);

            Assert.That(cueSheetAssets.Count, Is.EqualTo(1));
            Assert.That(cueSheetIds.Count, Is.EqualTo(1));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessDeletedAssets_EmptyDeletedAssets_NoDictionaryChanges()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "path", asset } };
            var cueSheetIds = new HashSet<string> { asset.cueSheet.Id };

            CueSheetAssetImportChecker.ProcessDeletedAssets(new List<string>(), cueSheetAssets, cueSheetIds);

            Assert.That(cueSheetAssets.Count, Is.EqualTo(1));
            Assert.That(cueSheetIds.Count, Is.EqualTo(1));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessDeletedAssets_ExistingPath_RemovesFromDictionary()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var id = asset.cueSheet.Id;
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "path", asset } };
            var cueSheetIds = new HashSet<string> { id };

            CueSheetAssetImportChecker.ProcessDeletedAssets(new[] { "path" }, cueSheetAssets, cueSheetIds);

            Assert.That(cueSheetAssets.ContainsKey("path"), Is.False);
            Assert.That(cueSheetIds.Contains(id), Is.False);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessDeletedAssets_NonExistingPath_NoDictionaryChanges()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "path", asset } };
            var cueSheetIds = new HashSet<string> { asset.cueSheet.Id };

            CueSheetAssetImportChecker.ProcessDeletedAssets(new[] { "other/path" }, cueSheetAssets, cueSheetIds);

            Assert.That(cueSheetAssets.Count, Is.EqualTo(1));
            Assert.That(cueSheetIds.Count, Is.EqualTo(1));

            Object.DestroyImmediate(asset);
        }

        // --- ProcessMovedAssets ---

        [Test]
        public void ProcessMovedAssets_NullMovedAssets_NoDictionaryChanges()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "old/path", asset } };

            CueSheetAssetImportChecker.ProcessMovedAssets(null, new[] { "old/path" }, cueSheetAssets);

            Assert.That(cueSheetAssets.ContainsKey("old/path"), Is.True);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessMovedAssets_EmptyMovedFromPaths_NoDictionaryChanges()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "old/path", asset } };

            CueSheetAssetImportChecker.ProcessMovedAssets(new[] { "new/path" }, new List<string>(), cueSheetAssets);

            Assert.That(cueSheetAssets.ContainsKey("old/path"), Is.True);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessMovedAssets_OriginalPathNotInDictionary_NoDictionaryChanges()
        {
            var cueSheetAssets = new Dictionary<string, CueSheetAsset>();

            CueSheetAssetImportChecker.ProcessMovedAssets(
                new[] { "new/path" },
                new[] { "non/existing/path" },
                cueSheetAssets);

            Assert.That(cueSheetAssets.Count, Is.EqualTo(0));
        }

        [Test]
        public void ProcessMovedAssets_ValidMove_UpdatesDictionaryKey()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "old/path", asset } };

            CueSheetAssetImportChecker.ProcessMovedAssets(
                new[] { "new/path" },
                new[] { "old/path" },
                cueSheetAssets);

            Assert.That(cueSheetAssets.ContainsKey("old/path"), Is.False);
            Assert.That(cueSheetAssets.ContainsKey("new/path"), Is.True);
            Assert.That(cueSheetAssets["new/path"], Is.SameAs(asset));

            Object.DestroyImmediate(asset);
        }

        // --- ProcessImportedAssets ---

        [Test]
        public void ProcessImportedAssets_NullImportedAssets_NoDictionaryChanges()
        {
            var cueSheetAssets = new Dictionary<string, CueSheetAsset>();
            var cueSheetIds = new HashSet<string>();

            var modified = CueSheetAssetImportChecker.ProcessImportedAssets(
                null, cueSheetAssets, cueSheetIds, _ => null);

            Assert.That(cueSheetAssets.Count, Is.EqualTo(0));
            Assert.That(modified, Is.Empty);
        }

        [Test]
        public void ProcessImportedAssets_ExistingPath_Reimport_NoDictionaryChanges()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset> { { "path", asset } };
            var cueSheetIds = new HashSet<string> { asset.cueSheet.Id };

            var modified = CueSheetAssetImportChecker.ProcessImportedAssets(
                new[] { "path" }, cueSheetAssets, cueSheetIds, _ => asset);

            Assert.That(cueSheetAssets.Count, Is.EqualTo(1));
            Assert.That(modified, Is.Empty);

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessImportedAssets_NewPathWithExistingId_DuplicatesAndAddsToDict()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var originalId = asset.cueSheet.Id;
            var cueSheetAssets = new Dictionary<string, CueSheetAsset>();
            var cueSheetIds = new HashSet<string> { originalId };

            var modified = CueSheetAssetImportChecker.ProcessImportedAssets(
                new[] { "path" }, cueSheetAssets, cueSheetIds, _ => asset);

            Assert.That(cueSheetAssets.ContainsKey("path"), Is.True);
            Assert.That(asset.cueSheet.Id, Is.Not.EqualTo(originalId));
            Assert.That(modified, Has.Count.EqualTo(1));
            Assert.That(modified[0], Is.SameAs(asset));

            Object.DestroyImmediate(asset);
        }

        [Test]
        public void ProcessImportedAssets_NewPathWithNewId_AddsToDict()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var cueSheetAssets = new Dictionary<string, CueSheetAsset>();
            var cueSheetIds = new HashSet<string>();

            var modified = CueSheetAssetImportChecker.ProcessImportedAssets(
                new[] { "path" }, cueSheetAssets, cueSheetIds, _ => asset);

            Assert.That(cueSheetAssets.ContainsKey("path"), Is.True);
            Assert.That(cueSheetIds.Contains(asset.cueSheet.Id), Is.True);
            Assert.That(modified, Is.Empty);

            Object.DestroyImmediate(asset);
        }
    }
}
