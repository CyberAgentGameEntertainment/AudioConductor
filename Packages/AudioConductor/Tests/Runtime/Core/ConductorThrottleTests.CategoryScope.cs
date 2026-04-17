// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    public partial class ConductorThrottleTests
    {
        [Test]
        public void Play_CategoryThrottle_LimitZero_Unlimited()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 0
            });

            var cue = CreateCue("cue", categoryId: categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }
        }

        [Test]
        public void Play_CategoryThrottle_NegativeLimit_Unlimited()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = -1
            });

            var cue = CreateCue("cue", categoryId: categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }
        }

        [Test]
        public void Play_CategoryNotRegistered_CategoryScopeUnlimited()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", categoryId: 99);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }
        }

        [Test]
        public void Play_CategoryThrottle_FCFS_ExceedsLimit_Rejected()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 2
            });

            var cueA = CreateCue("cueA", categoryId: categoryId);
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB", categoryId: categoryId);
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cueA");
            conductor.Play(sheetHandle, "cueB");
            var handle3 = conductor.Play(sheetHandle, "cueA");

            Assert.That(handle3.IsValid, Is.False);
        }

        [Test]
        public void Play_CategoryThrottle_FCFS_CrossSheetCounted()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            var category = new Category
            {
                id = categoryId,
                name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed,
                throttleLimit = 2
            };
            _settings.categoryList.Add(category);

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
            conductor.Play(sheetHandleB, "cueB");
            var handle3 = conductor.Play(sheetHandleA, "cueA");

            Assert.That(handle3.IsValid, Is.False);
        }

        [Test]
        public void Play_CategoryThrottle_DifferentCategories_Independent()
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
                id = catId2, name = "cat2",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 1
            });

            var cue1 = CreateCue("cue1", categoryId: catId1);
            cue1.trackList.Add(CreateTrack(clip));
            var cue2 = CreateCue("cue2", categoryId: catId2);
            cue2.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue1, cue2);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue1");
            var handle2 = conductor.Play(sheetHandle, "cue2");

            Assert.That(handle1.IsValid, Is.True);
            Assert.That(handle2.IsValid, Is.True);
        }

        [Test]
        public void Play_CategoryThrottle_PriorityOrder_CrossSheetEviction()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            var category = new Category
            {
                id = categoryId,
                name = "cat1",
                throttleType = ThrottleType.PriorityOrder,
                throttleLimit = 1
            };
            _settings.categoryList.Add(category);

            var cueA = CreateCue("cueA", categoryId: categoryId);
            cueA.trackList.Add(CreateTrack(clip));
            var assetA = CreateSheetAsset(cueA);

            var cueB = CreateCue("cueB", categoryId: categoryId);
            cueB.trackList.Add(CreateTrack(clip));
            var assetB = CreateSheetAsset(cueB);

            using var conductor = CreateConductor();
            var sheetHandleA = conductor.RegisterCueSheet(assetA);
            var sheetHandleB = conductor.RegisterCueSheet(assetB);

            var handle1 = conductor.Play(sheetHandleA, "cueA");
            var handle2 = conductor.Play(sheetHandleB, "cueB");

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);
        }

        [Test]
        public void Play_CategoryThrottle_FCFS_AfterStop_CrossSheetSlotFreed()
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

            var handle1 = conductor.Play(sheetHandleA, "cueA");
            conductor.Stop(handle1);
            var handle2 = conductor.Play(sheetHandleB, "cueB");

            Assert.That(handle2.IsValid, Is.True);
        }

        [Test]
        public void Play_CategoryThrottle_PriorityOrder_MixedPriorities_LowestEvicted()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.PriorityOrder, throttleLimit = 2
            });

            var cueA = CreateCue("cueA", categoryId: categoryId);
            var lowTrack = CreateTrack(clip, 1);
            lowTrack.name = "low";
            cueA.trackList.Add(lowTrack);
            var assetA = CreateSheetAsset(cueA);

            var cueB = CreateCue("cueB", categoryId: categoryId);
            var highTrack = CreateTrack(clip, 10);
            highTrack.name = "high";
            cueB.trackList.Add(highTrack);
            var assetB = CreateSheetAsset(cueB);

            using var conductor = CreateConductor();
            var sheetHandleA = conductor.RegisterCueSheet(assetA);
            var sheetHandleB = conductor.RegisterCueSheet(assetB);

            var handle1 = conductor.Play(sheetHandleA, "cueA");
            var handle2 = conductor.Play(sheetHandleB, "cueB");

            var handle3 = conductor.Play(sheetHandleB, "cueB");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False, "Lowest priority player should be evicted");
            Assert.That(conductor.IsPlaying(handle2), Is.True);
        }

        [Test]
        public void Play_CategoryThrottle_FCFS_AfterStop_SlotFreed()
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
            var cueB = CreateCue("cueB", categoryId: categoryId);
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueA");
            conductor.Stop(handle1);
            var handle2 = conductor.Play(sheetHandle, "cueB");

            Assert.That(handle2.IsValid, Is.True);
        }

        [Test]
        public void Play_CategoryThrottle_FCFS_AfterNaturalEnd_SlotFreed()
        {
            var clip = CreateClip();
            const int categoryId = 1;
            _settings.categoryList.Add(new Category
            {
                id = categoryId, name = "cat1",
                throttleType = ThrottleType.FirstComeFirstServed, throttleLimit = 1
            });

            var cue = CreateCue("cue", categoryId: categoryId);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            _managedProvider.Created[0].Stop();
            conductor.Update(0f);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
        }
    }
}
