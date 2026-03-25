// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
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

        [Test]
        public void Setup_InitialState_IsNotPlaying()
        {
            _player.Setup(null, _clip, 0, 1f, 1f, false, 0, 0, 0);

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [Test]
        public void Setup_WithNullClip_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _player.Setup(null, null!, 0, 1f, 1f, false, 0, 0, 0));
        }

        [Test]
        public void Create_HasTwoAudioSourceChildren()
        {
            Assert.That(_player.GetComponentsInChildren<AudioSource>().Length, Is.EqualTo(2));
        }
    }
}
