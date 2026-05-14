// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if UNITY_6000_2_OR_NEWER
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
#else
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem;
#endif
using System;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Foundation.TinyRx;

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
