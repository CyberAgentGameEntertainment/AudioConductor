// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class ConductorCallbackTests
    {
        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _managedProvider = new ClockablePlayerProvider();
            _oneShotProvider = new ClockablePlayerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
            foreach (var obj in _created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            _created.Clear();
        }

        private ClockablePlayerProvider _managedProvider = null!;
        private ClockablePlayerProvider _oneShotProvider = null!;
        private AudioConductorSettings _settings = null!;
        private readonly List<Object> _created = new();

        private Conductor CreateConductor()
        {
            return new Conductor(_settings, _managedProvider, _oneShotProvider);
        }

        private AudioClip CreateClip()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            _created.Add(clip);
            return clip;
        }

        private CueSheetAsset CreateSheetAsset(Cue cue)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            asset.cueSheet.cueList.Add(cue);
            _created.Add(asset);
            return asset;
        }

        private static Cue CreateCue(string name, ThrottleType throttleType = ThrottleType.FirstComeFirstServed,
            int throttleLimit = 0)
        {
            return new Cue { name = name, throttleType = throttleType, throttleLimit = throttleLimit };
        }

        private static Track CreateTrack(AudioClip clip)
        {
            return new Track { name = "track", audioClip = clip };
        }

        private void TriggerNaturalEnd(Conductor conductor, int playerIndex = 0)
        {
            _managedProvider.Created[playerIndex].Clock.DspTime = 100.0;
            conductor.Update(0.016f);
        }

        private void TriggerNaturalEndOneShot(Conductor conductor, int playerIndex = 0)
        {
            _oneShotProvider.Created[playerIndex].Clock.DspTime = 100.0;
            conductor.Update(0.016f);
        }

        // --- OnEnd via PlayOptions ---

        [Test]
        public void Play_WithOnEndInPlayOptions_InvokesCallback_WhenPlaybackEndsNaturally()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var endCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { OnEnd = () => endCalled = true });

            TriggerNaturalEnd(conductor);

            Assert.That(endCalled, Is.True);
        }

        [Test]
        public void Play_WithOnEndInPlayOptions_DoesNotInvoke_WhenStoppedExplicitly()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var endCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { OnEnd = () => endCalled = true });

            conductor.Stop(handle);

            Assert.That(endCalled, Is.False);
        }

        [Test]
        public void Play_WithOnEndInPlayOptions_DoesNotInvoke_WhenNaturalEndOccursDuringFadeOut()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var endCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { OnEnd = () => endCalled = true });

            conductor.Stop(handle, 0.5f);
            TriggerNaturalEnd(conductor); // clip ends before fade completes

            Assert.That(endCalled, Is.False);
        }

        // --- OnStop via PlayOptions ---

        [Test]
        public void Play_WithOnStopInPlayOptions_InvokesCallback_OnStop()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCalled = true });

            conductor.Stop(handle);

            Assert.That(stopCalled, Is.True);
        }

        [Test]
        public void Play_WithOnStopInPlayOptions_InvokesAfterFadeCompletes_NotAtStopCall()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCalled = true });

            conductor.Stop(handle, 0.1f);
            Assert.That(stopCalled, Is.False);

            conductor.Update(0.2f);
            Assert.That(stopCalled, Is.True);
        }

        // --- Both callbacks ---

        [Test]
        public void Play_WithBothCallbacks_InvokesBothOnNaturalEnd()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var endCount = 0;
            var stopCount = 0;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions
            {
                OnEnd = () => endCount++,
                OnStop = () => stopCount++
            });

            TriggerNaturalEnd(conductor);

            Assert.That(endCount, Is.EqualTo(1));
            Assert.That(stopCount, Is.EqualTo(1));
        }

        // --- Eviction ---

        [Test]
        public void Play_WithOnStopInPlayOptions_InvokedOnce_WhenEvictedByThrottle()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1", ThrottleType.PriorityOrder, 1);
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCount = 0;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);

            conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCount++ });
            conductor.Play(sheet, "cue1"); // evicts the first

            Assert.That(stopCount, Is.EqualTo(1));
        }

        // --- StopAll ---

        [Test]
        public void Play_WithOnStopInPlayOptions_InvokedOnce_WhenStopAllIsCalled()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCount = 0;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCount++ });

            conductor.StopAll();

            Assert.That(stopCount, Is.EqualTo(1));
        }

        // --- Dispose ---

        [Test]
        public void Play_WithOnStopInPlayOptions_InvokedOnce_WhenConductorDisposed()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCount = 0;
            var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCount++ });

            conductor.Dispose();

            Assert.That(stopCount, Is.EqualTo(1));
        }

        // --- No double-fire after natural end ---

        [Test]
        public void Play_WithOnStopInPlayOptions_InvokedOnce_AfterNaturalEndThenStop()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCount = 0;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCount++ });

            TriggerNaturalEnd(conductor); // fires OnStop once, removes from managed playbacks

            conductor.Stop(handle); // handle no longer tracked — no-op

            Assert.That(stopCount, Is.EqualTo(1));
        }

        // --- Exception isolation ---

        [Test]
        public void Play_OnStopThrowingCallback_DoesNotCrashConductor()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            var handle = conductor.Play(sheet, "cue1",
                new PlayOptions { OnStop = () => throw new InvalidOperationException("test") });

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: test"));
            Assert.DoesNotThrow(() => conductor.Stop(handle));
        }

        [Test]
        public void Play_OnEndThrowingCallback_DoesNotSkipFollowingStopCallback()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.Play(sheet, "cue1", new PlayOptions
            {
                OnEnd = () => throw new InvalidOperationException("end"),
                OnStop = () => stopCalled = true
            });

            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: end"));
            TriggerNaturalEnd(conductor);

            Assert.That(stopCalled, Is.True);
        }

        // --- PlayOneShot with PlayOneShotOptions ---

        [Test]
        public void PlayOneShot_WithOnEndInPlayOneShotOptions_InvokesAtNaturalEnd()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var endCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1", new PlayOneShotOptions { OnEnd = () => endCalled = true });

            TriggerNaturalEndOneShot(conductor);

            Assert.That(endCalled, Is.True);
        }

        [Test]
        public void PlayOneShot_WithOnStopInPlayOneShotOptions_InvokesOnStopAll()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            var stopCalled = false;
            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);
            conductor.PlayOneShot(sheet, "cue1", new PlayOneShotOptions { OnStop = () => stopCalled = true });

            conductor.StopAll();

            Assert.That(stopCalled, Is.True);
        }

        [Test]
        public void PlayOneShot_WithNullOptions_DoesNotThrow()
        {
            var clip = CreateClip();
            var cue = CreateCue("cue1");
            cue.trackList.Add(CreateTrack(clip));
            var asset = CreateSheetAsset(cue);

            using var conductor = CreateConductor();
            var sheet = conductor.RegisterCueSheet(asset);

            Assert.DoesNotThrow(() => conductor.PlayOneShot(sheet, "cue1", null));
        }

        // --- Helper: player provider with controllable DSP clocks ---

        private sealed class ClockablePlayerProvider : IPlayerProvider
        {
            private readonly Queue<(AudioClipPlayer Player, StubDspClock Clock)> _pool = new();
            public List<(AudioClipPlayer Player, StubDspClock Clock)> Created { get; } = new();

            public void Prewarm(int count)
            {
            }

            public AudioClipPlayer Rent()
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue().Player;

                var clock = new StubDspClock();
                var player = new AudioClipPlayer(
                    new IAudioSourceWrapper[] { new StubAudioSourceWrapper(), new StubAudioSourceWrapper() },
                    clock,
                    NullLifecycle.Instance
                );
                Created.Add((player, clock));
                return player;
            }

            public void Return(AudioClipPlayer player)
            {
                player.ResetState();
                for (var i = 0; i < Created.Count; i++)
                    if (Created[i].Player == player)
                    {
                        _pool.Enqueue(Created[i]);
                        return;
                    }
            }
        }
    }
}
