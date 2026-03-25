// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
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
        public void StartFade_SetsActiveFadeIdAndFadeState()
        {
            var player = new SpyPlayer();

            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            Assert.That(player.ActiveFadeId, Is.Not.EqualTo(0u));
            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingIn));
        }

        [Test]
        public void StartFade_MultipleCalls_AssignsDifferentFadeIds()
        {
            var player1 = new SpyPlayer();
            var player2 = new SpyPlayer();

            _fadeManager.StartFade(player1, Faders.Linear, 0f, 1f, 1f);
            _fadeManager.StartFade(player2, Faders.Linear, 0f, 1f, 1f);

            Assert.That(player1.ActiveFadeId, Is.Not.EqualTo(player2.ActiveFadeId));
        }

        [Test]
        public void Update_ProgressesFade_SetsVolumeFade()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.Update(0.5f);

            Assert.That(player.VolumeFade, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingIn));
        }

        [Test]
        public void Update_CompletedFadeIn_SetsFadeStateNone()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.Update(1f);

            Assert.That(player.FadeState, Is.EqualTo(FadeState.None));
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.VolumeFade, Is.EqualTo(1f).Within(0.01f));
        }

        [Test]
        public void Update_CompletedFadeOut_SetsFadingOutComplete()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 1f, 0f, 1f);

            _fadeManager.Update(1f);

            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingOutComplete));
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.VolumeFade, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void Update_StaleFade_IsReclaimed()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            // Cancel the fade (makes it stale).
            _fadeManager.CancelFade(player);

            // Update should reclaim the stale fade without error.
            _fadeManager.Update(0.5f);

            Assert.That(player.FadeState, Is.EqualTo(FadeState.None));
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
        }

        [Test]
        public void CancelFade_ResetsActiveFadeIdAndFadeState()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.CancelFade(player);

            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
            Assert.That(player.FadeState, Is.EqualTo(FadeState.None));
        }

        [Test]
        public void Dispose_ClearsAllOperations()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);

            _fadeManager.Dispose();

            // After dispose, operations are cleared; Update no longer progresses fades.
            _fadeManager.Update(1f);
            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingIn));
        }

        [Test]
        public void StartFade_MultiplePlayers_AllProgressIndependently()
        {
            var player1 = new SpyPlayer();
            var player2 = new SpyPlayer();

            _fadeManager.StartFade(player1, Faders.Linear, 0f, 1f, 1f);
            _fadeManager.StartFade(player2, Faders.Linear, 0f, 1f, 2f);

            _fadeManager.Update(1f);

            // player1 (duration 1f): elapsed 1f/1f = 1.0 → VolumeFade == 1f, FadeState == None
            Assert.That(player1.VolumeFade, Is.EqualTo(1f).Within(0.01f));
            Assert.That(player1.FadeState, Is.EqualTo(FadeState.None));

            // player2 (duration 2f): elapsed 1f/2f = 0.5 → VolumeFade == 0.5f, FadeState == FadingIn
            Assert.That(player2.VolumeFade, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(player2.FadeState, Is.EqualTo(FadeState.FadingIn));
        }

        [Test]
        public void StartFade_SamePlayerTwice_SecondOverridesFirst()
        {
            var player = new SpyPlayer();

            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);
            var firstFadeId = player.ActiveFadeId;

            _fadeManager.StartFade(player, Faders.Linear, 1f, 0f, 1f);

            Assert.That(player.ActiveFadeId, Is.Not.EqualTo(firstFadeId));
            Assert.That(player.ActiveFadeId, Is.Not.EqualTo(0u));
        }

        [Test]
        public void StartFade_SamePlayerTwice_UpdateCompletesSecondFade()
        {
            var player = new SpyPlayer();
            _fadeManager.StartFade(player, Faders.Linear, 0f, 1f, 1f);
            _fadeManager.StartFade(player, Faders.Linear, 1f, 0f, 1f);

            _fadeManager.Update(1f);

            Assert.That(player.VolumeFade, Is.EqualTo(0f).Within(0.01f));
            // Second fade is FadeOut direction, so completion yields FadingOutComplete.
            Assert.That(player.FadeState, Is.EqualTo(FadeState.FadingOutComplete));
            Assert.That(player.ActiveFadeId, Is.EqualTo(0u));
        }
    }
}
