// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    public partial class ConductorThrottleTests
    {
        [Test]
        public void Play_SheetThrottle_LimitZero_Unlimited()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 0;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }
        }

        [Test]
        public void Play_SheetThrottle_NegativeLimit_Unlimited()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = -1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }
        }

        [Test]
        public void Play_SheetThrottle_FCFS_WithinLimit_Succeed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 3;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 3; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }
        }

        [Test]
        public void Play_SheetThrottle_FCFS_ExceedsLimit_Rejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.False);
        }

        [Test]
        public void Play_SheetThrottle_FCFS_DifferentCuesCountedTogether()
        {
            var clip = CreateClip();
            var cueA = CreateCue("cueA");
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB");
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cueA");
            conductor.Play(sheetHandle, "cueB");
            var handle3 = conductor.Play(sheetHandle, "cueA");

            Assert.That(handle3.IsValid, Is.False);
        }

        [Test]
        public void Play_SheetThrottle_PriorityOrder_OldestStopped()
        {
            var clip = CreateClip();
            var cueA = CreateCue("cueA");
            cueA.trackList.Add(CreateTrack(clip));
            var cueB = CreateCue("cueB");
            cueB.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cueA, cueB);
            asset.cueSheet.throttleType = ThrottleType.PriorityOrder;
            asset.cueSheet.throttleLimit = 2;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cueA");
            conductor.Play(sheetHandle, "cueB");
            var handle3 = conductor.Play(sheetHandle, "cueA");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);
        }

        [Test]
        public void Play_SheetThrottle_FCFS_AfterStop_SlotFreed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Stop(handle1);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
        }

        [Test]
        public void Play_SheetThrottle_FCFS_AfterNaturalEnd_SlotFreed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);
            asset.cueSheet.throttleType = ThrottleType.FirstComeFirstServed;
            asset.cueSheet.throttleLimit = 1;

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            _managedProvider.Created[0].IsPlaying = false;
            conductor.Update(0f);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);
        }
    }
}
