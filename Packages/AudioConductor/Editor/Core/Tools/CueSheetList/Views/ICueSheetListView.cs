// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Core.Models;
using AudioConductor.Editor.Core.Tools.CueSheetList.Models;
using AudioConductor.Editor.Foundation.TinyRx;

namespace AudioConductor.Editor.Core.Tools.CueSheetList.Views
{
    internal interface ICueSheetListView : IDisposable
    {
        IObservable<string> SearchTextChangedAsObservable { get; }
        IObservable<CueSheetAsset> OpenRequestedAsObservable { get; }
        IObservable<Empty> ValidateAllClickedAsObservable { get; }
        void Setup();
        void RenderItems(CueSheetListItem[] items);
        void SetSearchText(string text);
    }
}
