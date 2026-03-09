// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core.Enums;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public partial class AudioConductorThrottleTests
    {
        [Test]
        public void Play_CueThrottle_LimitZero_Unlimited()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", throttleLimit: 0);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_NegativeLimit_Unlimited()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", throttleLimit: -1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 5; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_WithinLimit_Succeed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 3);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 3; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed");
            }

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_ExactlyAtLimit_NextRejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 3);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            for (var i = 0; i < 3; i++)
            {
                var handle = conductor.Play(sheetHandle, "cue");
                Assert.That(handle.IsValid, Is.True, $"Play #{i} should succeed (within limit)");
            }

            var rejected = conductor.Play(sheetHandle, "cue");
            Assert.That(rejected.IsValid, Is.False, "Play at count==limit should be rejected");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_ExceedsLimit_Rejected()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_AfterStop_NewPlaySucceeds()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");
            conductor.Stop(handle1);
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_FadeOutPlayerStillCounted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Stop(handle1, 1.0f);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_AfterFadeOutComplete_SlotFreed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Stop(handle1, 0.1f);
            // Simulate fade completion: clear IsFading so Update removes the playback
            var player1 = _managedProvider.Created[0];
            player1.IsFading = false;
            conductor.Update(0f);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_AfterNaturalEnd_SlotFreed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            // Simulate natural playback end
            var player1 = _managedProvider.Created[0];
            player1.IsPlaying = false;
            conductor.Update(0f);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_PausedPlayerStillCounted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Pause(handle1);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_PriorityOrder_ExceedsLimit_OldestStopped()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.PriorityOrder, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");
            var handle3 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_AfterStopAll_AllSlotsFreed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 2);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            conductor.Play(sheetHandle, "cue");
            conductor.StopAll();

            var handle3 = conductor.Play(sheetHandle, "cue");
            var handle4 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(handle4.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_CueThrottle_FCFS_ResumedPlayerStillCounted()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue", ThrottleType.FirstComeFirstServed, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Pause(handle1);
            conductor.Resume(handle1);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.False,
                "Resumed player should still occupy the slot");

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }
    }
}
