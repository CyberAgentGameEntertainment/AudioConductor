// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableProperty
{
    public static class ObservablePropertyExtensions
    {
        public static ReadOnlyObservableProperty<T> ToReadOnly<T>(this IObservableProperty<T> self) => new(self);
    }
}
