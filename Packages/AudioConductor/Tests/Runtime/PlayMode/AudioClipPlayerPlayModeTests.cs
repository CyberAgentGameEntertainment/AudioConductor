// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections;
using AudioConductor.Core.Enums;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public class AudioClipPlayerPlayModeTests
    {
        private AudioClip _clip = null!;
        private AudioClipPlayer _player = null!;

        [SetUp]
        public void SetUp()
        {
            var root = new GameObject("TestRoot");
            root.AddComponent<AudioListener>();
            _player = AudioClipPlayer.Create(root.transform);
            _clip = CreatePlayableClip();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_player.gameObject.transform.parent.gameObject);
            Object.Destroy(_clip);
        }

        private void SetupPlayer(bool isLoop = false)
        {
            _player.Setup(null, _clip, 0, 1f, 1f, isLoop, 0, 0, _clip.samples);
        }

        private static AudioClip CreatePlayableClip(int samples = 44100)
        {
            var clip = AudioClip.Create("test", samples, 1, 44100, false);
            var data = new float[samples];
            for (var i = 0; i < data.Length; i++)
                data[i] = Mathf.Sin(2f * Mathf.PI * 440f * i / 44100f);
            clip.SetData(data, 0);
            return clip;
        }

        [UnityTest]
        public IEnumerator Play_SetsIsPlayingTrue()
        {
            SetupPlayer();

            _player.Play();

            // PlayScheduled uses dspTime + 0.1s delay; wait long enough for playback to start
            yield return new WaitForSeconds(0.2f);

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
        }

        [UnityTest]
        public IEnumerator Stop_AfterPlay_SetsIsPlayingFalse()
        {
            SetupPlayer();
            _player.Play();
            yield return new WaitForSeconds(0.2f);

            _player.Stop();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [UnityTest]
        public IEnumerator Pause_AfterPlay_SetsIsPausedTrue()
        {
            SetupPlayer();
            _player.Play();
            yield return new WaitForSeconds(0.2f);

            _player.Pause();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));
        }

        [UnityTest]
        public IEnumerator Resume_AfterPause_SetsIsPausedFalse()
        {
            SetupPlayer();
            _player.Play();
            yield return new WaitForSeconds(0.2f);
            _player.Pause();

            _player.Resume();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));
        }

        [UnityTest]
        public IEnumerator ManualUpdate_AfterPlaybackEnds_InvokesEndAction()
        {
            SetupPlayer();
            var endCalled = false;
            _player.AddEndAction(() => endCalled = true);
            _player.Play();

            // Poll ManualUpdate each frame until end action fires or timeout
            var elapsed = 0f;
            const float timeout = 3f;
            while (!endCalled && elapsed < timeout)
            {
                _player.ManualUpdate(Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Assert.That(endCalled, Is.True);
        }

        [UnityTest]
        public IEnumerator Play_Loop_ContinuesPlaying()
        {
            SetupPlayer(true);

            _player.Play();
            yield return new WaitForSeconds(0.2f);

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));

            // Verify still playing after additional frames
            yield return new WaitForSeconds(0.5f);

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));

            _player.Stop();
        }

        [UnityTest]
        public IEnumerator Pause_WhenLoopPlaying_PausesCorrectly()
        {
            SetupPlayer(true);
            _player.Play();
            yield return new WaitForSeconds(0.2f);

            _player.Pause();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Paused));

            _player.Stop();
        }

        [UnityTest]
        public IEnumerator Resume_WhenLoopPaused_ResumesCorrectly()
        {
            SetupPlayer(true);
            _player.Play();
            yield return new WaitForSeconds(0.2f);
            _player.Pause();

            _player.Resume();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Playing));

            _player.Stop();
        }

        [UnityTest]
        public IEnumerator Stop_WhenLoopPlaying_SetsIsPlayingFalse()
        {
            SetupPlayer(true);
            _player.Play();
            yield return new WaitForSeconds(0.2f);

            _player.Stop();

            Assert.That(_player.State, Is.EqualTo(PlayerState.Stopped));
        }

        [UnityTest]
        public IEnumerator GetCurrentSample_WhenPlaying_ReturnsNonNegative()
        {
            SetupPlayer();
            _player.Play();
            yield return new WaitForSeconds(0.2f);

            var sample = _player.GetCurrentSample();

            Assert.That(sample, Is.GreaterThanOrEqualTo(0));

            _player.Stop();
        }

        [UnityTest]
        public IEnumerator SetCurrentSample_WhenNotPlaying_UpdatesPosition()
        {
            SetupPlayer();

            _player.SetCurrentSample(100);

            Assert.That(_player.GetCurrentSample(), Is.EqualTo(100));
            yield return null;
        }
    }
}
