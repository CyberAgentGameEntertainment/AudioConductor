// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal class CueIdAssignerTests
    {
        // --- HasDuplicateCueIds ---

        [Test]
        public void HasDuplicateCueIds_AllZero_ReturnsTrue()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 0 },
                new() { name = "B", cueId = 0 }
            };

            Assert.That(CueIdAssigner.HasDuplicateCueIds(cueList), Is.True);
        }

        [Test]
        public void HasDuplicateCueIds_SingleCueWithZero_ReturnsTrue()
        {
            // A single cue with cueId=0 is treated as "needs assignment" (unassigned sentinel).
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 0 }
            };

            Assert.That(CueIdAssigner.HasDuplicateCueIds(cueList), Is.True);
        }

        [Test]
        public void HasDuplicateCueIds_DuplicateNonZeroIds_ReturnsTrue()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 1 },
                new() { name = "B", cueId = 1 }
            };

            Assert.That(CueIdAssigner.HasDuplicateCueIds(cueList), Is.True);
        }

        [Test]
        public void HasDuplicateCueIds_UniqueNonZeroIds_ReturnsFalse()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 1 },
                new() { name = "B", cueId = 2 },
                new() { name = "C", cueId = 3 }
            };

            Assert.That(CueIdAssigner.HasDuplicateCueIds(cueList), Is.False);
        }

        [Test]
        public void HasDuplicateCueIds_EmptyList_ReturnsFalse()
        {
            var cueList = new List<Cue>();

            Assert.That(CueIdAssigner.HasDuplicateCueIds(cueList), Is.False);
        }

        // --- GetNextCueId ---

        [Test]
        public void GetNextCueId_EmptyList_ReturnsOne()
        {
            var cueList = new List<Cue>();

            Assert.That(CueIdAssigner.GetNextCueId(cueList), Is.EqualTo(1));
        }

        [Test]
        public void GetNextCueId_AllZero_ReturnsOne()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 0 },
                new() { name = "B", cueId = 0 }
            };

            Assert.That(CueIdAssigner.GetNextCueId(cueList), Is.EqualTo(1));
        }

        [Test]
        public void GetNextCueId_SingleCue_ReturnsTwo()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 1 }
            };

            Assert.That(CueIdAssigner.GetNextCueId(cueList), Is.EqualTo(2));
        }

        [Test]
        public void GetNextCueId_WithExistingIds_ReturnsMaxPlusOne()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 1 },
                new() { name = "B", cueId = 3 },
                new() { name = "C", cueId = 2 }
            };

            Assert.That(CueIdAssigner.GetNextCueId(cueList), Is.EqualTo(4));
        }

        // --- AssignMissingCueIds ---

        [Test]
        public void AssignMissingCueIds_AllZero_AssignsSequentialIds()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 0 },
                new() { name = "B", cueId = 0 },
                new() { name = "C", cueId = 0 }
            };

            CueIdAssigner.AssignMissingCueIds(cueList);

            var ids = new HashSet<int>();
            foreach (var cue in cueList)
            {
                Assert.That(cue.cueId, Is.GreaterThanOrEqualTo(1));
                ids.Add(cue.cueId);
            }

            Assert.That(ids.Count, Is.EqualTo(3));
        }

        [Test]
        public void AssignMissingCueIds_AlreadyAssigned_DoesNotChange()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 1 },
                new() { name = "B", cueId = 2 },
                new() { name = "C", cueId = 3 }
            };

            CueIdAssigner.AssignMissingCueIds(cueList);

            Assert.That(cueList[0].cueId, Is.EqualTo(1));
            Assert.That(cueList[1].cueId, Is.EqualTo(2));
            Assert.That(cueList[2].cueId, Is.EqualTo(3));
        }

        [Test]
        public void AssignMissingCueIds_EmptyList_DoesNotThrow()
        {
            var cueList = new List<Cue>();

            Assert.DoesNotThrow(() => CueIdAssigner.AssignMissingCueIds(cueList));
        }

        [Test]
        public void AssignMissingCueIds_PartiallyAssigned_AssignsOnlyMissing()
        {
            var cueList = new List<Cue>
            {
                new() { name = "A", cueId = 2 },
                new() { name = "B", cueId = 0 }
            };

            CueIdAssigner.AssignMissingCueIds(cueList);

            Assert.That(cueList[0].cueId, Is.EqualTo(2));
            Assert.That(cueList[1].cueId, Is.EqualTo(3));
        }
    }
}
