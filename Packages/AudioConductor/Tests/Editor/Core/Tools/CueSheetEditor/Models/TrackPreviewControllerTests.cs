// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Tests
{
    internal class TrackPreviewControllerTests
    {
        private AudioClip _clip = null!;

        [SetUp]
        public void SetUp()
        {
            _clip = AudioClip.Create("TestClip", 44100, 1, 44100, false);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_clip);
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void IsPlaying_InitialState_ReturnsFalse(bool isLoop, int startSample, int loopStartSample,
            int endSample)
        {
            using var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            Assert.That(controller.IsPlaying, Is.False);
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void IsPlaying_AfterDispose_ReturnsFalse(bool isLoop, int startSample, int loopStartSample,
            int endSample)
        {
            var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            controller.Dispose();
            Assert.That(controller.IsPlaying, Is.False);
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void GetCurrentSample_BeforePlay_ReturnsZero(bool isLoop, int startSample, int loopStartSample,
            int endSample)
        {
            using var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            Assert.That(controller.GetCurrentSample(), Is.EqualTo(0));
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void Dispose_CalledTwice_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            controller.Dispose();
            Assert.DoesNotThrow(() => controller.Dispose());
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void Play_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            using var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            Assert.DoesNotThrow(() => controller.Play());
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void Stop_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            using var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            Assert.DoesNotThrow(() => controller.Stop());
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void Pause_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            using var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            Assert.DoesNotThrow(() => controller.Pause());
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void UnPause_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            using var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            Assert.DoesNotThrow(() => controller.UnPause());
        }

        [Test]
        public void SetCurrentSample_ThenGetCurrentSample_ReturnsSetValue()
        {
            using var controller = new TrackPreviewController(_clip, -1, 1f, 1f, false, 0, 0, 0);
            controller.SetCurrentSample(100);
            Assert.That(controller.GetCurrentSample(), Is.EqualTo(100));
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void Play_AfterDispose_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            controller.Dispose();
            Assert.DoesNotThrow(() => controller.Play());
        }

        [TestCase(false, 0, 0, 0)]
        [TestCase(true, 0, 1000, 40000)]
        public void Stop_AfterDispose_DoesNotThrow(bool isLoop, int startSample, int loopStartSample, int endSample)
        {
            var controller =
                new TrackPreviewController(_clip, -1, 1f, 1f, isLoop, startSample, loopStartSample, endSample);
            controller.Dispose();
            Assert.DoesNotThrow(() => controller.Stop());
        }
    }
}
