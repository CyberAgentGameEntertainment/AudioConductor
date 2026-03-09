// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public partial class AudioConductorThrottleTests
    {
        [Test]
        public void Play_MultiScope_CueStricterThanGlobal_CueLimitApplied()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 5;

            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_GlobalStricterThanCue_GlobalLimitApplied()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cueA = CreateCue("cueA", throttleLimit: 5);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB", throttleLimit: 5);
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cueA");
            var handle2 = conductor.Play(sheetHandle, "cueB");

            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_CueEvictsClearsGlobalCount()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_DoubleEviction_SecondScopeSkipsAlreadyEvicted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_DoubleEviction_CueAndCategoryShareOldest()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.PriorityOrder, throttleLimit = 1
            });

            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1, categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            // Cue and Category both have limit=1 and PriorityOrder.
            // Cue evicts handle1; AdjustCounts decrements catCount,
            // so Category passes without a second eviction.
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_CueEviction_AdjustsSheetCount()
        {
            var clip = CreateClip();
            var cueA = CreateCue("cueA", ThrottleType.PriorityOrder, 1);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB");
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueA");
            var handle2 = conductor.Play(sheetHandle, "cueB");
            var handle3 = conductor.Play(sheetHandle, "cueA");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);
            Assert.That(conductor.IsPlaying(handle2), Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_BothScopesEvictDifferentPlayers()
        {
            var clip = CreateClip();
            var cueX = CreateCue("cueX", ThrottleType.PriorityOrder, 2);
            cueX.trackList.Add(CreateTrack(clip));
            var cueY = CreateCue("cueY");
            cueY.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueX, cueY);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 3;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueY");
            var handle2 = conductor.Play(sheetHandle, "cueX");
            conductor.Play(sheetHandle, "cueY");
            conductor.Play(sheetHandle, "cueX");

            var handle5 = conductor.Play(sheetHandle, "cueX");

            Assert.That(handle5.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle2), Is.False, "Cue scope should evict handle2");
            Assert.That(conductor.IsPlaying(handle1), Is.False, "Sheet scope should evict handle1");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_AlreadyEvictedManaged_StopOldestSkips()
        {
            var clip = CreateClip();
            var cueX = CreateCue("cueX", ThrottleType.PriorityOrder, 1);
            cueX.trackList.Add(CreateTrack(clip));
            var cueY = CreateCue("cueY");
            cueY.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueX, cueY);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueX");
            var handle2 = conductor.Play(sheetHandle, "cueY");

            var handle3 = conductor.Play(sheetHandle, "cueX");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False, "handle1 evicted by cue scope");
            Assert.That(conductor.IsPlaying(handle2), Is.True,
                "handle2 not evicted - sheet count adjusted below limit");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_AlreadyEvictedOneShot_StopOldestSkips()
        {
            var clip = CreateClip();
            var cueX = CreateCue("cueX", ThrottleType.PriorityOrder, 1);
            cueX.trackList.Add(CreateTrack(clip));
            var cueY = CreateCue("cueY");
            cueY.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueX, cueY);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cueX");
            var oneShotPlayer = _oneShotProvider.Created[0];
            var handle2 = conductor.Play(sheetHandle, "cueY");

            var handle3 = conductor.Play(sheetHandle, "cueX");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(oneShotPlayer.IsPlaying, Is.False, "OneShot evicted by cue scope");
            Assert.That(conductor.IsPlaying(handle2), Is.True,
                "handle2 not evicted - sheet count adjusted below limit");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_OneShotEviction_AdjustsOtherScopeCounts()
        {
            var clip = CreateClip();
            var cueX = CreateCue("cueX", ThrottleType.PriorityOrder, 1);
            cueX.trackList.Add(CreateTrack(clip));
            var cueY = CreateCue("cueY");
            cueY.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueX, cueY);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cueX");
            var oneShotPlayer = _oneShotProvider.Created[0];
            var handle2 = conductor.Play(sheetHandle, "cueY");

            var handle3 = conductor.Play(sheetHandle, "cueX");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(oneShotPlayer.IsPlaying, Is.False);
            Assert.That(conductor.IsPlaying(handle2), Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_AllScopes_AllWithinLimits_Succeeds()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 10
            });
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 20;

            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 3, categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 5;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle1.IsValid, Is.True);
            Assert.That(handle2.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_AllScopes_MiddleScopeRejects()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 5
            });
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 5;

            var cueA = CreateCue("cueA", ThrottleType.FirstComeFirstServed, 5, categoryId);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB", ThrottleType.FirstComeFirstServed, 5, categoryId);
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cueA");
            var handle2 = conductor.Play(sheetHandle, "cueB");

            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_AllFourScopes_PriorityOrder_CascadingEvictions()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.PriorityOrder, throttleLimit = 3
            });
            _settings.throttleType = ThrottleType.PriorityOrder;
            _settings.throttleLimit = 3;

            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 3, categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 3;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");

            var handle4 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle4.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void UnregisterCueSheet_WithActivePlayers_GlobalThrottleCountNotFreed()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cueA = CreateCue("cueA");
            cueA.trackList.Add(CreateTrack(clip));
            var assetA = CreateSheetAsset(cueA);

            var cueB = CreateCue("cueB");
            cueB.trackList.Add(CreateTrack(clip));
            var assetB = CreateSheetAsset(cueB);

            using var conductor = CreateConductor();
            var sheetHandleA = conductor.RegisterCueSheet(assetA);
            var sheetHandleB = conductor.RegisterCueSheet(assetB);

            conductor.Play(sheetHandleA, "cueA");
            conductor.UnregisterCueSheet(sheetHandleA);

            var handle2 = conductor.Play(sheetHandleB, "cueB");
            Assert.That(handle2.IsValid, Is.False,
                "Orphaned player from unregistered sheet should still count against global throttle");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(assetA);
            Object.DestroyImmediate(assetB);
        }

        [Test]
        public void UnregisterCueSheet_WithActivePlayers_CategoryThrottleCountNotFreed()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 1
            });

            var cueA = CreateCue("cueA", categoryId: categoryId);
            cueA.trackList.Add(CreateTrack(clip));
            var assetA = CreateSheetAsset(cueA);

            var cueB = CreateCue("cueB", categoryId: categoryId);
            cueB.trackList.Add(CreateTrack(clip));
            var assetB = CreateSheetAsset(cueB);

            using var conductor = CreateConductor();
            var sheetHandleA = conductor.RegisterCueSheet(assetA);
            var sheetHandleB = conductor.RegisterCueSheet(assetB);

            conductor.Play(sheetHandleA, "cueA");
            conductor.UnregisterCueSheet(sheetHandleA);

            var handle2 = conductor.Play(sheetHandleB, "cueB");
            Assert.That(handle2.IsValid, Is.False,
                "Orphaned player from unregistered sheet should still count against category throttle");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(assetA);
            Object.DestroyImmediate(assetB);
        }

        [Test]
        public void Play_MultiScope_SheetAndCategory_BothLimited_StricterApplied()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 3
            });

            var cue = CreateCue("cue", categoryId: categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void UnregisterCueSheet_WithActivePlayers_PriorityOrder_OrphanedPlayerEvicted()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.PriorityOrder;
            _settings.throttleLimit = 1;

            var cueA = CreateCue("cueA");
            cueA.trackList.Add(CreateTrack(clip));
            var assetA = CreateSheetAsset(cueA);

            var cueB = CreateCue("cueB");
            cueB.trackList.Add(CreateTrack(clip));
            var assetB = CreateSheetAsset(cueB);

            using var conductor = CreateConductor();
            var sheetHandleA = conductor.RegisterCueSheet(assetA);
            var sheetHandleB = conductor.RegisterCueSheet(assetB);

            conductor.Play(sheetHandleA, "cueA");
            conductor.UnregisterCueSheet(sheetHandleA);

            var handle2 = conductor.Play(sheetHandleB, "cueB");

            Assert.That(handle2.IsValid, Is.True,
                "PriorityOrder should evict the orphaned player from unregistered sheet");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(assetA);
            Object.DestroyImmediate(assetB);
        }

        [Test]
        public void Play_MultiScope_SheetEviction_DifferentCategory_CatCountPreserved()
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

            var handle1 = conductor.Play(sheetHandle, "cueA");
            var handle2 = conductor.Play(sheetHandle, "cueB");
            // Sheet would evict cueA(catId=2), but catCount for catId=1 is not decremented,
            // so Category FCFS rejects. Atomic reject: no eviction should occur.
            var handle3 = conductor.Play(sheetHandle, "cueB");

            Assert.That(handle3.IsValid, Is.False,
                "Category FCFS should reject because catCount for catId=1 was not decremented by sheet eviction of catId=2");
            Assert.That(conductor.IsPlaying(handle1), Is.True,
                "handle1 must not be evicted — play was rejected atomically");
            Assert.That(conductor.IsPlaying(handle2), Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_MultiScope_SheetAndCategoryEvictDifferentPlayers()
        {
            var clip = CreateClip();
            const int catId1 = 1;
            const int catId2 = 2;
            _settings.categoryList.Add(new Category
            {
                id = catId1, name = "cat1",
                throttleType = ThrottleType.PriorityOrder, throttleLimit = 2
            });
            _settings.categoryList.Add(new Category
            {
                id = catId2, name = "cat2"
            });

            var cueA = CreateCue("cueA", categoryId: catId2);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB", categoryId: catId1);
            cueB.trackList.Add(CreateTrack(clip));
            var cueC = CreateCue("cueC", categoryId: catId1);
            cueC.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB, cueC);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 3;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueA");
            var handle2 = conductor.Play(sheetHandle, "cueB");
            var handle3 = conductor.Play(sheetHandle, "cueC");
            // Sheet evicts handle1(cueA, catId=2); Category evicts handle2(cueB, catId=1)
            var handle4 = conductor.Play(sheetHandle, "cueB");

            Assert.That(handle4.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False, "Sheet scope should evict handle1");
            Assert.That(conductor.IsPlaying(handle2), Is.False, "Category scope should evict handle2");
            Assert.That(conductor.IsPlaying(handle3), Is.True, "handle3 should survive");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_GlobalThrottle_FCFS_HigherPriority_ForcesEviction()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue");
            var lowTrack = CreateTrack(clip, 1);
            lowTrack.name = "low";
            var highTrack = CreateTrack(clip, 10);
            highTrack.name = "high";
            cue.trackList.Add(lowTrack);
            cue.trackList.Add(highTrack);
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "low" });
            var handle2 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "high" });

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void UnregisterCueSheet_Reregister_NewSheetScopeIndependent()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle1 = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle1, "cue");
            conductor.UnregisterCueSheet(sheetHandle1);

            var sheetHandle2 = conductor.RegisterCueSheet(asset);
            // Orphaned player has old CueSheetId; new sheet has a different CueSheetId.
            var handle2 = conductor.Play(sheetHandle2, "cue");

            Assert.That(handle2.IsValid, Is.True,
                "Re-registered sheet should have independent sheetCount from orphaned player");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
