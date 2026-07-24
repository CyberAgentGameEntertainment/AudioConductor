// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;
using AudioConductor.Editor.Core.Tools.Validation.Rules;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Validation.Tests
{
    internal sealed class CueSheetValidatorTests
    {
        private CueSheetAsset _asset = null!;
        private int _nextCueId;

        [SetUp]
        public void SetUp()
        {
            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            _nextCueId = 1;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_asset);
        }

        private static CueSheetValidator NoRules()
        {
            return new CueSheetValidator(new List<ICueSheetValidationRule>(),
                new List<ICueValidationRule>(),
                new List<ITrackValidationRule>());
        }

        private static CueSheetValidator CueSheetOnly(params ICueSheetValidationRule[] rules)
        {
            return new CueSheetValidator(new List<ICueSheetValidationRule>(rules),
                new List<ICueValidationRule>(),
                new List<ITrackValidationRule>());
        }

        private static CueSheetValidator CueOnly(params ICueValidationRule[] rules)
        {
            return new CueSheetValidator(new List<ICueSheetValidationRule>(),
                new List<ICueValidationRule>(rules),
                new List<ITrackValidationRule>());
        }

        private static CueSheetValidator TrackOnly(params ITrackValidationRule[] rules)
        {
            return new CueSheetValidator(new List<ICueSheetValidationRule>(),
                new List<ICueValidationRule>(),
                new List<ITrackValidationRule>(rules));
        }

        private Cue MakeCue(string name = "Cue", List<Track>? tracks = null,
            CuePlayType playType = CuePlayType.Sequential)
        {
            var cue = new Cue { name = name, cueId = _nextCueId++, playType = playType };
            if (tracks != null)
                cue.trackList.AddRange(tracks);
            return cue;
        }

        private static Track MakeTrack(string name = "Track")
        {
            return new Track { name = name };
        }

        [Test]
        public void Validate_WithAllEmptyInjectedRules_ReturnsNoIssues()
        {
            var issues = NoRules().Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_DefaultConstructor_ActivatesAllRules()
        {
            var validator = new CueSheetValidator();

            var issues = validator.Validate(_asset, null);

            Assert.That(issues.Select(i => i.Code), Contains.Item("CueSheet.EmptyCueList"));
        }

        [Test]
        public void Validate_EmptyCueList_ReturnsWarning()
        {
            var issues = CueSheetOnly(new EmptyCueListRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("CueSheet.EmptyCueList", ValidationSeverity.Warning) }));
        }

        [Test]
        public void Validate_NonEmptyCueList_NoEmptyCueListWarning()
        {
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { MakeTrack() }));

            var issues = CueSheetOnly(new EmptyCueListRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_CueWithNoTracks_ReturnsError()
        {
            _asset.cueSheet.cueList.Add(MakeCue());

            var issues = CueOnly(new EmptyTrackListRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Cue.EmptyTrackList", ValidationSeverity.Error) }));
        }

        [Test]
        public void Validate_CueWithTracks_NoCueEmptyTrackListError()
        {
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { MakeTrack() }));

            var issues = CueOnly(new EmptyTrackListRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_CueThrottleExceedsCueSheet_BothNonZero_ReturnsWarning()
        {
            _asset.cueSheet.throttleLimit = 2;
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.throttleLimit = 5;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new ThrottleExceedsCueSheetRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Cue.ThrottleExceedsCueSheet", ValidationSeverity.Warning) }));
        }

        [Test]
        public void Validate_CueThrottleExceedsCueSheet_MinimalExcess_ReturnsWarning()
        {
            _asset.cueSheet.throttleLimit = 2;
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.throttleLimit = 3;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new ThrottleExceedsCueSheetRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Cue.ThrottleExceedsCueSheet", ValidationSeverity.Warning) }));
        }

        [Test]
        public void Validate_CueThrottleEqualsCueSheet_NoThrottleWarning()
        {
            _asset.cueSheet.throttleLimit = 2;
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.throttleLimit = 2;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new ThrottleExceedsCueSheetRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_CueSheetThrottleZero_NoThrottleWarning()
        {
            _asset.cueSheet.throttleLimit = 0;
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.throttleLimit = 5;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new ThrottleExceedsCueSheetRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_CueThrottleZero_NoThrottleWarning()
        {
            _asset.cueSheet.throttleLimit = 2;
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.throttleLimit = 0;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new ThrottleExceedsCueSheetRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_CategoryIdNotInSettings_ReturnsWarning()
        {
            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            settings.categoryList.Add(new Category { id = 0, name = "BGM" });
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.categoryId = 99;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new InvalidCategoryIdRule()).Validate(_asset, settings);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Cue.InvalidCategoryId", ValidationSeverity.Warning) }));

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Validate_CategoryIdInSettings_NoCategoryWarning()
        {
            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            settings.categoryList.Add(new Category { id = 0, name = "BGM" });
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.categoryId = 0;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new InvalidCategoryIdRule()).Validate(_asset, settings);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Validate_SettingsNull_NoCategoryWarning()
        {
            var cue = MakeCue(tracks: new List<Track> { MakeTrack() });
            cue.categoryId = 99;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new InvalidCategoryIdRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_TrackAudioClipNull_ReturnsError()
        {
            var track = new Track { name = "T", audioClip = null };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new MissingAudioClipRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.MissingAudioClip", ValidationSeverity.Error) }));
        }

        [Test]
        public void Validate_TrackAudioClipNotNull_NoMissingAudioClipError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, endSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new MissingAudioClipRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackEndSampleZeroWithoutClip_NoEndSampleOutOfRangeError()
        {
            // endSample = 0 is the sentinel meaning "play to clip end", not an error.
            var track = new Track { name = "T", audioClip = null, endSample = 0 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new EndSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_TrackStartSampleGeEndSample_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 100, endSample = 50 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new InvalidSampleRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.InvalidSampleRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackStartSampleEqualEndSample_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 100, endSample = 100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new InvalidSampleRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.InvalidSampleRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackBothSamplesZero_NoSampleRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 0, endSample = 0 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new InvalidSampleRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackStartSampleNonZeroWithEndSampleZero_NoSampleRangeError()
        {
            // endSample = 0 is the sentinel meaning "play to clip end"; the guard must
            // skip the range check even when startSample alone would exceed 0.
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 100, endSample = 0 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new InvalidSampleRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackValidSampleRange_NoSampleRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 0, endSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new InvalidSampleRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackAudioClipNullWithInvalidSampleRange_ReturnsAllErrors()
        {
            var track = new Track { name = "T", audioClip = null, startSample = 100, endSample = 50 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new MissingAudioClipRule(), new InvalidSampleRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.MissingAudioClip", ValidationSeverity.Error),
                    ("Track.InvalidSampleRange", ValidationSeverity.Error)
                }));
        }

        [Test]
        public void Validate_LoopTrackLoopStartExceedsEnd_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 1000, isLoop = true, loopStartSample = 2000 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.LoopStartOutOfRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_LoopTrackLoopStartWithinEnd_NoLoopStartError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 1000, isLoop = true, loopStartSample = 500 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_LoopTrackLoopStartEqualsEnd_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 1000, isLoop = true, loopStartSample = 1000 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.LoopStartOutOfRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_NonLoopTrackLoopStartExceedsEnd_NoLoopStartError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 1000, isLoop = false, loopStartSample = 2000 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_LoopTrackEndSampleZero_NoLoopStartOutOfRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 0, isLoop = true, loopStartSample = 0 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_LoopTrackLoopStartNonZeroWithEndSampleZero_NoLoopStartOutOfRangeError()
        {
            // endSample = 0 is the sentinel meaning "play to clip end"; the guard must
            // skip the range check even when loopStartSample alone would exceed 0.
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 0, isLoop = true, loopStartSample = 500 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackAudioClipNullWithLoopStartOutOfRange_ReturnsAllErrors()
        {
            var track = new Track
                { name = "T", audioClip = null, isLoop = true, endSample = 1000, loopStartSample = 2000 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new MissingAudioClipRule(), new LoopStartOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.MissingAudioClip", ValidationSeverity.Error),
                    ("Track.LoopStartOutOfRange", ValidationSeverity.Error)
                }));
        }

        [Test]
        public void Validate_TrackStartSampleNegative_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = -1, endSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new StartSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.StartSampleOutOfRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackStartSampleExceedsClip_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 44100, endSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new InvalidSampleRangeRule(), new StartSampleOutOfRangeRule())
                .Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.InvalidSampleRange", ValidationSeverity.Error),
                    ("Track.StartSampleOutOfRange", ValidationSeverity.Error)
                }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackStartSampleWithinRange_NoStartSampleOutOfRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, startSample = 0, endSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new StartSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackAudioClipNullWithNegativeStartSample_ReturnsStartSampleOutOfRangeError()
        {
            var track = new Track { name = "T", audioClip = null, startSample = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new MissingAudioClipRule(), new StartSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.MissingAudioClip", ValidationSeverity.Error),
                    ("Track.StartSampleOutOfRange", ValidationSeverity.Error)
                }));
        }

        [Test]
        public void Validate_TrackEndSampleNegative_NoEndSampleOutOfRangeError()
        {
            // Negative endSample values (e.g. -1) are a valid sentinel meaning
            // "play to clip end", not an error.
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, endSample = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new EndSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackEndSampleExceedsClip_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, endSample = 44101 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new EndSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.EndSampleOutOfRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackEndSampleEqualsClipSamples_NoEndSampleOutOfRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, endSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new EndSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackEndSampleZero_NoEndSampleOutOfRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track { name = "T", audioClip = clip, endSample = 0 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new EndSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackAudioClipNullWithNegativeEndSample_ReturnsOnlyMissingAudioClipError()
        {
            // Negative endSample is a valid sentinel, so only the missing-clip error remains.
            var track = new Track { name = "T", audioClip = null, endSample = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new EndSampleOutOfRangeRule(), new MissingAudioClipRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.MissingAudioClip", ValidationSeverity.Error) }));
        }

        [Test]
        public void Validate_LoopTrackLoopStartSampleNegative_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 44100, isLoop = true, loopStartSample = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.LoopStartSampleOutOfRange", ValidationSeverity.Error) }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_LoopTrackLoopStartSampleExceedsClip_ReturnsError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 44100, isLoop = true, loopStartSample = 44100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartOutOfRangeRule(), new LoopStartSampleOutOfRangeRule())
                .Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.LoopStartOutOfRange", ValidationSeverity.Error),
                    ("Track.LoopStartSampleOutOfRange", ValidationSeverity.Error)
                }));

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_LoopTrackLoopStartSampleWithinRange_NoLoopStartSampleOutOfRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 44100, isLoop = true, loopStartSample = 100 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_NonLoopTrackLoopStartSampleNegative_NoLoopStartSampleOutOfRangeError()
        {
            var clip = AudioClip.Create("clip", 44100, 1, 44100, false);
            var track = new Track
                { name = "T", audioClip = clip, endSample = 44100, isLoop = false, loopStartSample = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartSampleOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip);
        }

        [Test]
        public void Validate_TrackAudioClipNullWithNegativeLoopStartSample_ReturnsLoopStartSampleOutOfRangeError()
        {
            var track = new Track { name = "T", audioClip = null, isLoop = true, loopStartSample = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new LoopStartSampleOutOfRangeRule(), new MissingAudioClipRule())
                .Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.LoopStartSampleOutOfRange", ValidationSeverity.Error),
                    ("Track.MissingAudioClip", ValidationSeverity.Error)
                }));
        }

        [Test]
        public void Validate_RandomCuePartialWeightZero_ReturnsWarning()
        {
            var clip1 = AudioClip.Create("clip1", 44100, 1, 44100, false);
            var clip2 = AudioClip.Create("clip2", 44100, 1, 44100, false);
            var cue = MakeCue(tracks: new List<Track>
            {
                new() { name = "T1", audioClip = clip1, endSample = 44100, randomWeight = 1 },
                new() { name = "T2", audioClip = clip2, endSample = 44100, randomWeight = 0 }
            });
            cue.playType = CuePlayType.Random;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new PartialWeightZeroRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[] { ("Track.PartialWeightZero", ValidationSeverity.Warning) }));

            Object.DestroyImmediate(clip1);
            Object.DestroyImmediate(clip2);
        }

        [Test]
        public void Validate_RandomCueAllWeightZero_NoPartialWeightZeroWarnings()
        {
            var clip1 = AudioClip.Create("clip1", 44100, 1, 44100, false);
            var clip2 = AudioClip.Create("clip2", 44100, 1, 44100, false);
            var cue = MakeCue(tracks: new List<Track>
            {
                new() { name = "T1", audioClip = clip1, endSample = 44100, randomWeight = 0 },
                new() { name = "T2", audioClip = clip2, endSample = 44100, randomWeight = 0 }
            });
            cue.playType = CuePlayType.Random;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new PartialWeightZeroRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip1);
            Object.DestroyImmediate(clip2);
        }

        [Test]
        public void Validate_RandomCueAllWeightNonZero_NoPartialWeightZeroWarning()
        {
            var clip1 = AudioClip.Create("clip1", 44100, 1, 44100, false);
            var clip2 = AudioClip.Create("clip2", 44100, 1, 44100, false);
            var cue = MakeCue(tracks: new List<Track>
            {
                new() { name = "T1", audioClip = clip1, endSample = 44100, randomWeight = 1 },
                new() { name = "T2", audioClip = clip2, endSample = 44100, randomWeight = 2 }
            });
            cue.playType = CuePlayType.Random;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new PartialWeightZeroRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip1);
            Object.DestroyImmediate(clip2);
        }

        [Test]
        public void
            Validate_RandomCueNullTrackAndZeroWeightTrack_NoExceptionReturnsStructureInvalidAndPartialWeightZeroWarning()
        {
            var clip1 = AudioClip.Create("clip1", 44100, 1, 44100, false);
            var clip2 = AudioClip.Create("clip2", 44100, 1, 44100, false);
            var cue = MakeCue(tracks: new List<Track>
            {
                new() { name = "T1", audioClip = clip1, endSample = 44100, randomWeight = 0 },
                new() { name = "T2", audioClip = clip2, endSample = 44100, randomWeight = 1 },
                null!
            });
            cue.playType = CuePlayType.Random;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new PartialWeightZeroRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.StructureInvalid", ValidationSeverity.Error),
                    ("Track.PartialWeightZero", ValidationSeverity.Warning)
                }));

            Object.DestroyImmediate(clip1);
            Object.DestroyImmediate(clip2);
        }

        [Test]
        public void Validate_SequentialCuePartialWeightZero_NoPartialWeightZeroWarning()
        {
            var clip1 = AudioClip.Create("clip1", 44100, 1, 44100, false);
            var clip2 = AudioClip.Create("clip2", 44100, 1, 44100, false);
            var cue = MakeCue(tracks: new List<Track>
            {
                new() { name = "T1", audioClip = clip1, endSample = 44100, randomWeight = 1 },
                new() { name = "T2", audioClip = clip2, endSample = 44100, randomWeight = 0 }
            });
            cue.playType = CuePlayType.Sequential;
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new PartialWeightZeroRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);

            Object.DestroyImmediate(clip1);
            Object.DestroyImmediate(clip2);
        }

        [Test]
        public void Validate_TrackRandomWeightNegative_RandomCue_ReturnsError()
        {
            var track = new Track { name = "T", audioClip = null, randomWeight = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }, playType: CuePlayType.Random));

            var issues = TrackOnly(new MissingAudioClipRule(), new RandomWeightOutOfRangeRule()).Validate(_asset, null);

            Assert.That(
                issues.Select(i => (i.Code, i.Severity)),
                Is.EquivalentTo(new[]
                {
                    ("Track.MissingAudioClip", ValidationSeverity.Error),
                    ("Track.RandomWeightOutOfRange", ValidationSeverity.Error)
                }));
        }

        [Test]
        public void Validate_TrackRandomWeightNegative_SequentialCue_NoError()
        {
            var track = new Track { name = "T", audioClip = null, randomWeight = -1 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }, playType: CuePlayType.Sequential));

            var issues = TrackOnly(new RandomWeightOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_TrackRandomWeightZero_NoRandomWeightOutOfRangeError()
        {
            var track = new Track { name = "T", audioClip = null, randomWeight = 0 };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }, playType: CuePlayType.Random));

            var issues = TrackOnly(new RandomWeightOutOfRangeRule()).Validate(_asset, null);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Validate_CueIssue_CueEditorIdIsSet()
        {
            var cue = MakeCue();
            _asset.cueSheet.cueList.Add(cue);

            var issues = CueOnly(new EmptyTrackListRule()).Validate(_asset, null);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Code, Is.EqualTo("Cue.EmptyTrackList"));
            Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
            Assert.That(issues[0].CueEditorId, Is.EqualTo(cue.Id));
        }

        [Test]
        public void Validate_MultipleCues_EachIssueHasCorrectCueEditorId()
        {
            var cue1 = MakeCue("Cue1");
            var cue2 = MakeCue("Cue2");
            _asset.cueSheet.cueList.Add(cue1);
            _asset.cueSheet.cueList.Add(cue2);

            var issues = CueOnly(new EmptyTrackListRule()).Validate(_asset, null);

            Assert.That(issues, Has.Count.EqualTo(2));
            Assert.That(issues.Select(i => i.Code), Is.All.EqualTo("Cue.EmptyTrackList"));
            Assert.That(issues.Select(i => i.Severity), Is.All.EqualTo(ValidationSeverity.Error));
            Assert.That(issues.Select(i => i.CueEditorId), Is.EquivalentTo(new[] { cue1.Id, cue2.Id }));
        }

        [Test]
        public void Validate_CueSheetIssue_CueEditorIdIsNull()
        {
            var issues = CueSheetOnly(new EmptyCueListRule()).Validate(_asset, null);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Code, Is.EqualTo("CueSheet.EmptyCueList"));
            Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Warning));
            Assert.That(issues[0].CueEditorId, Is.Null);
        }

        [TestCase(-0.01f)]
        [TestCase(1.01f)]
        public void Validate_TrackVolumeOutOfRange_ReturnsError(float volume)
        {
            var track = new Track { name = "T", volume = volume };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new TrackScalarRangeRule()).Validate(_asset, null);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Code, Is.EqualTo("Track.VolumeOutOfRange"));
            Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
        }

        [TestCase(ValueRangeConst.Volume.Min)]
        [TestCase(ValueRangeConst.Volume.Max)]
        [TestCase(0.5f)]
        public void Validate_TrackVolumeInRange_NoError(float volume)
        {
            var track = new Track { name = "T", volume = volume };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new TrackScalarRangeRule()).Validate(_asset, null);

            Assert.That(issues.Where(i => i.Code == "Track.VolumeOutOfRange"), Is.Empty);
        }

        [TestCase(0f)]
        [TestCase(0.009f)]
        [TestCase(3.01f)]
        public void Validate_TrackPitchOutOfRange_ReturnsError(float pitch)
        {
            var track = new Track { name = "T", pitch = pitch };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new TrackScalarRangeRule()).Validate(_asset, null);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Code, Is.EqualTo("Track.PitchOutOfRange"));
            Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
        }

        [TestCase(ValueRangeConst.Pitch.Min)]
        [TestCase(ValueRangeConst.Pitch.Max)]
        [TestCase(1f)]
        public void Validate_TrackPitchInRange_NoError(float pitch)
        {
            var track = new Track { name = "T", pitch = pitch };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new TrackScalarRangeRule()).Validate(_asset, null);

            Assert.That(issues.Where(i => i.Code == "Track.PitchOutOfRange"), Is.Empty);
        }

        [TestCase(-0.01f)]
        [TestCase(-1f)]
        public void Validate_TrackFadeTimeNegative_ReturnsError(float fadeTime)
        {
            var track = new Track { name = "T", fadeTime = fadeTime };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new TrackScalarRangeRule()).Validate(_asset, null);

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0].Code, Is.EqualTo("Track.FadeTimeOutOfRange"));
            Assert.That(issues[0].Severity, Is.EqualTo(ValidationSeverity.Error));
        }

        [TestCase(ValueRangeConst.FadeTime.Min)]
        [TestCase(1f)]
        public void Validate_TrackFadeTimeNonNegative_NoError(float fadeTime)
        {
            var track = new Track { name = "T", fadeTime = fadeTime };
            _asset.cueSheet.cueList.Add(MakeCue(tracks: new List<Track> { track }));

            var issues = TrackOnly(new TrackScalarRangeRule()).Validate(_asset, null);

            Assert.That(issues.Where(i => i.Code == "Track.FadeTimeOutOfRange"), Is.Empty);
        }
    }
}
