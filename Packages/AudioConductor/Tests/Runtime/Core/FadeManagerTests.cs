// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core;
using AudioConductor.Tests.Runtime.Core.Fakes;
using NUnit.Framework;

namespace AudioConductor.Tests.Runtime.Core
{
    /// <summary>
    ///     Tests for <see cref="FadeManager" /> fade lifecycle management.
    /// </summary>
    [TestFixture]
    internal sealed class FadeManagerTests
    {
        [SetUp]
        public void SetUp()
        {
            _fadeManager = new FadeManager();
        }

        [TearDown]
        public void TearDown()
        {
            _fadeManager.Dispose();
        }

        private FadeManager _fadeManager = null!;

        [Test]
        public void StartFade_SetsActiveFadeIdAndIsFading()
        {
            var player = new FakePlayer();

            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            Assert.That(player.ActiveFadeId, Is.Not.EqualTo(0u));
            Assert.That(player.IsFading, Is.True);
        }

        [Test]
        public void StartFade_MultipleCalls_AssignsDifferentFadeIds()
        {
            var player1 = new FakePlayer();
            var player2 = new FakePlayer();

            _fadeManager.StartFade(player1, Faders.Linear, 0f, 1f, 1f);
            _fadeManager.StartFade(player2, Faders.Linear, 0f, 1f, 1f);

            Assert.That(player1.ActiveFadeId, Is.Not.EqualTo(player2.ActiveFadeId));
        }

        [Test]
        public void Update_ProgressesFade_SetsVolumeFade()
        {
            var player = new FakePlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.Update(0.5f);

            Assert.That(player.VolumeFade, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(player.IsFading, Is.True);
        }

        [Test]
        public void Update_CompletedFade_ClearsIsFadingAndActiveFadeId()
        {
            var player = new FakePlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.Update(1f);

            Assert.That(player.IsFading, Is.False);
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.VolumeFade, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public void Update_StaleFade_IsReclaimed()
        {
            var player = new FakePlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            // Cancel the fade (makes it stale).
            _fadeManager.CancelFade(player);

            // Update should reclaim the stale fade without error.
            _fadeManager.Update(0.5f);

            Assert.That(player.IsFading, Is.False);
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
        }

        [Test]
        public void MarkFadeOut_RegistersTarget()
        {
            var player = new FakePlayer();

            _fadeManager.MarkFadeOut(player);

            Assert.That(_fadeManager.IsFadingOut(player), Is.True);
        }

        [Test]
        public void IsFadingOut_UnregisteredTarget_ReturnsFalse()
        {
            var player = new FakePlayer();

            Assert.That(_fadeManager.IsFadingOut(player), Is.False);
        }

        [Test]
        public void RemoveFadeOutTarget_RemovesFromTracking()
        {
            var player = new FakePlayer();

            _fadeManager.MarkFadeOut(player);
            _fadeManager.RemoveFadeOutTarget(player);

            Assert.That(_fadeManager.IsFadingOut(player), Is.False);
        }

        [Test]
        public void CancelFade_ResetsActiveFadeIdAndIsFading()
        {
            var player = new FakePlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.CancelFade(player);

            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.IsFading, Is.False);
        }

        [Test]
        public void CancelFade_RemovesFadeOutTarget()
        {
            var player = new FakePlayer();
            _fadeManager.MarkFadeOut(player);

            _fadeManager.CancelFade(player);

            Assert.That(_fadeManager.IsFadingOut(player), Is.False);
        }

        [Test]
        public void Dispose_ClearsAllState()
        {
            var player = new FakePlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);
            _fadeManager.MarkFadeOut(player);

            _fadeManager.Dispose();

            // After dispose, fade-out tracking is cleared.
            Assert.That(_fadeManager.IsFadingOut(player), Is.False);
        }
    }
}
