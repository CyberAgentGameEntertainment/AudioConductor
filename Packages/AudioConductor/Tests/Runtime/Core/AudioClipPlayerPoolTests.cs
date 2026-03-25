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
    public class AudioClipPlayerPoolTests
    {
        private AudioClipPlayerPool _pool = null!;
        private GameObject _root = null!;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot");
            _pool = new AudioClipPlayerPool(_root.transform, true);
        }

        [TearDown]
        public void TearDown()
        {
            // Destroy parent first so OnClear() finds null instances and skips Object.Destroy()
            Object.DestroyImmediate(_root);
            _pool.Dispose();
        }

        [Test]
        public void Rent_ReturnsAudioClipPlayerInstance()
        {
            var player = _pool.Rent();

            Assert.That(player, Is.Not.Null);
            Assert.That(player, Is.InstanceOf<AudioClipPlayer>());

            _pool.Return(player);
        }

        [Test]
        public void OnBeforeRent_ActivatesGameObject()
        {
            var player = _pool.Rent();
            _pool.Return(player);

            var rented = _pool.Rent();

            Assert.That(rented.gameObject.activeSelf, Is.True);

            _pool.Return(rented);
        }

        [Test]
        public void OnBeforeReturn_DeactivatesGameObject()
        {
            var player = _pool.Rent();

            _pool.Return(player);

            Assert.That(player.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void OnBeforeReturn_CallsResetState()
        {
            var player = _pool.Rent();
            player.FadeState = FadeState.FadingOut;
            player.ActiveFadeId = 1u;

            _pool.Return(player);

            Assert.That(player.FadeState, Is.EqualTo(FadeState.None));
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
        }
    }
}
