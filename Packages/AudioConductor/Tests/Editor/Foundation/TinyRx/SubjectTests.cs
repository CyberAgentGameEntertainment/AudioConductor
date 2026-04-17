// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace AudioConductor.Editor.Foundation.TinyRx.Tests
{
    internal sealed class SubjectTests
    {
        private sealed class FakeObserver<T> : IObserver<T>
        {
            public List<T> OnNextValues { get; } = new();
            public List<Exception> OnErrorValues { get; } = new();
            public int OnCompletedCount { get; private set; }

            public void OnNext(T value)
            {
                OnNextValues.Add(value);
            }

            public void OnError(Exception error)
            {
                OnErrorValues.Add(error);
            }

            public void OnCompleted()
            {
                OnCompletedCount++;
            }
        }

        [Test]
        public void Subscribe_WithObserver_ReturnsNonNullDisposable()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();

            var disposable = subject.Subscribe(observer);

            Assert.That(disposable, Is.Not.Null);
        }

        [Test]
        public void OnNext_WithSubscribedObserver_ObserverReceivesValue()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);

            subject.OnNext(42);

            Assert.That(observer.OnNextValues, Has.Count.EqualTo(1));
            Assert.That(observer.OnNextValues[0], Is.EqualTo(42));
        }

        [Test]
        public void OnNext_WithMultipleObservers_AllReceiveValue()
        {
            var subject = new Subject<int>();
            var observer1 = new FakeObserver<int>();
            var observer2 = new FakeObserver<int>();
            subject.Subscribe(observer1);
            subject.Subscribe(observer2);

            subject.OnNext(99);

            Assert.That(observer1.OnNextValues, Has.Count.EqualTo(1));
            Assert.That(observer2.OnNextValues, Has.Count.EqualTo(1));
        }

        [Test]
        public void OnNext_AfterSubscriptionDisposed_ObserverDoesNotReceive()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            var subscription = subject.Subscribe(observer);
            subscription.Dispose();

            subject.OnNext(1);

            Assert.That(observer.OnNextValues, Is.Empty);
        }

        [Test]
        public void OnError_WithObserver_ObserverReceivesError()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);
            var error = new InvalidOperationException("test error");

            subject.OnError(error);

            Assert.That(observer.OnErrorValues, Has.Count.EqualTo(1));
            Assert.That(observer.OnErrorValues[0], Is.SameAs(error));
        }

        [Test]
        public void OnError_SetsDidTerminateAndError()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);
            var error = new InvalidOperationException("test error");

            subject.OnError(error);

            Assert.That(subject.DidTerminate, Is.True);
            Assert.That(subject.Error, Is.SameAs(error));
        }

        [Test]
        public void OnCompleted_WithObserver_ObserverReceivesCompleted()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);

            subject.OnCompleted();

            Assert.That(observer.OnCompletedCount, Is.EqualTo(1));
        }

        [Test]
        public void OnCompleted_SetsDidTerminate()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);

            subject.OnCompleted();

            Assert.That(subject.DidTerminate, Is.True);
            Assert.That(subject.Error, Is.Null);
        }

        [Test]
        public void Subscribe_AfterOnCompleted_ObserverReceivesCompletedImmediately()
        {
            var subject = new Subject<int>();
            subject.OnCompleted();

            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);

            Assert.That(observer.OnCompletedCount, Is.EqualTo(1));
        }

        [Test]
        public void Subscribe_AfterOnError_ObserverReceivesErrorImmediately()
        {
            var subject = new Subject<int>();
            var error = new InvalidOperationException("test error");
            subject.OnError(error);

            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);

            Assert.That(observer.OnErrorValues, Has.Count.EqualTo(1));
            Assert.That(observer.OnErrorValues[0], Is.SameAs(error));
        }

        [Test]
        public void Dispose_SetsDidDispose()
        {
            var subject = new Subject<int>();

            subject.Dispose();

            Assert.That(subject.DidDispose, Is.True);
        }

        [Test]
        public void Dispose_ClearsObservers()
        {
            var subject = new Subject<int>();
            var observer = new FakeObserver<int>();
            subject.Subscribe(observer);
            subject.OnNext(1);

            subject.Dispose();

            Assert.That(subject.DidDispose, Is.True);
            Assert.That(observer.OnNextValues, Has.Count.EqualTo(1));
        }
    }
}
