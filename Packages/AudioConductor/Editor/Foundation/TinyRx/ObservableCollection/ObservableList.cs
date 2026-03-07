// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableCollection
{
    [Serializable]
    public class ObservableList<T> : ObservableListBase<T>
    {
        [SerializeField] private List<T> _internalList = new();

        protected override List<T> InternalList => _internalList;
    }
}
