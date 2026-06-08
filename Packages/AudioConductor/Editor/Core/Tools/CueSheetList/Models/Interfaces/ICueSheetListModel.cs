// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Collections.Generic;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Models.Interfaces
{
    internal interface ICueSheetListModel
    {
        IReadOnlyObservableProperty<CueSheetListItem[]> Items { get; }
        IObservableProperty<string> SearchFilter { get; }
        IObservable<CueSheetAsset> OpenRequested { get; }
        IObservable<IReadOnlyList<CueSheetAsset>> ValidateAllRequested { get; }
        void RequestOpen(CueSheetAsset asset);
        void RequestValidateAll();
    }
}
