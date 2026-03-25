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
    internal sealed class AudioClipPlayerCoreTests
    {
        [SetUp]
        public void SetUp()
        {
            _source0 = new SpyAudioSourceWrapper();
            _source1 = new SpyAudioSourceWrapper();
            _clock = new StubDspClock();
            _core = new AudioClipPlayerCore(new IAudioSourceWrapper[] { _source0, _source1 }, _clock);
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
        private AudioClipPlayerCore _core = null!;
        private AudioClip _clip = null!;

        private void SetupCore(float volume = 1f, float pitch = 1f, bool isLoop = false, int endSample = 0)
        {
            _core.Setup(null, _clip, 0, volume, pitch, isLoop, 0, 0, endSample);
        }

        // --- Volume (5-factor multiplication) ---

        [Test]
        public void GetActualVolume_AllFactorsOne_ReturnsOne()
        {
            SetupCore();

            Assert.That(_core.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void GetActualVolume_MultiplyAllFiveFactors()
        {
            SetupCore(0.5f);
            _core.SetVolume(0.5f);
            _core.SetVolumeFade(0.5f);
            _core.SetMasterVolume(0.5f);
            _core.SetCategoryVolume(0.5f);

            // asset(0.5) * runtime(0.5) * fade(0.5) * master(0.5) * category(0.5) = 0.03125
            Assert.That(_core.GetActualVolume(), Is.EqualTo(0.03125f).Within(0.0001f));
        }

        [Test]
        public void GetActualVolume_ClampedToZero_WhenNegative()
        {
            SetupCore(-1f);

            Assert.That(_core.GetActualVolume(), Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void GetActualVolume_ClampedToOne_WhenOverOne()
        {
            SetupCore(2f);

            Assert.That(_core.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void SetCategoryVolume_AffectsGetActualVolume()
        {
            SetupCore();
            _core.SetCategoryVolume(0.5f);

            Assert.That(_core.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetMasterVolume_AffectsGetActualVolume()
        {
            SetupCore();
            _core.SetMasterVolume(0.5f);

            Assert.That(_core.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetVolumeFade_AffectsGetActualVolume()
        {
            SetupCore();
            _core.SetVolumeFade(0.5f);

            Assert.That(_core.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        // --- Pitch ---

        [Test]
        public void GetActualPitch_MultiplyInternalAndExternal()
        {
            SetupCore(pitch: 0.5f);
            _core.SetPitch(2f);

            Assert.That(_core.GetActualPitch(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void GetActualPitch_ClampedToMax()
        {
            SetupCore(pitch: 4f);
            _core.SetPitch(4f);

            Assert.That(_core.GetActualPitch(), Is.LessThanOrEqualTo(3f));
        }

        [Test]
        public void GetActualPitch_ClampedToNegativeMax()
        {
            SetupCore(pitch: -4f);
            _core.SetPitch(4f);

            Assert.That(_core.GetActualPitch(), Is.GreaterThanOrEqualTo(-3f));
        }

        // --- State transitions ---

        [Test]
        public void Setup_InitialState_IsNotPlaying()
        {
            SetupCore();

            Assert.That(_core.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Pause_SetsStatePaused()
        {
            SetupCore();

            _core.Pause();

            Assert.That(_core.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void Pause_WhenAlreadyPaused_DoesNotPauseAgain()
        {
            SetupCore();
            _core.Pause();

            _core.Pause();

            Assert.That(_core.State, Is.EqualTo(PlayerState.Paused));
        }

        [Test]
        public void Resume_AfterPauseWithoutPlaying_RemainsStateStopped()
        {
            SetupCore();
            _core.Pause();

            _core.Resume();

            Assert.That(_core.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNotThrow()
        {
            SetupCore();

            Assert.DoesNotThrow(() => _core.Resume());
            Assert.That(_core.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Stop_SetsStateStopped()
        {
            SetupCore();
            _core.Pause();

            _core.Stop();

            Assert.That(_core.State, Is.EqualTo(PlayerState.Stopped));
        }

        // --- Callbacks ---

        [Test]
        public void Stop_InvokesStopAction()
        {
            SetupCore();
            var called = false;
            _core.AddStopAction(() => called = true);

            _core.Stop();

            Assert.That(called, Is.True);
        }

        [Test]
        public void Stop_InvokesStopActionOnlyOnce()
        {
            SetupCore();
            var callCount = 0;
            _core.AddStopAction(() => callCount++);

            _core.Stop();
            _core.Stop();

            Assert.That(callCount, Is.EqualTo(1));
        }

        [Test]
        public void Stop_WithoutStopAction_DoesNotThrow()
        {
            SetupCore();

            Assert.DoesNotThrow(() => _core.Stop());
        }

        // --- ResetState ---

        [Test]
        public void ResetState_ResetsCategoryVolumeToOne()
        {
            SetupCore();
            _core.SetCategoryVolume(0.3f);

            _core.ResetState();

            Assert.That(_core.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsMasterVolumeToOne()
        {
            SetupCore();
            _core.SetMasterVolume(0.5f);

            _core.ResetState();

            Assert.That(_core.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsPitchExternalToOne()
        {
            SetupCore();
            _core.SetPitch(2f);

            _core.ResetState();

            Assert.That(_core.GetPitch(), Is.EqualTo(1f));
        }

        // --- Setup with null clip ---

        [Test]
        public void Setup_WithNullClip_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _core.Setup(null, null!, 0, 1f, 1f, false, 0, 0, 0));
        }

        // --- GetVolume / SetVolume ---

        [Test]
        public void GetVolume_AfterSetup_ReturnsOne()
        {
            SetupCore();

            Assert.That(_core.GetVolume(), Is.EqualTo(1f));
        }

        [Test]
        public void SetVolume_ChangesGetVolume()
        {
            SetupCore();

            _core.SetVolume(0.7f);

            Assert.That(_core.GetVolume(), Is.EqualTo(0.7f));
        }

        // --- GetPitch / SetPitch ---

        [Test]
        public void GetPitch_AfterSetup_ReturnsOne()
        {
            SetupCore();

            Assert.That(_core.GetPitch(), Is.EqualTo(1f));
        }

        [Test]
        public void SetPitch_ChangesGetPitch()
        {
            SetupCore();

            _core.SetPitch(1.5f);

            Assert.That(_core.GetPitch(), Is.EqualTo(1.5f));
        }

        // --- Loop playback ---

        [Test]
        public void Play_Loop_WithValidEndSample_SchedulesSource0()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);

            _core.Play();

            Assert.That(_source0.IsPlaying, Is.True);
        }

        [Test]
        public void Play_Loop_WithDuration0_DoesNotScheduleAudioSource()
        {
            // endSample == startSample == 0 → samples=0 → duration=0
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: 0);

            _core.Play();

            Assert.That(_source0.IsPlaying, Is.False);
        }

        [Test]
        public void ManualUpdate_Loop_WithDuration0_DoesNotRepeatSchedule()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: 0);
            _core.Play();

            for (var i = 0; i < 5; i++)
            {
                _clock.DspTime += 0.1;
                _core.ManualUpdate(0.1f);
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
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();

            // Before _nextEventTime (0.1s): source1 not yet scheduled
            _clock.DspTime = 0.05;
            _core.ManualUpdate(0.05f);
            Assert.That(_source1.IsPlaying, Is.False);

            // After _nextEventTime (0.1s): source1 scheduled for next loop
            _clock.DspTime = 0.15;
            _core.ManualUpdate(0.1f);
            Assert.That(_source1.IsPlaying, Is.True);
        }

        // --- Restart ---

        [Test]
        public void Restart_WhilePlaying_RestartFromBeginning()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: false, endSample: _clip.samples);
            _core.Play();

            _clock.DspTime = 0.5;
            _core.Restart();

            // Restart calls Play() which schedules via the next alternating AudioSource index
            Assert.That(_core.State, Is.EqualTo(PlayerState.Playing));
        }

        // --- Pause (loop 2-source branch) ---

        [Test]
        public void Pause_Loop_Source0Playing_PausesSource0StopsSource1()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;

            _core.Pause();

            Assert.That(_source0.PauseCount, Is.EqualTo(1));
            Assert.That(_source1.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Pause_Loop_Source1Playing_StopsSource0PausesSource1()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;

            _core.Pause();

            Assert.That(_source1.PauseCount, Is.EqualTo(1));
            Assert.That(_source0.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Pause_NonLoop_PausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: false, endSample: _clip.samples);
            _core.Play();

            _core.Pause();

            Assert.That(_source0.PauseCount, Is.EqualTo(1));
        }

        // --- Resume (loop branch) ---

        [Test]
        public void Resume_Loop_PausedSource0_UnPausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;
            _core.Pause();

            _core.Resume();

            Assert.That(_source0.UnPauseCount, Is.EqualTo(1));
            Assert.That(_source1.UnPauseCount, Is.EqualTo(0));
        }

        [Test]
        public void Resume_Loop_PausedSource1_UnPausesSource1()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = false;
            _source1.IsPlaying = true;
            _core.Pause();

            _core.Resume();

            Assert.That(_source1.UnPauseCount, Is.EqualTo(1));
            Assert.That(_source0.UnPauseCount, Is.EqualTo(0));
        }

        [Test]
        public void Resume_NonLoop_UnPausesSource0()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: false, endSample: _clip.samples);
            _core.Play();
            _core.Pause();

            _core.Resume();

            Assert.That(_source0.UnPauseCount, Is.EqualTo(1));
        }

        // --- AddEndAction / _onEnd fire ---

        [Test]
        public void AddEndAction_OnPlaybackEnd_InvokesCallback()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: false, endSample: _clip.samples);
            _core.Play();

            var called = false;
            _core.AddEndAction(() => called = true);

            // Advance past scheduledEndTime to trigger ManualUpdate → _onEnd
            _clock.DspTime = 10.0;
            _core.ManualUpdate(10.0f);

            Assert.That(called, Is.True);
        }

        [Test]
        public void AddEndAction_Multiple_InvokesAllCallbacks()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: false, endSample: _clip.samples);
            _core.Play();

            var count = 0;
            _core.AddEndAction(() => count++);
            _core.AddEndAction(() => count++);

            _clock.DspTime = 10.0;
            _core.ManualUpdate(10.0f);

            Assert.That(count, Is.EqualTo(2));
        }

        // --- GetCurrentSample / SetCurrentSample ---

        [Test]
        public void GetCurrentSample_NonLoop_ReturnsSource0TimeSamples()
        {
            SetupCore(isLoop: false);
            _source0.TimeSamples = 1234;

            Assert.That(_core.GetCurrentSample(), Is.EqualTo(1234));
        }

        [Test]
        public void GetCurrentSample_Loop_Source0Playing_ReturnsSource0TimeSamples()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;
            _source0.TimeSamples = 500;

            Assert.That(_core.GetCurrentSample(), Is.EqualTo(500));
        }

        [Test]
        public void GetCurrentSample_Loop_BothPlaying_ReturnsHigherTimeSamples()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = true;
            _source0.TimeSamples = 1000;
            _source1.TimeSamples = 500;

            // GetPlayingSource returns source with higher TimeSamples
            Assert.That(_core.GetCurrentSample(), Is.EqualTo(1000));
        }

        [Test]
        public void GetCurrentSample_Loop_NonePlaying_ReturnsZero()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _source0.IsPlaying = false;
            _source1.IsPlaying = false;

            Assert.That(_core.GetCurrentSample(), Is.EqualTo(0));
        }

        [Test]
        public void SetCurrentSample_NonLoop_SetsSource0TimeSamples()
        {
            SetupCore(isLoop: false, endSample: _clip.samples);

            _core.SetCurrentSample(2000);

            Assert.That(_source0.TimeSamples, Is.EqualTo(2000));
        }

        [Test]
        public void SetCurrentSample_Loop_SetsPlayingSourceTimeSamples()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: true, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = true;
            _source1.IsPlaying = false;

            _core.SetCurrentSample(3000);

            Assert.That(_source0.TimeSamples, Is.EqualTo(3000));
        }

        [Test]
        public void SetCurrentSample_ReschedulesEndTime()
        {
            _clock.DspTime = 0.0;
            SetupCore(isLoop: false, endSample: _clip.samples);
            _core.Play();
            _source0.IsPlaying = true;
            var endTimeBefore = _source0.LastScheduledEndTime;

            _clock.DspTime = 0.5;
            _source0.TimeSamples = 22050; // halfway
            _core.SetCurrentSample(22050);

            Assert.That(_source0.LastScheduledEndTime, Is.Not.EqualTo(endTimeBefore));
        }
    }
}
