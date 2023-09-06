// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;
using UnityEditor.IMGUI.Controls;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces
{
    internal interface ICueListModel
    {
        TreeViewItem Root { get; }

        CueListTreeView.State CueListTreeViewState { get; }

        IObservable<Empty> MoveAsObservable { get; }

        IObservable<CueListItem> AddAsObservable { get; }

        IObservable<CueListItem> RemoveAsObservable { get; }

        IInspectorModel CreateInspectorModel(CueListItem[] items);

        void MoveItem(ItemMoveOperationRequestedEvent evt);

        void AddCue(CueAddOperationRequestedEvent evt);

        void AddTrack(TrackAddOperationRequestedEvent evt);

        void RemoveItem(ItemRemoveOperationRequestedEvent evt);

        void DuplicateItem(ItemDuplicateOperationRequestedEvent evt);

        void AddAsset(AssetAddOperationRequestedEvent evt);

        void ChangeValue(ColumnValueChangedEvent evt);
    }
}
