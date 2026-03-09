// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Enums;
using NUnit.Framework;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public partial class AudioConductorThrottleTests
    {
        [Test]
        public void Play_GlobalThrottle_LimitZero_Unlimited()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 0;

            var cue = CreateCue("cue");
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
        public void Play_GlobalThrottle_NegativeLimit_Unlimited()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = -1;

            var cue = CreateCue("cue");
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
        public void Play_GlobalThrottle_FCFS_WithinLimit_Succeed()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 3;

            var cue = CreateCue("cue");
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
        public void Play_GlobalThrottle_FCFS_ExceedsLimit_Rejected()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 2;

            var cue = CreateCue("cue");
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
        public void Play_GlobalThrottle_PriorityOrder_OldestStopped()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.PriorityOrder;
            _settings.throttleLimit = 2;

            var cue = CreateCue("cue");
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
        public void Play_GlobalThrottle_PriorityOrder_MixedPriorities_LowestEvicted()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.PriorityOrder;
            _settings.throttleLimit = 2;

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
            conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "high" });
            var handle3 = conductor.Play(sheetHandle, "cue", new PlayOptions { TrackName = "high" });

            Assert.That(handle3.IsValid, Is.True);
            Assert.That(conductor.IsPlaying(handle1), Is.False);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_GlobalThrottle_FCFS_AfterStop_SlotFreed()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            var handle1 = conductor.Play(sheetHandle, "cue");
            conductor.Stop(handle1);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_GlobalThrottle_FCFS_FadeOutPlayerStillCounted()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue");
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
        public void Play_GlobalThrottle_FCFS_AfterNaturalEnd_SlotFreed()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 1;

            var cue = CreateCue("cue");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheetHandle = conductor.RegisterCueSheet(asset);

            conductor.Play(sheetHandle, "cue");
            _managedProvider.Created[0].IsPlaying = false;
            conductor.Update(0f);
            var handle2 = conductor.Play(sheetHandle, "cue");

            Assert.That(handle2.IsValid, Is.True);

            Object.DestroyImmediate(clip);
            Object.DestroyImmediate(asset);
        }

        [Test]
        public void Play_GlobalThrottle_FCFS_AfterStopAll_AllSlotsFreed()
        {
            var clip = CreateClip();
            _settings.throttleType = ThrottleType.FirstComeFirstServed;
            _settings.throttleLimit = 2;

            var cue = CreateCue("cue");
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
    }
}
