// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_WEBGL
using AudioConductor.Core;
using AudioConductor.Core.Enums;
using AudioConductor.Editor.Core.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tests
{
    [TestFixture]
    internal sealed class AudioClipPlayerWebGLTests
    {
        [SetUp]
        public void SetUp()
        {
            _source0 = new SpyAudioSourceWrapper();
            _source1 = new SpyAudioSourceWrapper();
            _clock = new StubDspClock();
            _player = new AudioClipPlayer(new IAudioSourceWrapper[] { _source0, _source1 }, _clock,
                NullLifecycle.Instance);
            _clip = AudioClip.Create("test", 44100, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_clip);
        }

        private SpyAudioSourceWrapper _source0 = null!;
        private SpyAudioSourceWrapper _source1 = null!;
        private StubDspClock _clock = null!;
        private AudioClipPlayer _player = null!;
        private AudioClip _clip = null!;

        private void SetupAndPlay(bool isLoop = false)
        {
            _player.Setup(null, _clip, 0, 1f, 1f, isLoop, 0, 0, _clip.samples);
            _player.Play();
        }

        // --- PauseBySystem ---

        [Test]
        public void PauseBySystem_WhenPlaying_SetsStatePaused()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();

            _player.PauseBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void PauseBySystem_WhenAlreadySystemPaused_NoOp()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _player.PauseBySystem();

            _player.PauseBySystem();

            Assert.That(_source0.PauseCount, Is.EqualTo(1));
        }

        [Test]
        public void PauseBySystem_WhenUserPaused_DoesNotSystemPause()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _player.Pause();

            _player.PauseBySystem();

            // Only the user Pause() call should have called Pause on the source
            Assert.That(_source0.PauseCount, Is.EqualTo(1));
        }

        [Test]
        public void PauseBySystem_WhenNotPlaying_NoOp()
        {
            _player.Setup(null, _clip, 0, 1f, 1f, false, 0, 0, _clip.samples);

            _player.PauseBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void PauseBySystem_Loop_Source0Playing_PausesSource0StopsSource1()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;

            _player.PauseBySystem();

            Assert.That(_source0.PauseCount, Is.EqualTo(1));
            Assert.That(_source1.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void PauseBySystem_Loop_NeitherSourcePlaying_SetsStatePausedWithoutPausingAnySources()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = false; // simulate PlayScheduleDelay window
            _source1.IsPlaying = false;

            _player.PauseBySystem();

            Assert.That(_source0.PauseCount, Is.EqualTo(0));
            Assert.That(_source1.PauseCount, Is.EqualTo(0));
            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        // --- ResumeBySystem ---

        [Test]
        public void ResumeBySystem_WhenNotSystemPaused_DoesNotThrow()
        {
            _player.Setup(null, _clip, 0, 1f, 1f, false, 0, 0, _clip.samples);

            Assert.DoesNotThrow(() => _player.ResumeBySystem());
        }

        [Test]
        public void ResumeBySystem_WhenUserPaused_DoesNotUnpauseUser()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _player.Pause();
            _player.PauseBySystem();

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void ResumeBySystem_ShiftsScheduledEndTime()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _source0.IsPlaying = true;
            var endTimeBefore = _source0.LastScheduledEndTime;

            _clock.DspTime = 1.0;
            _player.PauseBySystem();
            _clock.DspTime = 3.0;
            _player.ResumeBySystem();

            Assert.That(_source0.LastScheduledEndTime, Is.EqualTo(endTimeBefore + 2.0).Within(0.0001));
        }

        [Test]
        public void ResumeBySystem_Loop_Source0Paused_UnpausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;
            _player.PauseBySystem();

            _player.ResumeBySystem();

            Assert.That(_source0.UnPauseCount, Is.EqualTo(1));
            Assert.That(_source1.UnPauseCount, Is.EqualTo(0));
        }

        [Test]
        public void ResumeBySystem_Loop_Source1Paused_UnpausesSource1()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;
            _player.PauseBySystem();

            _player.ResumeBySystem();

            Assert.That(_source1.UnPauseCount, Is.EqualTo(1));
            Assert.That(_source0.UnPauseCount, Is.EqualTo(0));
        }

        [Test]
        public void ResumeBySystem_Loop_Source1Paused_ShiftsScheduledEndTimeOnSource1()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;
            var scheduledEndTime = _source0.LastScheduledEndTime;

            _clock.DspTime = 1.0;
            _player.PauseBySystem();
            _clock.DspTime = 3.0;
            _player.ResumeBySystem();

            Assert.That(_source1.LastScheduledEndTime, Is.EqualTo(scheduledEndTime + 2.0).Within(0.0001));
        }

        [Test]
        public void ResumeBySystem_Loop_WasStoppedBeforePlay_Reschedules()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = false; // simulate PlayScheduleDelay window
            _source1.IsPlaying = false;
            _player.PauseBySystem();

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
            Assert.That(_source0.IsPlaying, Is.True);
            Assert.That(_source1.IsPlaying, Is.False);
        }

        [Test]
        public void ResumeBySystem_Loop_WasStoppedBeforePlay_WhenUserAlsoPaused_DoesNotReschedule()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay(true);
            _source0.IsPlaying = false; // simulate PlayScheduleDelay window
            _source1.IsPlaying = false;
            _player.PauseBySystem();
            _player.Pause();

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void PauseBySystem_NonLoop_NeitherSourcePlaying_SetsStatePausedWithoutPausingAnySources()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _source0.IsPlaying = false; // simulate PlayScheduleDelay window

            _player.PauseBySystem();

            Assert.That(_source0.PauseCount, Is.EqualTo(0));
            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void ResumeBySystem_NonLoop_WasStoppedBeforePlay_Reschedules()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _source0.IsPlaying = false; // simulate PlayScheduleDelay window
            _player.PauseBySystem();

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
            Assert.That(_source0.IsPlaying, Is.True);
        }

        [Test]
        public void ResumeBySystem_NonLoop_WasStoppedBeforePlay_WhenUserAlsoPaused_DoesNotReschedule()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _source0.IsPlaying = false; // simulate PlayScheduleDelay window
            _player.PauseBySystem();
            _player.Pause();

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        // --- ManualUpdate interaction ---

        [Test]
        public void ManualUpdate_WhenSystemPaused_DoesNotTriggerStop()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            var stopped = false;
            _player.SetStopAction(() => stopped = true);

            _player.PauseBySystem();
            _clock.DspTime = 100.0;
            _player.ManualUpdate(0f);

            Assert.That(stopped, Is.False);
            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        // --- ResetState ---

        [Test]
        public void ResetState_ClearsSystemPaused()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _player.PauseBySystem();

            _player.ResetState();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }
    }
}

#endif
