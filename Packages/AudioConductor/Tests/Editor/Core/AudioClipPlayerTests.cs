// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class AudioClipPlayerTests
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

        private void SetupPlayer(float volume = 1f, float pitch = 1f, bool isLoop = false, int endSample = 0)
        {
            _player.Setup(null, _clip, 0, volume, pitch, isLoop, 0, 0, endSample);
        }

        // --- Volume (5-factor multiplication) ---

        [Test]
        public void GetActualVolume_AllFactorsOne_ReturnsOne()
        {
            SetupPlayer();

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void GetActualVolume_MultiplyAllFiveFactors()
        {
            SetupPlayer(0.5f);
            _player.SetVolume(0.5f);
            _player.SetVolumeFade(0.5f);
            _player.SetMasterVolume(0.5f);
            _player.SetCategoryVolume(0.5f);

            // asset(0.5) * runtime(0.5) * fade(0.5) * master(0.5) * category(0.5) = 0.03125
            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.03125f).Within(0.0001f));
        }

        [Test]
        public void GetActualVolume_ClampedToZero_WhenNegative()
        {
            SetupPlayer(-1f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void GetActualVolume_ClampedToOne_WhenOverOne()
        {
            SetupPlayer(2f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void SetCategoryVolume_AffectsGetActualVolume()
        {
            SetupPlayer();
            _player.SetCategoryVolume(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetMasterVolume_AffectsGetActualVolume()
        {
            SetupPlayer();
            _player.SetMasterVolume(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetVolumeFade_AffectsGetActualVolume()
        {
            SetupPlayer();
            _player.SetVolumeFade(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        // --- Pitch ---

        [Test]
        public void GetActualPitch_MultiplyInternalAndExternal()
        {
            SetupPlayer(pitch: 0.5f);
            _player.SetPitch(2f);

            Assert.That(_player.GetActualPitch(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void GetActualPitch_ClampedToMax()
        {
            SetupPlayer(pitch: 4f);
            _player.SetPitch(4f);

            Assert.That(_player.GetActualPitch(), Is.LessThanOrEqualTo(3f));
        }

        [Test]
        public void GetActualPitch_ClampedToNegativeMax()
        {
            SetupPlayer(pitch: -4f);
            _player.SetPitch(4f);

            Assert.That(_player.GetActualPitch(), Is.GreaterThanOrEqualTo(-3f));
        }

        // --- State transitions ---

        [Test]
        public void Setup_InitialState_IsNotPlaying()
        {
            SetupPlayer();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Pause_SetsStatePaused()
        {
            SetupPlayer();

            _player.Pause();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void Pause_WhenAlreadyPaused_DoesNotPauseAgain()
        {
            SetupPlayer();
            _player.Pause();

            _player.Pause();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void Resume_AfterPauseWithoutPlaying_RemainsStateStopped()
        {
            SetupPlayer();
            _player.Pause();

            _player.Resume();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNotThrow()
        {
            SetupPlayer();

            Assert.DoesNotThrow(() => _player.Resume());
            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Stop_SetsStateStopped()
        {
            SetupPlayer();
            _player.Pause();

            _player.Stop();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        // --- Callbacks ---

        [Test]
        public void Stop_InvokesStopAction()
        {
            SetupPlayer();
            var called = false;
            _player.AddStopAction(() => called = true);

            _player.Stop();

            Assert.That(called, Is.True);
        }

        [Test]
        public void Stop_InvokesStopActionOnlyOnce()
        {
            SetupPlayer();
            var callCount = 0;
            _player.AddStopAction(() => callCount++);

            _player.Stop();
            _player.Stop();

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Stop_WithoutStopAction_DoesNotThrow()
        {
            SetupPlayer();

            Assert.DoesNotThrow(() => _player.Stop());
        }

        // --- ResetState ---

        [Test]
        public void ResetState_ResetsCategoryVolumeToOne()
        {
            SetupPlayer();
            _player.SetCategoryVolume(0.3f);

            _player.ResetState();

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsMasterVolumeToOne()
        {
            SetupPlayer();
            _player.SetMasterVolume(0.5f);

            _player.ResetState();

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsPitchExternalToOne()
        {
            SetupPlayer();
            _player.SetPitch(2f);

            _player.ResetState();

            Assert.That(_player.GetPitch(), Is.EqualTo(1f));
        }

        // --- Setup with null clip ---

        [Test]
        public void Setup_WithNullClip_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _player.Setup(null, null!, 0, 1f, 1f, false, 0, 0, 0));
        }

        // --- GetVolume / SetVolume ---

        [Test]
        public void GetVolume_AfterSetup_ReturnsOne()
        {
            SetupPlayer();

            Assert.That(_player.GetVolume(), Is.EqualTo(1f));
        }

        [Test]
        public void SetVolume_ChangesGetVolume()
        {
            SetupPlayer();

            _player.SetVolume(0.7f);

            Assert.That(_player.GetVolume(), Is.EqualTo(0.7f));
        }

        // --- GetPitch / SetPitch ---

        [Test]
        public void GetPitch_AfterSetup_ReturnsOne()
        {
            SetupPlayer();

            Assert.That(_player.GetPitch(), Is.EqualTo(1f));
        }

        [Test]
        public void SetPitch_ChangesGetPitch()
        {
            SetupPlayer();

            _player.SetPitch(1.5f);

            Assert.That(_player.GetPitch(), Is.EqualTo(1.5f));
        }

        // --- Loop playback ---

        [Test]
        public void Play_Loop_WithValidEndSample_SchedulesSource0()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);

            _player.Play();

            Assert.That(_source0.IsPlaying, Is.True);
        }

        [Test]
        public void Play_Loop_WithDuration0_DoesNotScheduleAudioSource()
        {
            // endSample == startSample == 0 → samples=0 → duration=0
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: 0);

            _player.Play();

            Assert.That(_source0.IsPlaying, Is.False);
        }

        [Test]
        public void ManualUpdate_Loop_WithDuration0_DoesNotRepeatSchedule()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: 0);
            _player.Play();

            for (var i = 0; i < 5; i++)
            {
                _clock.DspTime += 0.1;
                _player.ManualUpdate(0.1f);
            }

            Assert.That(_source0.IsPlaying, Is.False);
            Assert.That(_source1.IsPlaying, Is.False);
        }

        [Test]
        public void ManualUpdate_Loop_WithValidEndSample_TriggersLoopOnTime()
        {
            // clip: 44100 samples @ 44100 Hz → duration=1.0s
            // After Play: _nextEventTime = dspTime + 0.1 (playStartTime) + 1.0 - 1.0 = dspTime + 0.1
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();

            // Before _nextEventTime (0.1s): source1 not yet scheduled
            _clock.DspTime = 0.05;
            _player.ManualUpdate(0.05f);
            Assert.That(_source1.IsPlaying, Is.False);

            // After _nextEventTime (0.1s): source1 scheduled for next loop
            _clock.DspTime = 0.15;
            _player.ManualUpdate(0.1f);
            Assert.That(_source1.IsPlaying, Is.True);
        }

        // --- Restart ---

        [Test]
        public void Restart_WhilePlaying_RestartFromBeginning()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: false, endSample: _clip.samples);
            _player.Play();

            _clock.DspTime = 0.5;
            _player.Restart();

            // Restart calls Play() which schedules via the next alternating AudioSource index
            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
        }

        // --- Pause (loop 2-source branch) ---

        [Test]
        public void Pause_Loop_Source0Playing_PausesSource0StopsSource1()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;

            _player.Pause();

            Assert.That(_source0.PauseCount, Is.EqualTo(1));
            Assert.That(_source1.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Pause_Loop_Source1Playing_StopsSource0PausesSource1()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;

            _player.Pause();

            Assert.That(_source1.PauseCount, Is.EqualTo(1));
            Assert.That(_source0.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Pause_NonLoop_PausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: false, endSample: _clip.samples);
            _player.Play();

            _player.Pause();

            Assert.That(_source0.PauseCount, Is.EqualTo(1));
        }

        // --- Resume (loop branch) ---

        [Test]
        public void Resume_Loop_PausedSource0_UnPausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;
            _player.Pause();

            _player.Resume();

            Assert.That(_source0.UnPauseCount, Is.EqualTo(1));
            Assert.That(_source1.UnPauseCount, Is.EqualTo(0));
        }

        [Test]
        public void Resume_Loop_PausedSource1_UnPausesSource1()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;
            _player.Pause();

            _player.Resume();

            Assert.That(_source1.UnPauseCount, Is.EqualTo(1));
            Assert.That(_source0.UnPauseCount, Is.EqualTo(0));
        }

        [Test]
        public void Resume_NonLoop_UnPausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: false, endSample: _clip.samples);
            _player.Play();
            _player.Pause();

            _player.Resume();

            Assert.That(_source0.UnPauseCount, Is.EqualTo(1));
        }

        // --- AddEndAction / _onEnd fire ---

        [Test]
        public void AddEndAction_OnPlaybackEnd_InvokesCallback()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: false, endSample: _clip.samples);
            _player.Play();

            var called = false;
            _player.AddEndAction(() => called = true);

            // Advance past scheduledEndTime to trigger ManualUpdate → _onEnd
            _clock.DspTime = 10.0;
            _player.ManualUpdate(10.0f);

            Assert.That(called, Is.True);
        }

        [Test]
        public void AddEndAction_Multiple_InvokesAllCallbacks()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: false, endSample: _clip.samples);
            _player.Play();

            var count = 0;
            _player.AddEndAction(() => count++);
            _player.AddEndAction(() => count++);

            _clock.DspTime = 10.0;
            _player.ManualUpdate(10.0f);

            Assert.That(count, Is.EqualTo(2));
        }

        // --- GetCurrentSample / SetCurrentSample ---

        [Test]
        public void GetCurrentSample_NonLoop_ReturnsSource0TimeSamples()
        {
            SetupPlayer(isLoop: false);
            _source0.TimeSamples = 1234;

            Assert.That(_player.GetCurrentSample(), Is.EqualTo(1234));
        }

        [Test]
        public void GetCurrentSample_Loop_Source0Playing_ReturnsSource0TimeSamples()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;
            _source0.TimeSamples = 500;

            Assert.That(_player.GetCurrentSample(), Is.EqualTo(500));
        }

        [Test]
        public void GetCurrentSample_Loop_BothPlaying_ReturnsHigherTimeSamples()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = true;
            _source0.TimeSamples = 1000;
            _source1.TimeSamples = 500;

            // GetPlayingSource returns source with higher TimeSamples
            Assert.That(_player.GetCurrentSample(), Is.EqualTo(1000));
        }

        [Test]
        public void GetCurrentSample_Loop_NonePlaying_ReturnsZero()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _source0.IsPlaying = false;
            _source1.IsPlaying = false;

            Assert.That(_player.GetCurrentSample(), Is.EqualTo(0));
        }

        [Test]
        public void SetCurrentSample_NonLoop_SetsSource0TimeSamples()
        {
            SetupPlayer(isLoop: false, endSample: _clip.samples);

            _player.SetCurrentSample(2000);

            Assert.That(_source0.TimeSamples, Is.EqualTo(2000));
        }

        [Test]
        public void SetCurrentSample_Loop_SetsPlayingSourceTimeSamples()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: true, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;

            _player.SetCurrentSample(3000);

            Assert.That(_source0.TimeSamples, Is.EqualTo(3000));
        }

        [Test]
        public void SetCurrentSample_ReschedulesEndTime()
        {
            _clock.DspTime = 0.0;
            SetupPlayer(isLoop: false, endSample: _clip.samples);
            _player.Play();
            _source0.IsPlaying = true;
            var endTimeBefore = _source0.LastScheduledEndTime;

            _clock.DspTime = 0.5;
            _source0.TimeSamples = 22050; // halfway
            _player.SetCurrentSample(22050);

            Assert.That(_source0.LastScheduledEndTime, Is.Not.EqualTo(endTimeBefore));
        }
    }
}
