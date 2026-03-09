// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public partial class ConductorThrottleTests
    {
        [Test]
        public void Play_CueThrottle_FCFS_SamePriority_Rejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip, 5));
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
        public void Play_CueThrottle_FCFS_HigherPriority_ForcesEviction()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            var lowTrack = CreateTrack(clip, 1);
            lowTrack.name = "low";
            var highTrack = CreateTrack(clip, 99);
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
        public void Play_CueThrottle_PriorityOrder_MixedPriorities_LowestEvicted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 3);
            var track1 = CreateTrack(clip, 1);
            track1.name = "t1";
            var track5 = CreateTrack(clip, 5);
            track5.name = "t5";
            var track3 = CreateTrack(clip, 3);
            track3.name = "t3";
            var track5b = CreateTrack(clip, 5);
            track5b.name = "t5b";
            cue.trackList.Add(track1);
            cue.trackList.Add(track5);
            cue.trackList.Add(track3);
            cue.trackList.Add(track5b);
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "t1" });
            conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "t5" });
            conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "t3" });
            var handle4 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "t5b" });

            Assert.That(handle4.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_EqualToMinPriority_OldestEvicted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            cue.trackList.Add(CreateTrack(clip, 5));
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
        public void Play_CueThrottle_PriorityOrder_SamePriority_OldestEvicted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            var handle2 = conductor.Play(sheetHandle, "cue");
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);
            Assert.That(conductor.IsPlaying(handle2), Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_LowPriorityRejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            var highTrack = CreateTrack(clip, 10);
            highTrack.name = "high";
            var lowTrack = CreateTrack(clip);
            lowTrack.name = "low";
            cue.trackList.Add(highTrack);
            cue.trackList.Add(lowTrack);
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "high" });
            var handle2 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "low" });

            Assert.That(handle1.IsValid, Is.True);
            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_HighPriorityEvicts()
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

            var handle1 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "low" });
            var handle2 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "high" });

            Assert.That(handle2.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_RepeatedEvictions_AllSucceed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed via eviction");
            }

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_CrossCueLowPriority_DoesNotAffectOtherCueDecision()
        {
            var clip = CreateClip();
            var cueA = CreateCue("cueA");
            cueA.trackList.Add(CreateTrack(clip, 1));
            var cueB = CreateCue("cueB", ThrottleType.FirstComeFirstServed, 1);
            var cueBTrack5 = CreateTrack(clip, 5);
            cueBTrack5.name = "t5";
            var cueBTrack3 = CreateTrack(clip, 3);
            cueBTrack3.name = "t3";
            cueB.trackList.Add(cueBTrack5);
            cueB.trackList.Add(cueBTrack3);
            var asset = CreateSheetAsset(cueA, cueB);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cueA");
            conductor.Play(sheetHandle, "cueB", new PlayOptions { TrackName = "t5" });
            var handle3 = conductor.Play(sheetHandle, "cueB", new PlayOptions { TrackName = "t3" });

            Assert.That(handle3.IsValid, Is.False,
                "CueA's low priority should not affect CueB's FCFS decision");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_CrossCueLowPriority_DoesNotAffectEviction()
        {
            var clip = CreateClip();
            var cueA = CreateCue("cueA");
            cueA.trackList.Add(CreateTrack(clip, 1));
            var cueB = CreateCue("cueB", ThrottleType.PriorityOrder, 1);
            var cueBTrack5 = CreateTrack(clip, 5);
            cueBTrack5.name = "t5";
            var cueBTrack3 = CreateTrack(clip, 3);
            cueBTrack3.name = "t3";
            cueB.trackList.Add(cueBTrack5);
            cueB.trackList.Add(cueBTrack3);
            var asset = CreateSheetAsset(cueA, cueB);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cueA");
            conductor.Play(sheetHandle, "cueB", new PlayOptions { TrackName = "t5" });
            var handle3 = conductor.Play(sheetHandle, "cueB", new PlayOptions { TrackName = "t3" });

            Assert.That(handle3.IsValid, Is.False,
                "CueB's min priority (5) > 3, so new play should be rejected even though CueA has priority 1");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
