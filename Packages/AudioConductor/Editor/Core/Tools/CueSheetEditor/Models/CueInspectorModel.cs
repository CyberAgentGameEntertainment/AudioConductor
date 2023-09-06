// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Views;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;
using UnityEngine.Assertions;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueInspectorModel : ICueInspectorModel
    {
        private readonly string _tag;

        private readonly ItemCue[] _items;
        private readonly HashSet<int> _itemIds;
        private readonly Cue[] _target;
        private readonly AutoIncrementHistory _history;
        private readonly IAssetSaveService _assetSaveService;

        private readonly ObservableProperty<MixedValue<string>> _name;
        private readonly ObservableProperty<MixedValue<string>> _color;
        private readonly ObservableProperty<MixedValue<int>> _categoryId;
        private readonly ObservableProperty<MixedValue<ThrottleType>> _throttleType;
        private readonly ObservableProperty<MixedValue<int>> _throttleLimit;
        private readonly ObservableProperty<MixedValue<float>> _volume;
        private readonly ObservableProperty<MixedValue<float>> _volumeRange;
        private readonly ObservableProperty<MixedValue<float>> _pitch;
        private readonly ObservableProperty<MixedValue<float>> _pitchRange;
        private readonly ObservableProperty<MixedValue<bool>> _pitchInvert;
        private readonly ObservableProperty<MixedValue<CuePlayType>> _playType;

        private readonly CuePreviewModel _trackPreviewModel;

        public CueInspectorModel([NotNull] ItemCue[] items,
                                 [NotNull] AutoIncrementHistory history,
                                 [NotNull] IAssetSaveService assetSaveService)
        {
            Assert.IsTrue(items.Length > 0);

            _tag = $"[SelectionCue:{IdentifierFactory.Create()}]";

            _items = items;
            _itemIds = new HashSet<int>(_items.Length);
            _target = new Cue[_items.Length];
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                _itemIds.Add(item.id);
                _target[i] = item.RawData;
            }

            _history = history;
            _assetSaveService = assetSaveService;

            var mixed = new bool[11];
            foreach (var cue in _target)
            {
                mixed[0] |= Name != cue.name;
                mixed[1] |= Color != cue.colorId;
                mixed[2] |= CategoryId != cue.categoryId;
                mixed[3] |= ThrottleType != cue.throttleType;
                mixed[4] |= ThrottleLimit != cue.throttleLimit;
                mixed[5] |= !Mathf.Approximately(Volume, cue.volume);
                mixed[6] |= !Mathf.Approximately(VolumeRange, cue.volumeRange);
                mixed[7] |= !Mathf.Approximately(Pitch, cue.pitch);
                mixed[8] |= !Mathf.Approximately(PitchRange, cue.pitchRange);
                mixed[9] |= PitchInvert != cue.pitchInvert;
                mixed[10] |= PlayType != cue.playType;
            }

            // ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
            _name = new(new(Name, mixed[0]));
            _color = new(new(Color, mixed[1]));
            _categoryId = new(new(CategoryId, mixed[2]));
            _throttleType = new(new(ThrottleType, mixed[3]));
            _throttleLimit = new(new(ThrottleLimit, mixed[4]));
            _volume = new(new(Volume, mixed[5]));
            _volumeRange = new(new(VolumeRange, mixed[6]));
            _pitch = new(new(Pitch, mixed[7]));
            _pitchRange = new(new(PitchRange, mixed[8]));
            _pitchInvert = new(new(PitchInvert, mixed[9]));
            _playType = new(new(PlayType, mixed[10]));
            // ReSharper enable ArrangeObjectCreationWhenTypeNotEvident

            if (CanPreview)
                _trackPreviewModel = new CuePreviewModel(_items[0]);
        }

        private Cue TypicalTarget => _target[0];

        public bool CanPreview => _items.Length == 1;

        #region Name

        public string Name
        {
            get => TypicalTarget.name;
            set
            {
                var old = _target.Select(cue => cue.name).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(Name)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.name = value;
                    _name.Value = new MixedValue<string>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].name = old[i];
                        mixed |= Name != _target[i].name;
                    }

                    _name.Value = new MixedValue<string>(Name, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<string>> NameObservable => _name;

        #endregion

        #region Color

        public string Color
        {
            get => TypicalTarget.colorId;
            set
            {
                var old = _target.Select(cue => cue.colorId).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(Color)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.colorId = value;
                    _color.Value = new MixedValue<string>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].colorId = old[i];
                        mixed |= Color != _target[i].colorId;
                    }

                    _color.Value = new MixedValue<string>(Color, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<string>> ColorObservable => _color;

        #endregion

        #region CategoryId

        public int CategoryId
        {
            get => TypicalTarget.categoryId;
            set
            {
                var old = _target.Select(cue => cue.categoryId).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(CategoryId)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.categoryId = value;
                    _categoryId.Value = new MixedValue<int>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].categoryId = old[i];
                        mixed |= CategoryId != _target[i].categoryId;
                    }

                    _categoryId.Value = new MixedValue<int>(CategoryId, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> CategoryIdObservable => _categoryId;

        #endregion

        #region ThrottleType

        public ThrottleType ThrottleType
        {
            get => TypicalTarget.throttleType;
            set
            {
                var old = _target.Select(cue => cue.throttleType).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(ThrottleType)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.throttleType = value;
                    _throttleType.Value = new MixedValue<ThrottleType>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].throttleType = old[i];
                        mixed |= ThrottleType != _target[i].throttleType;
                    }

                    _throttleType.Value = new MixedValue<ThrottleType>(ThrottleType, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<ThrottleType>> ThrottleTypeObservable => _throttleType;

        #endregion

        #region ThrottleLimit

        public int ThrottleLimit
        {
            get => TypicalTarget.throttleLimit;
            set
            {
                var old = _target.Select(cue => cue.throttleLimit).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(ThrottleLimit)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.throttleLimit = value;
                    _throttleLimit.Value = new MixedValue<int>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].throttleLimit = old[i];
                        mixed |= ThrottleLimit != _target[i].throttleLimit;
                    }

                    _throttleLimit.Value = new MixedValue<int>(ThrottleLimit, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> ThrottleLimitObservable => _throttleLimit;

        #endregion

        #region Volume

        public float Volume
        {
            get => TypicalTarget.volume;
            set
            {
                value = ValueRangeConst.Volume.Clamp(value);
                var old = _target.Select(cue => cue.volume).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(Volume)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.volume = value;
                    _volume.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].volume = old[i];
                        mixed |= !Mathf.Approximately(Volume, _target[i].volume);
                    }

                    _volume.Value = new MixedValue<float>(Volume, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<float>> VolumeObservable => _volume;

        #endregion

        #region VolumeRange

        public float VolumeRange
        {
            get => TypicalTarget.volumeRange;
            set
            {
                value = ValueRangeConst.VolumeRange.Clamp(value);
                var old = _target.Select(cue => cue.volumeRange).ToArray();
                _history.Register($"{_tag} Set Cue VolumeRange {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.volumeRange = value;
                    _volumeRange.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].volumeRange = old[i];
                        mixed |= !Mathf.Approximately(VolumeRange, _target[i].volumeRange);
                    }

                    _volumeRange.Value = new MixedValue<float>(VolumeRange, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<float>> VolumeRangeObservable => _volumeRange;

        #endregion

        #region Pitch

        public float Pitch
        {
            get => TypicalTarget.pitch;
            set
            {
                value = ValueRangeConst.Pitch.Clamp(value);
                var old = _target.Select(cue => cue.pitch).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(Pitch)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.pitch = value;
                    _pitch.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].pitch = old[i];
                        mixed |= !Mathf.Approximately(Pitch, _target[i].pitch);
                    }

                    _pitch.Value = new MixedValue<float>(Pitch, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<float>> PitchObservable => _pitch;

        #endregion

        #region PitchRange

        public float PitchRange
        {
            get => TypicalTarget.pitchRange;
            set
            {
                value = ValueRangeConst.PitchRange.Clamp(value);
                var old = _target.Select(cue => cue.pitchRange).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(PitchRange)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.pitchRange = value;
                    _pitchRange.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].pitchRange = old[i];
                        mixed |= !Mathf.Approximately(PitchRange, _target[i].pitchRange);
                    }

                    _pitchRange.Value = new MixedValue<float>(PitchRange, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<float>> PitchRangeObservable => _pitchRange;

        #endregion

        #region PitchInvert

        public bool PitchInvert
        {
            get => TypicalTarget.pitchInvert;
            set
            {
                var old = _target.Select(cue => cue.pitchInvert).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(PitchInvert)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.pitchInvert = value;
                    _pitchInvert.Value = new MixedValue<bool>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].pitchInvert = old[i];
                        mixed |= PitchInvert != _target[i].pitchInvert;
                    }

                    _pitchInvert.Value = new MixedValue<bool>(PitchInvert, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<bool>> PitchInvertObservable => _pitchInvert;

        #endregion

        #region PlayType

        public CuePlayType PlayType
        {
            get => TypicalTarget.playType;
            set
            {
                var old = _target.Select(cue => cue.playType).ToArray();
                _history.Register($"{_tag} Set Cue {nameof(CuePlayType)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var cue in _target)
                        cue.playType = value;
                    _playType.Value = new MixedValue<CuePlayType>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; i++)
                    {
                        _target[i].playType = old[i];
                        mixed |= PlayType != _target[i].playType;
                    }

                    _playType.Value = new MixedValue<CuePlayType>(PlayType, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<CuePlayType>> PlayTypeObservable => _playType;

        #endregion

        public bool Contains(int itemId) => _itemIds.Contains(itemId);

        public void ChangeValue(CueListTreeView.ColumnType columnType, object value)
        {
            switch (columnType)
            {
                case CueListTreeView.ColumnType.Name:
                    Name = (string)value;
                    break;
                case CueListTreeView.ColumnType.Color:
                    Color = (string)value;
                    break;
                case CueListTreeView.ColumnType.Category:
                    CategoryId = (int)value;
                    break;
                case CueListTreeView.ColumnType.ThrottleType:
                    ThrottleType = (ThrottleType)value;
                    break;
                case CueListTreeView.ColumnType.ThrottleLimit:
                    ThrottleLimit = (int)value;
                    break;
                case CueListTreeView.ColumnType.Volume:
                    Volume = (float)value;
                    break;
                case CueListTreeView.ColumnType.VolumeRange:
                    VolumeRange = (float)value;
                    break;
                case CueListTreeView.ColumnType.PlayType:
                    PlayType = (CuePlayType)value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }
        }

        public ICueController PlayCue()
            => _trackPreviewModel?.Play();

        public void StopCue()
            => _trackPreviewModel?.Stop();
    }
}
