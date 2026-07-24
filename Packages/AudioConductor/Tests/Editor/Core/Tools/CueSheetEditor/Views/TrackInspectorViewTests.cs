// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Editor.Core.Tools.Shared;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Views.Tests
{
    internal class TrackInspectorViewTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(1)]
        public void ShouldShowEndSampleSentinelHelp_HasMultipleDifferentValues_ReturnsFalse(int value)
        {
            var mixedValue = new MixedValue<int>(value, true);
            Assert.That(TrackInspectorView.ShouldShowEndSampleSentinelHelp(mixedValue), Is.False);
        }

        [TestCase(0, true)]
        [TestCase(-1, true)]
        [TestCase(1, false)]
        public void ShouldShowEndSampleSentinelHelp_SingleValue_ReturnsExpected(int value, bool expected)
        {
            var mixedValue = new MixedValue<int>(value, false);
            Assert.That(TrackInspectorView.ShouldShowEndSampleSentinelHelp(mixedValue), Is.EqualTo(expected));
        }
    }
}
