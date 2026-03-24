// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using NUnit.Framework;

namespace AudioConductor.Editor.Foundation.CommandBasedUndo.Tests
{
    internal sealed class AutoIncrementHistoryTests
    {
        // --- Limit ---

        [Test]
        public void Limit_Get_ReturnsDefaultValue()
        {
            var history = new AutoIncrementHistory();

            Assert.That(history.Limit, Is.EqualTo(10000));
        }

        [Test]
        public void Limit_Set_UpdatesValue()
        {
            var history = new AutoIncrementHistory();

            history.Limit = 500;

            Assert.That(history.Limit, Is.EqualTo(500));
        }

        // --- Register ---

        [Test]
        public void Register_NullActionTypeId_ThrowsArgumentNullException()
        {
            var history = new AutoIncrementHistory();

            Assert.Throws<ArgumentNullException>(() => history.Register(null!, () => { }, () => { }));
        }

        [Test]
        public void Register_ExecutesRedo()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);

            Assert.That(counter, Is.EqualTo(1));
        }

        [Test]
        public void Register_SameActionTypeId_GroupsTogether()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);
            history.Register("a", () => counter++, () => counter--);
            // counter == 2

            history.Undo();

            // Both commands share the same group, so both are undone at once
            Assert.That(counter, Is.EqualTo(0));
        }

        [Test]
        public void Register_DifferentActionTypeId_CreatesNewGroup()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);
            history.Register("b", () => counter++, () => counter--);
            // counter == 2

            history.Undo();

            // Only the last group ("b") is undone
            Assert.That(counter, Is.EqualTo(1));
        }

        // --- Undo / Redo ---

        [Test]
        public void Undo_RevertsLastGroup()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);
            // counter == 1

            history.Undo();

            Assert.That(counter, Is.EqualTo(0));
        }

        [Test]
        public void Redo_ReexecutesUndoneGroup()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);
            history.Undo();
            // counter == 0

            history.Redo();

            Assert.That(counter, Is.EqualTo(1));
        }

        [Test]
        public void Undo_MultipleGroups_RevertsOneGroupAtATime()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);
            history.Register("b", () => counter++, () => counter--);
            // counter == 2

            history.Undo();
            Assert.That(counter, Is.EqualTo(1));

            history.Undo();
            Assert.That(counter, Is.EqualTo(0));
        }

        // --- Clear ---

        [Test]
        public void Clear_RemovesAllHistory()
        {
            var history = new AutoIncrementHistory();
            var counter = 0;

            history.Register("a", () => counter++, () => counter--);
            history.Register("b", () => counter++, () => counter--);
            // counter == 2

            history.Clear();

            history.Undo();
            Assert.That(counter, Is.EqualTo(2)); // unchanged

            history.Redo();
            Assert.That(counter, Is.EqualTo(2)); // unchanged
        }
    }
}
