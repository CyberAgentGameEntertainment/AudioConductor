// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Editor.Core.Tools.Validation.Rules.Tests
{
    internal sealed class DuplicateCueIdRuleTests
    {
        private SpyValidationContext _context = null!;
        private DuplicateCueIdRule _rule = null!;

        [SetUp]
        public void SetUp()
        {
            _rule = new DuplicateCueIdRule();
            _context = new SpyValidationContext();
        }

        [Test]
        public void Validate_EmptyCueList_NoError()
        {
            var cueSheet = new CueSheet();

            _rule.Validate(cueSheet, _context);

            Assert.That(_context.Errors, Is.Empty);
        }

        [Test]
        public void Validate_SingleCueWithUniqueNonZeroId_NoError()
        {
            var cueSheet = new CueSheet { cueList = { new Cue { cueId = 1 } } };

            _rule.Validate(cueSheet, _context);

            Assert.That(_context.Errors, Is.Empty);
        }

        [Test]
        public void Validate_MultipleCuesWithUniqueNonZeroIds_NoError()
        {
            var cueSheet = new CueSheet
            {
                cueList =
                {
                    new Cue { cueId = 1 },
                    new Cue { cueId = 2 },
                    new Cue { cueId = 3 }
                }
            };

            _rule.Validate(cueSheet, _context);

            Assert.That(_context.Errors, Is.Empty);
        }

        [Test]
        public void Validate_DuplicateCueId_AddsError()
        {
            var cueSheet = new CueSheet
            {
                cueList =
                {
                    new Cue { cueId = 1 },
                    new Cue { cueId = 1 }
                }
            };

            _rule.Validate(cueSheet, _context);

            Assert.That(_context.Errors, Has.Count.EqualTo(1));
            Assert.That(_context.Errors[0].Code, Is.EqualTo("CueSheet.DuplicateCueId"));
        }

        [Test]
        public void Validate_UnassignedCueId_AddsError()
        {
            var cueSheet = new CueSheet { cueList = { new Cue { cueId = 0 } } };

            _rule.Validate(cueSheet, _context);

            Assert.That(_context.Errors, Has.Count.EqualTo(1));
            Assert.That(_context.Errors[0].Code, Is.EqualTo("CueSheet.DuplicateCueId"));
        }

        [Test]
        public void Validate_MixedUnassignedAndUniqueIds_AddsError()
        {
            var cueSheet = new CueSheet
            {
                cueList =
                {
                    new Cue { cueId = 0 },
                    new Cue { cueId = 1 }
                }
            };

            _rule.Validate(cueSheet, _context);

            Assert.That(_context.Errors, Has.Count.EqualTo(1));
        }

        private sealed class SpyValidationContext : ICueSheetValidationContext
        {
            internal List<(string Code, string Message)> Errors { get; } = new();
            internal List<(string Code, string Message)> Warnings { get; } = new();

            public void AddError(string code, string message)
            {
                Errors.Add((code, message));
            }

            public void AddWarning(string code, string message)
            {
                Warnings.Add((code, message));
            }
        }
    }
}
