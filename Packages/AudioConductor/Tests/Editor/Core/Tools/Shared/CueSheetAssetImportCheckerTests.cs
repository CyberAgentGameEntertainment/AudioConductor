// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;

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
    }
}
