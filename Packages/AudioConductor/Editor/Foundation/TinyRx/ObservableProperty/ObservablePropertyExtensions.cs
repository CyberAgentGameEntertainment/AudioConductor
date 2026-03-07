// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableProperty
{
    public static class ObservablePropertyExtensions
    {
        public static ReadOnlyObservableProperty<T> ToReadOnly<T>(this IObservableProperty<T> self)
        {
            return new ReadOnlyObservableProperty<T>(self);
        }
    }
}
