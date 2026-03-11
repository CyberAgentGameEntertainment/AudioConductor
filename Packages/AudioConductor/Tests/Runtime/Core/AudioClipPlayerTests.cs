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

        private void SetupPlayer(float volume = 1f)
        {
            _player.Setup(null, _clip, 0, volume, 1f, false, 0, 0, 0);
        }

        [Test]
        public void SetCategoryVolume_AffectsGetActualVolume()
        {
            SetupPlayer(1f);

            _player.SetCategoryVolume(0.5f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void SetCategoryVolume_DefaultIsOne_GetActualVolumeUnaffected()
        {
            SetupPlayer(1f);

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ResetState_ResetsCategoryVolumeToOne()
        {
            SetupPlayer(1f);
            _player.SetCategoryVolume(0.3f);

            _player.ResetState();

            Assert.That(_player.GetActualVolume(), Is.EqualTo(1f).Within(0.0001f));
        }
    }
}
