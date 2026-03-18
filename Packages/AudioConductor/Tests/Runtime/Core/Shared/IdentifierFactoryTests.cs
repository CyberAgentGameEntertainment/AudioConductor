// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;

namespace AudioConductor.Core.Shared.Tests
{
    public class IdentifierFactoryTests
    {
        [Test]
        public void Create_ConsecutiveCalls_ReturnDifferentValues()
        {
            var id1 = IdentifierFactory.Create();
            var id2 = IdentifierFactory.Create();

            Assert.That(id1, Is.Not.EqualTo(id2));
        }

        [Test]
        public void Create_ReturnedValue_IsNotEmpty()
        {
            var id = IdentifierFactory.Create();

            Assert.That(id, Is.Not.Empty);
        }
    }
}
