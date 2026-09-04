// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using NUnit.Framework;

namespace AudioConductor.Core.Tests
{
    [TestFixture]
    internal sealed class ThrottleScopeKindTests
    {
        [Test]
        public void Count_MatchesEnumMemberCount()
        {
            var memberCount = Enum.GetValues(typeof(ThrottleScopeKind)).Length;

            // Count itself is one of the enum members (the sentinel), so actual
            // scope values number (int)Count, and members total (int)Count + 1.
            Assert.That(memberCount, Is.EqualTo((int)ThrottleScopeKind.Count + 1));
        }

        [Test]
        public void Values_DefineResolveAndEvictionOrder()
        {
            Assert.That((int)ThrottleScopeKind.Cue, Is.EqualTo(0));
            Assert.That((int)ThrottleScopeKind.Sheet, Is.EqualTo(1));
            Assert.That((int)ThrottleScopeKind.Category, Is.EqualTo(2));
            Assert.That((int)ThrottleScopeKind.Global, Is.EqualTo(3));
            Assert.That((int)ThrottleScopeKind.Count, Is.EqualTo(4));
        }
    }
}
