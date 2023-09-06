// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Core.Tools.CueSheetEditor.Enums;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.DataTransferObjects;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;
using UnityEngine.Assertions;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed partial class CueListModel
    {
        private const string DefaultNewCueName = "New Cue";
        private const string DefaultNewTrackName = "New Track";

        public void MoveItem(ItemMoveOperationRequestedEvent evt)
        {
            if (evt.target.Type == ItemType.Cue)
                MoveCue(evt.oldIndex, evt.newIndex);
            else
                MoveTrack(evt.oldIndex, (ItemCue)evt.oldParent,
                          evt.newIndex, (ItemCue)evt.newParent,
                          (ItemTrack)evt.target);
        }

        public void AddCue(CueAddOperationRequestedEvent evt)
        {
            var newItem = new ItemCue(CreateNewId, new Cue
            {
                name = DefaultNewCueName
            });
            AddCue(_root.children.Count, newItem, $"Add Cue {Time.frameCount}");
        }

        public void AddTrack(TrackAddOperationRequestedEvent evt)
        {
            var index = evt.parent.children.Count;
            var newItem = new ItemTrack(CreateNewId, new Track
            {
                name = DefaultNewTrackName
            });
            AddTrack(index, evt.parent, newItem, $"Add Track {Time.frameCount}");
        }

        public void RemoveItem(ItemRemoveOperationRequestedEvent evt)
        {
            if (evt.target.Type == ItemType.Cue)
                RemoveCue(evt.index, (ItemCue)evt.target);
            else
                RemoveTrack(evt.index, (ItemTrack)evt.target);
        }

        public void DuplicateItem(ItemDuplicateOperationRequestedEvent evt)
        {
            if (evt.target.Type == ItemType.Cue)
                DuplicateCue(evt.insertIndex, (ItemCue)evt.target);
            else
                DuplicateTrack(evt.insertIndex, (ItemCue)evt.parent, (ItemTrack)evt.target);
        }

        public void AddAsset(AssetAddOperationRequestedEvent evt)
        {
            var actionTypeId = $"Add Asset {Time.frameCount}";
            var index = evt.insertIndex;

            var parent = evt.parent;
            if (parent.Type == ItemType.CueSheet)
            {
                var parentCue = new ItemCue(CreateNewId, new Cue
                {
                    name = evt.asset.name
                });
                AddCue(index, parentCue, actionTypeId);
                parent = parentCue;
                index = 0;
            }

            var newItem = new ItemTrack(CreateNewId, new Track
            {
                name = evt.asset.name,
                audioClip = evt.asset,
                endSample = evt.asset.samples
            });

            AddTrack(index, (ItemCue)parent, newItem, actionTypeId);
        }

        public void ChangeValue(ColumnValueChangedEvent evt)
        {
            // If any of the items in the selection, apply to all of them.
            if (_latestInspectorModel?.Contains(evt.itemId) ?? false)
            {
                _latestInspectorModel.ChangeValue(evt.columnType, evt.newValue);
                return;
            }

            // If not one of the items in the selection, apply to it.
            var item = _itemTable[evt.itemId];
            Assert.IsTrue(item.Type != ItemType.CueSheet);

            if (item.Type == ItemType.Cue)
                ChangeCueValue((ItemCue)item, evt.columnType, evt.newValue);
            else
                ChangeTrackValue((ItemTrack)item, evt.columnType, evt.newValue);
        }

        private void MoveCue(int oldIndex, int newIndex)
        {
            _history.Register($"Move Cue {Time.frameCount}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                _root.MoveChild(oldIndex, newIndex);
                _moveSubject.OnNext(Empty.Default);
                _assetSaveService.Save();
            }

            void Undo()
            {
                _root.MoveChild(newIndex, oldIndex);
                _moveSubject.OnNext(Empty.Default);
                _assetSaveService.Save();
            }

            #endregion
        }

        private void MoveTrack(int oldIndex, ItemCue oldParent, int newIndex, ItemCue newParent, ItemTrack target)
        {
            _history.Register($"Move Cue {Time.frameCount}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                oldParent.RemoveChild(oldIndex);
                newParent.InsertChild(newIndex, target);
                _moveSubject.OnNext(Empty.Default);
                _assetSaveService.Save();
            }

            void Undo()
            {
                newParent.RemoveChild(newIndex);
                oldParent.InsertChild(oldIndex, target);
                _moveSubject.OnNext(Empty.Default);
                _assetSaveService.Save();
            }

            #endregion
        }

        private void AddCue(int index, ItemCue newItem, string actionTypeId)
        {
            _history.Register(actionTypeId, Redo, Undo);

            #region MyRegion

            void Redo()
            {
                _itemTable.Add(newItem.id, newItem);
                if (newItem.hasChildren)
                    foreach (var newItemChild in newItem.children)
                        _itemTable.Add(newItemChild.id, (CueListItem)newItemChild);
                _root.InsertChild(index, newItem);
                _addSubject.OnNext(newItem);
                _assetSaveService.Save();
            }

            void Undo()
            {
                _itemTable.Remove(newItem.id);
                if (newItem.hasChildren)
                    foreach (var newItemChild in newItem.children)
                        _itemTable.Remove(newItemChild.id);
                _root.RemoveChild(index);
                _removeSubject.OnNext(newItem);
                _assetSaveService.Save();
            }

            #endregion
        }

        private void AddTrack(int index, ItemCue parent, ItemTrack newItem, string actionTypeId)
        {
            _history.Register(actionTypeId, Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                _itemTable.Add(newItem.id, newItem);
                parent.InsertChild(index, newItem);
                _addSubject.OnNext(newItem);
                _assetSaveService.Save();
            }

            void Undo()
            {
                _itemTable.Remove(newItem.id);
                parent.RemoveChild(index);
                _removeSubject.OnNext(newItem);
                _assetSaveService.Save();
            }

            #endregion
        }

        private void RemoveCue(int index, ItemCue cue)
        {
            _history.Register($"Remove Cue {Time.frameCount}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                _itemTable.Remove(cue.id);
                _root.RemoveChild(index);
                _removeSubject.OnNext(cue);
                _assetSaveService.Save();
            }

            void Undo()
            {
                _itemTable.Add(cue.id, cue);
                _root.InsertChild(index, cue);
                _addSubject.OnNext(cue);
                _assetSaveService.Save();
            }

            #endregion
        }

        private void RemoveTrack(int index, ItemTrack track)
        {
            var parent = (ItemCue)track.parent;

            _history.Register($"Remove Track {Time.frameCount}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                _itemTable.Remove(track.id);
                parent.RemoveChild(index);
                _removeSubject.OnNext(track);
                _assetSaveService.Save();
            }

            void Undo()
            {
                _itemTable.Add(track.id, track);
                parent.InsertChild(index, track);
                _addSubject.OnNext(track);
                _assetSaveService.Save();
            }

            #endregion
        }

        private void DuplicateCue(int index, ItemCue cue)
        {
            var actionTypeId = $"Duplicate Cue {Time.frameCount}";
            var newCue = cue.RawData.Duplicate();
            var newItem = new ItemCue(CreateNewId, newCue);
            foreach (var track in newCue.trackList)
                newItem.AddChild(new ItemTrack(CreateNewId, track));
            AddCue(index, newItem, actionTypeId);
        }

        private void DuplicateTrack(int index, ItemCue parent, ItemTrack track)
        {
            var newItem = new ItemTrack(CreateNewId, track.RawData.Duplicate());
            AddTrack(index, parent, newItem, $"Duplicate Track {Time.frameCount}");
        }

        private void ChangeCueValue(ItemCue item, CueListTreeView.ColumnType columnType, object newValue)
        {
            switch (columnType)
            {
                case CueListTreeView.ColumnType.Name:
                    ChangeCueName(item, (string)newValue);
                    break;
                case CueListTreeView.ColumnType.Color:
                    ChangeCueColor(item, (string)newValue);
                    break;
                case CueListTreeView.ColumnType.Category:
                    ChangeCueCategory(item, (int)newValue);
                    break;
                case CueListTreeView.ColumnType.ThrottleType:
                    ChangeCueThrottleType(item, (ThrottleType)newValue);
                    break;
                case CueListTreeView.ColumnType.ThrottleLimit:
                    ChangeCueThrottleLimit(item, (int)newValue);
                    break;
                case CueListTreeView.ColumnType.Volume:
                    ChangeCueVolume(item, (float)newValue);
                    break;
                case CueListTreeView.ColumnType.VolumeRange:
                    ChangeCueVolumeRange(item, (float)newValue);
                    break;
                case CueListTreeView.ColumnType.PlayType:
                    ChangeCuePlayType(item, (CuePlayType)newValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }
        }

        private void ChangeCueName(ItemCue item, string newValue)
        {
            var oldValue = item.RawData.name;

            _history.Register($"Change Cue Name {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.name = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.name = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCueColor(ItemCue item, string newValue)
        {
            var oldValue = item.RawData.colorId;

            _history.Register($"Change Cue Color {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.colorId = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.colorId = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCueCategory(ItemCue item, int newValue)
        {
            var oldValue = item.RawData.categoryId;

            _history.Register($"Change Cue Category {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.categoryId = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.categoryId = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCueThrottleType(ItemCue item, ThrottleType newValue)
        {
            var oldValue = item.RawData.throttleType;

            _history.Register($"Change Cue ThrottleType {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.throttleType = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.throttleType = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCueThrottleLimit(ItemCue item, int newValue)
        {
            newValue = ValueRangeConst.ThrottleLimit.Clamp(newValue);
            var oldValue = item.RawData.throttleLimit;

            _history.Register($"Change Cue ThrottleLimit {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.throttleLimit = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.throttleLimit = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCueVolume(ItemCue item, float newValue)
        {
            newValue = ValueRangeConst.Volume.Clamp(newValue);
            var oldValue = item.RawData.volume;

            _history.Register($"Change Cue Volume {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.volume = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.volume = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCueVolumeRange(ItemCue item, float newValue)
        {
            newValue = ValueRangeConst.VolumeRange.Clamp(newValue);
            var oldValue = item.RawData.volumeRange;

            _history.Register($"Change Cue VolumeRange {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.volumeRange = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.volumeRange = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeCuePlayType(ItemCue item, CuePlayType newValue)
        {
            var oldValue = item.RawData.playType;

            _history.Register($"Change Cue PlayType {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.playType = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.playType = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeTrackValue(ItemTrack item, CueListTreeView.ColumnType columnType, object newValue)
        {
            switch (columnType)
            {
                case CueListTreeView.ColumnType.Name:
                    ChangeTrackName(item, (string)newValue);
                    break;
                case CueListTreeView.ColumnType.Color:
                    ChangeTrackColor(item, (string)newValue);
                    break;
                case CueListTreeView.ColumnType.Volume:
                    ChangeTrackVolume(item, (float)newValue);
                    break;
                case CueListTreeView.ColumnType.VolumeRange:
                    ChangeTrackVolumeRange(item, (float)newValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }
        }

        private void ChangeTrackName(ItemTrack item, string newValue)
        {
            var oldValue = item.RawData.name;

            _history.Register($"Change Track Name {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.name = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.name = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeTrackColor(ItemTrack item, string newValue)
        {
            var oldValue = item.RawData.colorId;

            _history.Register($"Change Track Color {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.colorId = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.colorId = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeTrackVolume(ItemTrack item, float newValue)
        {
            newValue = ValueRangeConst.Volume.Clamp(newValue);
            var oldValue = item.RawData.volume;

            _history.Register($"Change Track Volume {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.volume = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.volume = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }

        private void ChangeTrackVolumeRange(ItemTrack item, float newValue)
        {
            newValue = ValueRangeConst.VolumeRange.Clamp(newValue);
            var oldValue = item.RawData.volumeRange;

            _history.Register($"Change Track VolumeRange {item.id}:{newValue}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                item.RawData.volumeRange = newValue;
                _assetSaveService.Save();
            }

            void Undo()
            {
                item.RawData.volumeRange = oldValue;
                _assetSaveService.Save();
            }

            #endregion
        }
    }
}
