// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using NUnit.Framework;

namespace AudioConductor.Editor.Foundation.TinyRx.Tests
{
    internal sealed class CompositeDisposableTests
    {
        private sealed class FakeDisposable : IDisposable
        {
            public int DisposeCount { get; private set; }

            public void Dispose()
            {
                DisposeCount++;
            }
        }

        [Test]
        public void Constructor_Default_CountIsZeroAndNotDisposed()
        {
            var cd = new CompositeDisposable();

            Assert.That(cd.Count, Is.EqualTo(0));
            Assert.That(cd.IsDisposed, Is.False);
        }

        [Test]
        public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CompositeDisposable(-1));
        }

        [Test]
        public void Add_SingleItem_CountIsOne()
        {
            var cd = new CompositeDisposable();
            var item = new FakeDisposable();

            cd.Add(item);

            Assert.That(cd.Count, Is.EqualTo(1));
        }

        [Test]
        public void Add_NullItem_ThrowsArgumentNullException()
        {
            var cd = new CompositeDisposable();

            Assert.Throws<ArgumentNullException>(() => cd.Add(null!));
        }

        [Test]
        public void Add_AfterDispose_ItemIsDisposedImmediately()
        {
            var cd = new CompositeDisposable();
            cd.Dispose();
            var item = new FakeDisposable();

            cd.Add(item);

            Assert.That(item.DisposeCount, Is.EqualTo(1));
            Assert.That(cd.Count, Is.EqualTo(0));
        }

        [Test]
        public void Remove_ExistingItem_ReturnsTrueAndDisposesItem()
        {
            var cd = new CompositeDisposable();
            var item = new FakeDisposable();
            cd.Add(item);

            var result = cd.Remove(item);

            Assert.That(result, Is.True);
            Assert.That(item.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void Remove_NonExistingItem_ReturnsTrueAndDisposesItem()
        {
            var cd = new CompositeDisposable();
            var item = new FakeDisposable();

            var result = cd.Remove(item);

            Assert.That(result, Is.True);
            Assert.That(item.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void Remove_NullItem_ThrowsArgumentNullException()
        {
            var cd = new CompositeDisposable();

            Assert.Throws<ArgumentNullException>(() => cd.Remove(null!));
        }

        [Test]
        public void Remove_AfterDispose_ReturnsFalse()
        {
            var cd = new CompositeDisposable();
            var item = new FakeDisposable();
            cd.Add(item);
            cd.Dispose();

            var result = cd.Remove(item);

            Assert.That(result, Is.False);
        }

        [Test]
        public void Clear_WithItems_DisposesAllAndCountIsZero()
        {
            var cd = new CompositeDisposable();
            var item1 = new FakeDisposable();
            var item2 = new FakeDisposable();
            cd.Add(item1);
            cd.Add(item2);

            cd.Clear();

            Assert.That(item1.DisposeCount, Is.EqualTo(1));
            Assert.That(item2.DisposeCount, Is.EqualTo(1));
            Assert.That(cd.Count, Is.EqualTo(0));
            Assert.That(cd.IsDisposed, Is.False);
        }

        [Test]
        public void Dispose_WithItems_DisposesAllAndSetsIsDisposed()
        {
            var cd = new CompositeDisposable();
            var item1 = new FakeDisposable();
            var item2 = new FakeDisposable();
            cd.Add(item1);
            cd.Add(item2);

            cd.Dispose();

            Assert.That(item1.DisposeCount, Is.EqualTo(1));
            Assert.That(item2.DisposeCount, Is.EqualTo(1));
            Assert.That(cd.IsDisposed, Is.True);
            Assert.That(cd.Count, Is.EqualTo(0));
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var cd = new CompositeDisposable();
            cd.Dispose();

            Assert.DoesNotThrow(() => cd.Dispose());
        }

        [Test]
        public void Contains_ExistingItem_ReturnsTrue()
        {
            var cd = new CompositeDisposable();
            var item = new FakeDisposable();
            cd.Add(item);

            Assert.That(cd.Contains(item), Is.True);
        }

        [Test]
        public void Contains_NonExistingItem_ReturnsFalse()
        {
            var cd = new CompositeDisposable();
            var item = new FakeDisposable();

            Assert.That(cd.Contains(item), Is.False);
        }

        [Test]
        public void Contains_NullItem_ThrowsArgumentNullException()
        {
            var cd = new CompositeDisposable();

            Assert.Throws<ArgumentNullException>(() => cd.Contains(null!));
        }
    }
}
