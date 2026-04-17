// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using UnityEngine;

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableCollection
{
    [Serializable]
    public class SerializeReferenceObservableDictionary<TKey, TValue> : ObservableDictionaryBase<TKey, TValue>
    {
        [SerializeReference] private TValue[] _values = null!;

        protected override TValue[] InternalValues
        {
            get => _values;
            set => _values = value;
        }
    }
}
