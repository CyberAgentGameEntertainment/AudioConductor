// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableCollection
{
    public static class ObservableCollectionExtensions
    {
        public static IReadOnlyObservableList<TValue> ToReadOnly<TValue>(this IObservableList<TValue> self)
            => (IReadOnlyObservableList<TValue>)self;
    }
}
