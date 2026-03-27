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
    internal sealed class AudioClipPlayerTests
    {
        private AudioClip _clip = null!;
        private AudioClipPlayer _player = null!;
        private GameObject _root = null!;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot");
            _player = AudioClipPlayer.Create(_root.transform);
            _clip = AudioClip.Create("test", 44100, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
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
        public void Create_HasTwoAudioSources()
        {
            var child = _root.transform.GetChild(0).gameObject;

            Assert.That(child.GetComponents<AudioSource>().Length, Is.EqualTo(2));
        }
    }
}
