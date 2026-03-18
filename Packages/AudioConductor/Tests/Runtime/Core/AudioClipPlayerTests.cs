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
    }
}
