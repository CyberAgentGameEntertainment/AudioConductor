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
        public void PlayOneShot_CueThrottle_FCFS_OneShotAndManagedCountedTogether()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(handle3.IsValid, Is.False);
        }

        [Test]
        public void PlayOneShot_CueThrottle_FCFS_Rejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            var oneShotCountBefore = _oneShotProvider.Created.Count;
            conductor.PlayOneShot(sheetHandle, "cue");

            Assert.That(_oneShotProvider.Created.Count, Is.EqualTo(oneShotCountBefore),
                "No OneShot player should be created when throttle rejects");
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_OneShotEvicted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cue");
            var oneShotPlayer = _oneShotProvider.Created[0];
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(oneShotPlayer.IsPlaying, Is.False);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_ManagedOlderThanOneShot_ManagedEvicted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.PlayOneShot(sheetHandle, "cue");
            var oneShotPlayer = _oneShotProvider.Created[0];
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);
            Assert.That(oneShotPlayer.IsPlaying, Is.True);
        }

        [Test]
        public void PlayOneShot_GlobalThrottle_FCFS_OneShotCounted()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);
        }

        [Test]
        public void PlayOneShot_SheetThrottle_FCFS_OneShotCounted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.PlayOneShot(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);
        }

        [Test]
        public void PlayOneShot_CategoryThrottle_FCFS_OneShotCounted()
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

            conductor.PlayOneShot(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);
        }

        [Test]
        public void PlayOneShot_CueThrottle_PriorityOrder_LowerPriority_Rejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            var lowTrack = CreateTrack(clip);
            lowTrack.name = "low";
            var highTrack = CreateTrack(clip, 10);
            highTrack.name = "high";
            cue.trackList.Add(lowTrack);
            cue.trackList.Add(highTrack);
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "high" });
            var oneShotCountBefore = _oneShotProvider.Created.Count;
            conductor.PlayOneShot(sheetHandle, "cue");

            Assert.That(_oneShotProvider.Created.Count, Is.EqualTo(oneShotCountBefore),
                "No OneShot player should be created when all existing players have higher priority");
        }

        [Test]
        public void PlayOneShot_CueThrottle_PriorityOrder_OneShotEvictsOneShot()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            // Fill both slots with OneShot players.
            conductor.PlayOneShot(sheetHandle, "cue");
            conductor.PlayOneShot(sheetHandle, "cue");
            var secondOneShotPlayer = _oneShotProvider.Created[1];

            // Third PlayOneShot should evict the oldest OneShot (first), shifting second to oldest position.
            conductor.PlayOneShot(sheetHandle, "cue");

            // Managed Play evicts the current oldest OneShot.
            // If the third PlayOneShot correctly evicted the first, second is now oldest and gets evicted here.
            // If the third PlayOneShot was rejected, first is still oldest and second survives.
            var handle = conductor.Play(sheetHandle, "cue");

            Assert.That(handle.IsValid, Is.True);
            Assert.That(secondOneShotPlayer.IsPlaying, Is.False,
                "Second OneShot should be evicted by managed Play, proving third PlayOneShot evicted first OneShot");
        }
    }
}
