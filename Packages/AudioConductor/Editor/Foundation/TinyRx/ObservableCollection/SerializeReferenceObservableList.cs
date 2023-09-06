// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AudioConductor.Editor.Foundation.TinyRx.ObservableCollection
{
    [Serializable]
    public abstract class SerializeReferenceObservableList<T> : ObservableListBase<T>
    {
        [SerializeReference]
        private List<T> _internalList = new();

        protected override List<T> InternalList => _internalList;
    }
}
