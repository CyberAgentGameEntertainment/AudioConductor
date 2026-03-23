// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core;
using AudioConductor.Editor.Tests.Core.Fakes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Tests.Core
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

        private void SetupCore(float volume = 1f, float pitch = 1f, bool isLoop = false)
        {
            _core.Setup(null, _clip, 0, volume, pitch, isLoop, 0, 0, 0);
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

            Assert.That(_core.IsPlaying, Is.False);
        }

        [Test]
        public void Pause_SetsIsPausedTrue()
        {
            SetupCore();

            _core.Pause();

            Assert.That(_core.IsPaused, Is.True);
        }

        [Test]
        public void Pause_WhenAlreadyPaused_DoesNotPauseAgain()
        {
            SetupCore();
            _core.Pause();

            _core.Pause();

            Assert.That(_core.IsPaused, Is.True);
        }

        [Test]
        public void Resume_AfterPause_SetsIsPausedFalse()
        {
            SetupCore();
            _core.Pause();

            _core.Resume();

            Assert.That(_core.IsPaused, Is.False);
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNotThrow()
        {
            SetupCore();

            Assert.DoesNotThrow(() => _core.Resume());
            Assert.That(_core.IsPaused, Is.False);
        }

        [Test]
        public void Stop_SetsIsPausedFalse()
        {
            SetupCore();
            _core.Pause();

            _core.Stop();

            Assert.That(_core.IsPaused, Is.False);
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
    }
}
