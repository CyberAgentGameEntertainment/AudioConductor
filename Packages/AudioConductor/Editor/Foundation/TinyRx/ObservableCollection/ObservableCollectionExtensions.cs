// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableCollection
{
    public static class ObservableCollectionExtensions
    {
        public static IReadOnlyObservableList<TValue> ToReadOnly<TValue>(this IObservableList<TValue> self)
        {
            return (IReadOnlyObservableList<TValue>)self;
        }
    }
}
