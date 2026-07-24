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
            // These tests cover the FakeMod dsp-clock scheduling path; force it on so EditMode does
            // not fall through to the stock AudioSource.SetScheduledEndTime path.
            AudioClipPlayer.UsesAudioConductorSchedulingOverride = true;
            _source0 = new SpyAudioSourceWrapper();
            _source1 = new SpyAudioSourceWrapper();
            _clock = new StubDspClock();
            _player = new AudioClipPlayer(new IAudioSourceWrapper[] { _source0, _source1 }, _clock,
                NullLifecycle.Instance);
            _clip = AudioClip.Create("test", 44100, 1, 44100, false);
            _longClip = AudioClip.Create("long", 441000, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            AudioClipPlayer.UsesAudioConductorSchedulingOverride = null;
            Object.DestroyImmediate(_clip);
            Object.DestroyImmediate(_longClip);
        }

        private SpyAudioSourceWrapper _source0 = null!;
        private SpyAudioSourceWrapper _source1 = null!;
        private StubDspClock _clock = null!;
        private AudioClipPlayer _player = null!;
        private AudioClip _clip = null!;
        private AudioClip _longClip = null!;

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
            _clock.DspTime = 0.5;
            _player.ManualUpdate(0f);
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;
            var scheduledEndTime = _source1.LastScheduledEndTime;

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
            _source0.IsPlaying = false; // simulate the source not yet becoming audible

            _player.PauseBySystem();

            Assert.That(_source0.PauseCount, Is.EqualTo(0));
            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void ResumeBySystem_NonLoop_WasStoppedBeforePlay_Reschedules()
        {
            _clock.DspTime = 0.0;
            SetupAndPlay();
            _source0.IsPlaying = false; // simulate the source not yet becoming audible
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
            _source0.IsPlaying = false; // simulate the source not yet becoming audible
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

        // --- Scheduled-stop re-arm after AudioContext resume ---
        // A SetScheduledEndTime issued while the browser AudioContext is suspended is
        // silently discarded by the engine's JS layer and never re-armed, so the player
        // must re-arm it once the context starts running.

        [Test]
        public void ManualUpdate_ArmedWhileContextSuspended_RearmsScheduledEndAfterResume()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();

            running = true;
            _clock.DspTime = 15.0;
            _source0.TimeSamples = 220500; // engine catch-up resumed mid-content (5s)
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(2));
            // 15.0 + (441000 - 220500) / 44100 = 20.0
            Assert.That(_source0.LastScheduledEndTime, Is.EqualTo(20.0).Within(1e-3));
            Assert.That(_source1.PlayScheduledCount, Is.Zero);
        }

        [Test]
        public void ManualUpdate_ContextRunningAtSchedule_DoesNotRearm()
        {
            _player.IsAudioContextRunningOverride = () => true;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();

            _clock.DspTime = 0.5;
            _source0.TimeSamples = 22050;
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(1));
        }

        [Test]
        public void ManualUpdate_ContextStillSuspended_DefersLoopSchedulingUntilResume()
        {
            _player.IsAudioContextRunningOverride = () => false;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();

            _clock.DspTime = 15.0; // past _nextEventTime while still suspended
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(1));
            Assert.That(_source1.PlayScheduledCount, Is.Zero);
        }

        [Test]
        public void ManualUpdate_CatchUpOvershotEndSample_Loop_ForcesImmediateLoopBoundary()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 44100, 220500);
            _player.Play();

            running = true;
            _clock.DspTime = 15.0;
            _source0.TimeSamples = 308700; // catch-up landed past the end sample (7s > 5s)
            _player.ManualUpdate(0f);

            Assert.That(_source0.LastScheduledEndTime, Is.EqualTo(15.0).Within(1e-3));
            Assert.That(_source1.PlayScheduledCount, Is.EqualTo(1));
            Assert.That(_source1.LastPlayScheduledTime, Is.EqualTo(15.0).Within(1e-3));
            Assert.That(_source1.TimeSamples, Is.EqualTo(44100));
            // 15.0 + (220500 - 44100) / 44100 = 19.0
            Assert.That(_source1.LastScheduledEndTime, Is.EqualTo(19.0).Within(1e-3));
        }

        [Test]
        public void ManualUpdate_CatchUpOvershotEndSample_NonLoop_FiresEndAction()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, false, 0, 0, 220500);
            _player.Play();
            var ended = false;
            _player.SetEndAction(() => ended = true);

            running = true;
            _clock.DspTime = 15.0;
            _source0.TimeSamples = 240000;
            _player.ManualUpdate(0f);

            Assert.That(ended, Is.True);
            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void ManualUpdate_ArmedWhileContextSuspended_NonLoop_BeforeSourceBecomesAudible_RearmsScheduledEnd()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, false, 0, 0, _longClip.samples);
            _player.Play();
            var originalEndTime = _source0.LastScheduledEndTime;

            running = true;
            _clock.DspTime = 0.05; // shortly after Play(); well before scheduledEndTime(10.0)
            _source0.IsPlaying = false; // source queued but not yet audible
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(2));
            Assert.That(_source0.LastScheduledEndTime, Is.EqualTo(originalEndTime).Within(1e-3));
        }

        [Test]
        public void ManualUpdate_ArmedWhileContextSuspended_Loop_LongSuspensionWithinClipDuration_RearmsScheduledEnd()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();
            var originalEndTime = _source0.LastScheduledEndTime;

            running = true;
            _clock.DspTime = 5.0; // beyond PlayScheduleDelay(0.1) but still well within scheduledEndTime(10.1)
            _source0.IsPlaying = false; // source queued but not yet audible
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(2));
            Assert.That(_source0.LastScheduledEndTime, Is.EqualTo(originalEndTime).Within(1e-3));
        }

        [Test]
        public void ManualUpdate_ArmedWhileContextSuspended_NonLoop_CatchUpOvershot_FiresEndActionWithoutStaleRearm()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, false, 0, 0, _longClip.samples);
            _player.Play();
            var ended = false;
            _player.SetEndAction(() => ended = true);

            running = true;
            _clock.DspTime = 15.0; // past scheduledEndTime(10.0); the queued sound already died
            _source0.IsPlaying = false;
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(1)); // nothing to re-arm
            Assert.That(ended, Is.True);
            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void ResumeBySystem_ContextStillSuspended_ArmsForRearmOnContextResume()
        {
            var running = true;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, false, 0, 0, _longClip.samples);
            _player.Play();
            _source0.IsPlaying = true;

            _clock.DspTime = 1.0;
            _player.PauseBySystem();

            running = false;
            _clock.DspTime = 3.0;
            _player.ResumeBySystem();
            var countAfterResume = _source0.SetScheduledEndTimeCount;

            running = true;
            _clock.DspTime = 3.5;
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.GreaterThan(countAfterResume));
        }

        [Test]
        public void Resume_ContextStillSuspended_ArmsForRearmOnContextResume()
        {
            var running = true;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, false, 0, 0, _longClip.samples);
            _player.Play();
            _source0.IsPlaying = true;

            _clock.DspTime = 1.0;
            _player.Pause();

            running = false;
            _clock.DspTime = 3.0;
            _player.Resume();
            var countAfterResume = _source0.SetScheduledEndTimeCount;

            running = true;
            _clock.DspTime = 3.5;
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.GreaterThan(countAfterResume));
        }

        // --- Backend-detect-pending deferral on SetPitch/SetCurrentSample ---
        // While the playback backend is still unknown (CompressedInMemory/Streaming loop), arming a
        // scheduled stop is irreversible on a buffer source (WebAudio stop() cannot be cancelled).
        // SetPitch/SetCurrentSample must defer arming until ResolvePendingBackend commits the strategy.

        [Test]
        public void SetPitch_WhileBackendDetectPending_DoesNotArmScheduledStop()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();
            _source0.IsPlaying = true; // audible, but backend not yet resolved
            var armCountBefore = _source0.SetScheduledEndTimeCount;

            _player.SetPitch(1.5f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(armCountBefore));
        }

        [Test]
        public void SetCurrentSample_WhileBackendDetectPending_DoesNotArmScheduledStop()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            var armCountBefore = _source0.SetScheduledEndTimeCount;

            _player.SetCurrentSample(44100);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(armCountBefore));
        }

        [Test]
        public void SetPitch_AfterBackendResolved_ArmsScheduledStop()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            _player.ManualUpdate(0f); // ResolvePendingBackend clears _backendDetectPending
            var armCountBefore = _source0.SetScheduledEndTimeCount;

            _player.SetPitch(1.5f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.GreaterThan(armCountBefore));
        }

        // While the backend is still unknown, PauseBySystem must not Pause() (it would risk the
        // MediaElement catch-up glitch if the source resolves to MediaElement) and must restart from
        // the start sample on resume so a cue with an intro region (loopStart > start) is not truncated.

        [Test]
        public void PauseBySystem_LoopBackendPending_StopsBothSourcesWithoutPausing()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 44100, 220500);
            _player.Play();
            _source0.IsPlaying = true; // audible, but backend not yet resolved

            _player.PauseBySystem();

            Assert.That(_source0.PauseCount, Is.EqualTo(0));
            Assert.That(_source1.PauseCount, Is.EqualTo(0));
            Assert.That(_source0.StopCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(_source1.StopCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void ResumeBySystem_LoopBackendPending_ReschedulesFromStartSample()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 44100, 220500);
            _player.Play();
            _source0.IsPlaying = true;
            _player.PauseBySystem();
            _source0.TimeSamples = 99999; // ensure the resume reschedule overwrites the position

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
            Assert.That(_source0.TimeSamples, Is.EqualTo(0)); // start sample, not loopStart (44100)
        }

        // A MediaElement crossover loop interrupted by a system pause is rescheduled fresh on resume.
        // A pause that lands inside the intro region (start..loopStart) must resume from the captured
        // live position so the remaining intro is not truncated; a pause inside the loop body resumes
        // from loopStart, the loop's own restart point.

        [Test]
        public void ResumeBySystem_LoopMediaElement_PausedDuringIntro_ResumesFromCapturedPosition()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 44100, 220500);
            _player.Play();
            _source0.IsPlaying = true;
            _player.ManualUpdate(0f); // resolve backend -> MediaElement crossover (not native, not pending)
            _source0.TimeSamples = 22050; // mid-intro (< loopStart 44100)
            _player.PauseBySystem();
            _source0.TimeSamples = 99999; // residue after stop; resume must overwrite from captured value

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
            Assert.That(_source0.TimeSamples, Is.EqualTo(22050));
        }

        [Test]
        public void ResumeBySystem_LoopMediaElement_PausedInLoopBody_ResumesFromLoopStart()
        {
            _player.LoadTypeOverride = AudioClipLoadType.CompressedInMemory;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 44100, 220500);
            _player.Play();
            _source0.IsPlaying = true;
            _player.ManualUpdate(0f); // resolve backend -> MediaElement crossover
            _source0.TimeSamples = 100000; // inside loop body (>= loopStart 44100)
            _player.PauseBySystem();
            _source0.TimeSamples = 99999;

            _player.ResumeBySystem();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
            Assert.That(_source0.TimeSamples, Is.EqualTo(44100));
        }

        [Test]
        public void ManualUpdate_CatchUpOvershotClipEnd_ResumesLoopScheduling()
        {
            var running = false;
            _player.IsAudioContextRunningOverride = () => running;
            _clock.DspTime = 0.0;
            _player.Setup(null, _longClip, 0, 1f, 1f, true, 0, 0, _longClip.samples);
            _player.Play();

            running = true;
            _clock.DspTime = 15.0;
            _source0.IsPlaying = false; // the queued sound died instantly (offset beyond clip end)
            _player.ManualUpdate(0f);

            Assert.That(_source0.SetScheduledEndTimeCount, Is.EqualTo(1)); // nothing to re-arm
            Assert.That(_source1.PlayScheduledCount, Is.EqualTo(1)); // loop scheduling resumed
        }
    }
}

#endif
