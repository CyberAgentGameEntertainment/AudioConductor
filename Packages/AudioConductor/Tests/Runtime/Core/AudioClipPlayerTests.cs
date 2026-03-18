// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class AudioClipPlayerTests
    {
        private AudioClip _clip = null!;
        private AudioClipPlayer _player = null!;

        [SetUp]
        public void SetUp()
        {
            var root = new GameObject("TestRoot");
            _player = AudioClipPlayer.Create(root.transform);
            _clip = AudioClip.Create("test", 44100, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player.gameObject.transform.parent.gameObject);
            Object.DestroyImmediate(_clip);
        }

        private void SetupPlayer(float volume = 1f, float pitch = 1f)
        {
            _player.Setup(null, _clip, 0, volume, pitch, false, 0, 0, 0);
        }

        [Test]
        public void SetCategoryVolume_AffectsGetActualVolume()
        {
            SetupPlayer();

            _player.SetCategoryVolume(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetCategoryVolume_DefaultIsOne_GetActualVolumeUnaffected()
        {
            SetupPlayer();

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsCategoryVolumeToOne()
        {
            SetupPlayer();
            _player.SetCategoryVolume(0.3f);

            _player.ResetState();

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        // --- Volume ---

        [Test]
        public void SetMasterVolume_AffectsGetActualVolume()
        {
            SetupPlayer();

            _player.SetMasterVolume(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetMasterVolume_CombinedWithCategoryVolume_Multiplied()
        {
            SetupPlayer();

            _player.SetMasterVolume(0.5f);
            _player.SetCategoryVolume(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void SetVolumeFade_AffectsGetActualVolume()
        {
            SetupPlayer();

            _player.SetVolumeFade(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void GetVolume_InitialValue_ReturnsOne()
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

        // --- Pitch ---

        [Test]
        public void GetPitch_InitialValue_ReturnsOne()
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

        [Test]
        public void GetActualPitch_WithInternalAndExternalPitch()
        {
            SetupPlayer(pitch: 0.5f);

            _player.SetPitch(2f);

            Assert.That(_player.GetActualPitch(), Is.EqualTo(1f).Within(0.0001f));
        }

        // --- Initial State ---

        [Test]
        public void Setup_InitialState_IsNotPlaying()
        {
            SetupPlayer();

            Assert.That(_player.IsPlaying, Is.False);
        }

        [Test]
        public void ResetState_ClearsVolumeAndPitchToDefaults()
        {
            SetupPlayer();
            _player.SetMasterVolume(0.5f);
            _player.SetCategoryVolume(0.3f);
            _player.SetVolumeFade(0.7f);

            _player.ResetState();

            // ResetState resets master, category, and fade volumes to 1.
            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsPitchToDefault()
        {
            SetupPlayer();
            _player.SetPitch(2f);

            _player.ResetState();

            Assert.That(_player.GetPitch(), Is.EqualTo(1f));
        }

        // --- Stop Actions ---

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

        [Test]
        public void Stop_ResetsIsPausedToFalse()
        {
            SetupPlayer();
            _player.Pause();

            _player.Stop();

            Assert.That(_player.IsPaused, Is.False);
        }

        // --- Pause / Resume ---

        [Test]
        public void Pause_NonLoop_SetsIsPausedTrue()
        {
            SetupPlayer();

            _player.Pause();

            Assert.That(_player.IsPaused, Is.True);
        }

        [Test]
        public void Resume_AfterPause_SetsIsPausedFalse()
        {
            SetupPlayer();
            _player.Pause();

            _player.Resume();

            Assert.That(_player.IsPaused, Is.False);
        }

        [Test]
        public void Pause_WhenAlreadyPaused_DoesNothing()
        {
            SetupPlayer();
            _player.Pause();

            _player.Pause();

            Assert.That(_player.IsPaused, Is.True);
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNothing()
        {
            SetupPlayer();

            Assert.DoesNotThrow(() => _player.Resume());
            Assert.That(_player.IsPaused, Is.False);
        }
    }
}
