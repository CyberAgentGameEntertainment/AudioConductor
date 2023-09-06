// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;

namespace AudioConductor.Editor.Foundation.TinyRx
{
    internal interface ISubject<T> : IObserver<T>, IObservable<T>
    {
    }
}
