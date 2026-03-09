// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public partial class ConductorThrottleTests
    {
        [Test]
        public void Play_AtomicReject_SheetEvictionRevertedWhenCategoryRejects()
        {
            var clip = CreateClip();
            const int catId1 = 1;
            const int catId2 = 2;
            _settings.categoryList.Add(new Category
            {
                id = catId1, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 1
            });
            _settings.categoryList.Add(new Category
            {
                id = catId2, name = "cat2"
            });

            var cueA = CreateCue("cueA", categoryId: catId2);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB", categoryId: catId1);
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueA"); // catId2, sheetCount=1
            var handle2 = conductor.Play(sheetHandle, "cueB"); // catId1, sheetCount=2
            // Sheet is full (2/2). Playing cueB again:
            //   Sheet(PriorityOrder)would evict handle1 (cueA, catId2),
            //   but Category(FCFS)for catId1 rejects (1/1, same priority).
            // Atomic reject: no eviction should occur.
            var handle3 = conductor.Play(sheetHandle, "cueB");

            Assert.That(handle3.IsValid, Is.False,
                "Play should be rejected by Category FCFS");
            Assert.That(conductor.IsPlaying(handle1), Is.True,
                "handle1 must NOT be evicted — the play was rejected, so no side effects should occur");
            Assert.That(conductor.IsPlaying(handle2), Is.True,
                "handle2 should still be playing");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void PlayOneShot_AtomicReject_SheetEvictionRevertedWhenCategoryRejects()
        {
            var clip = CreateClip();
            const int catId1 = 1;
            const int catId2 = 2;
            _settings.categoryList.Add(new Category
            {
                id = catId1, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 1
            });
            _settings.categoryList.Add(new Category
            {
                id = catId2, name = "cat2"
            });

            var cueA = CreateCue("cueA", categoryId: catId2);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB", categoryId: catId1);
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cueA"); // catId2
            var oneShotPlayerA = _oneShotProvider.Created[0];
            conductor.PlayOneShot(sheetHandle, "cueB"); // catId1
            // Sheet full, Category FCFS rejects.
            conductor.PlayOneShot(sheetHandle, "cueB");

            Assert.That(oneShotPlayerA.IsPlaying, Is.True,
                "OneShot cueA must NOT be evicted — the play was rejected");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
