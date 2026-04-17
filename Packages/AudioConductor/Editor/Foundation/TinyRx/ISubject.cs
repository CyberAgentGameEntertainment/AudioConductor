// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;

namespace AudioConductor.Editor.Foundation.TinyRx
{
    internal interface ISubject<T> : IObserver<T>, IObservable<T>
    {
    }
}
