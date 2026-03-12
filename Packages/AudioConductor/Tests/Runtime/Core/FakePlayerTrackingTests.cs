// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class FakePlayerTrackingTests
    {
        private FakePlayer _player = null!;

        [SetUp]
        public void SetUp()
        {
            _player = new FakePlayer();
        }

        [Test]
        public void Play_IncrementsPlayCount()
        {
            _player.Play();

            Assert.That(_player.PlayCount, Is.EqualTo(1));
        }

        [Test]
        public void Play_CalledTwice_PlayCountIsTwo()
        {
            _player.Play();
            _player.Stop();
            _player.Play();

            Assert.That(_player.PlayCount, Is.EqualTo(2));
        }

        [Test]
        public void Pause_WhenPlaying_IncrementsPauseCount()
        {
            _player.Play();
            _player.Pause();

            Assert.That(_player.PauseCount, Is.EqualTo(1));
        }

        [Test]
        public void Pause_WhenNotPlaying_DoesNotIncrementPauseCount()
        {
            _player.Pause();

            Assert.That(_player.PauseCount, Is.EqualTo(0));
        }

        [Test]
        public void Resume_WhenPaused_IncrementsResumeCount()
        {
            _player.Play();
            _player.Pause();
            _player.Resume();

            Assert.That(_player.ResumeCount, Is.EqualTo(1));
        }

        [Test]
        public void Resume_WhenNotPaused_DoesNotIncrementResumeCount()
        {
            _player.Resume();

            Assert.That(_player.ResumeCount, Is.EqualTo(0));
        }

        [Test]
        public void Setup_IncrementsSetupCount()
        {
            _player.Setup(null, null!, 0, 1f, 1f, false, 0, 0, 0);

            Assert.That(_player.SetupCount, Is.EqualTo(1));
        }

        [Test]
        public void Setup_RecordsArguments()
        {
            _player.Setup(null, null!, 5, 0.7f, 1.2f, true, 10, 20, 30);

            Assert.That(_player.SetupVolume, Is.EqualTo(0.7f));
            Assert.That(_player.SetupPitch, Is.EqualTo(1.2f));
            Assert.That(_player.SetupIsLoop, Is.True);
            Assert.That(_player.SetupStartSample, Is.EqualTo(10));
            Assert.That(_player.SetupLoopStartSample, Is.EqualTo(20));
            Assert.That(_player.SetupEndSample, Is.EqualTo(30));
        }

        [Test]
        public void SetVolume_RecordsVolume()
        {
            _player.SetVolume(0.5f);

            Assert.That(_player.Volume, Is.EqualTo(0.5f));
        }

        [Test]
        public void GetVolume_ReturnsRecordedVolume()
        {
            _player.SetVolume(0.8f);

            Assert.That(_player.GetVolume(), Is.EqualTo(0.8f));
        }

        [Test]
        public void SetPitch_RecordsPitch()
        {
            _player.SetPitch(1.5f);

            Assert.That(_player.Pitch, Is.EqualTo(1.5f));
        }

        [Test]
        public void GetPitch_ReturnsRecordedPitch()
        {
            _player.SetPitch(0.75f);

            Assert.That(_player.GetPitch(), Is.EqualTo(0.75f));
        }

        [Test]
        public void SetMasterVolume_RecordsMasterVolume()
        {
            _player.SetMasterVolume(0.6f);

            Assert.That(_player.MasterVolume, Is.EqualTo(0.6f));
        }

        [Test]
        public void SetCurrentSample_RecordsCurrentSample()
        {
            _player.SetCurrentSample(1000);

            Assert.That(_player.CurrentSample, Is.EqualTo(1000));
        }

        [Test]
        public void GetCurrentSample_ReturnsRecordedSample()
        {
            _player.SetCurrentSample(500);

            Assert.That(_player.GetCurrentSample(), Is.EqualTo(500));
        }

        [Test]
        public void ManualUpdate_IncrementsManualUpdateCount()
        {
            _player.ManualUpdate(0.016f);

            Assert.That(_player.ManualUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void ManualUpdate_RecordsLastDeltaTime()
        {
            _player.ManualUpdate(0.033f);

            Assert.That(_player.LastDeltaTime, Is.EqualTo(0.033f));
        }

        [Test]
        public void AddStopAction_RecordsLastStopAction()
        {
            Action action = () => { };
            _player.AddStopAction(action);

            Assert.That(_player.LastStopAction, Is.SameAs(action));
        }

        [Test]
        public void AddEndAction_RecordsLastEndAction()
        {
            Action action = () => { };
            _player.AddEndAction(action);

            Assert.That(_player.LastEndAction, Is.SameAs(action));
        }

        [Test]
        public void ResetState_ResetsAllTrackingFields()
        {
            _player.Play();
            _player.Pause();
            _player.Resume();
            _player.Setup(null, null!, 1, 0.5f, 1.2f, true, 1, 2, 3);
            _player.SetVolume(0.5f);
            _player.SetPitch(1.5f);
            _player.SetMasterVolume(0.8f);
            _player.SetCurrentSample(100);
            _player.ManualUpdate(0.016f);
            _player.AddStopAction(() => { });
            _player.AddEndAction(() => { });

            _player.ResetState();

            Assert.That(_player.PlayCount, Is.EqualTo(0));
            Assert.That(_player.PauseCount, Is.EqualTo(0));
            Assert.That(_player.ResumeCount, Is.EqualTo(0));
            Assert.That(_player.SetupCount, Is.EqualTo(0));
            Assert.That(_player.ManualUpdateCount, Is.EqualTo(0));
            Assert.That(_player.Volume, Is.EqualTo(0f));
            Assert.That(_player.Pitch, Is.EqualTo(0f));
            Assert.That(_player.MasterVolume, Is.EqualTo(0f));
            Assert.That(_player.CurrentSample, Is.EqualTo(0));
            Assert.That(_player.LastDeltaTime, Is.EqualTo(0f));
            Assert.That(_player.SetupVolume, Is.EqualTo(0f));
            Assert.That(_player.SetupPitch, Is.EqualTo(0f));
            Assert.That(_player.SetupIsLoop, Is.False);
            Assert.That(_player.SetupStartSample, Is.EqualTo(0));
            Assert.That(_player.SetupLoopStartSample, Is.EqualTo(0));
            Assert.That(_player.SetupEndSample, Is.EqualTo(0));
            Assert.That(_player.LastStopAction, Is.Null);
            Assert.That(_player.LastEndAction, Is.Null);
        }
    }
}
