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

        [Test]
        public void IsPlaying_InitialState_ReturnsFalse()
        {
            using var controller = new TrackPreviewController(_clip, -1, 1f, 1f, false, 0);
            Assert.That(controller.IsPlaying, Is.False);
        }

        [Test]
        public void IsPlaying_AfterDispose_ReturnsFalse()
        {
            var controller = new TrackPreviewController(_clip, -1, 1f, 1f, false, 0);
            controller.Dispose();
            Assert.That(controller.IsPlaying, Is.False);
        }

        [Test]
        public void GetCurrentSample_BeforePlay_ReturnsZero()
        {
            using var controller = new TrackPreviewController(_clip, -1, 1f, 1f, false, 0);
            Assert.That(controller.GetCurrentSample(), Is.EqualTo(0));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var controller = new TrackPreviewController(_clip, -1, 1f, 1f, false, 0);
            controller.Dispose();
            Assert.DoesNotThrow(() => controller.Dispose());
        }
    }
}
