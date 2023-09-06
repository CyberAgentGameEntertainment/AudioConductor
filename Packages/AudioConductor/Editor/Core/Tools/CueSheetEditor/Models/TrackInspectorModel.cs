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
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;
using UnityEngine.Assertions;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class TrackInspectorModel : ITrackInspectorModel
    {
        private readonly string _tag;

        private readonly ItemTrack[] _items;
        private readonly HashSet<int> _itemIds;
        private readonly Track[] _target;
        private readonly AutoIncrementHistory _history;
        private readonly IAssetSaveService _assetSaveService;

        private readonly ObservableProperty<MixedValue<string>> _name;
        private readonly ObservableProperty<MixedValue<string>> _color;
        private readonly ObservableProperty<MixedValue<AudioClip>> _audioClip;
        private readonly ObservableProperty<MixedValue<float>> _volume;
        private readonly ObservableProperty<MixedValue<float>> _volumeRange;
        private readonly ObservableProperty<MixedValue<float>> _pitch;
        private readonly ObservableProperty<MixedValue<float>> _pitchRange;
        private readonly ObservableProperty<MixedValue<bool>> _pitchInvert;
        private readonly ObservableProperty<MixedValue<int>> _startSample;
        private readonly ObservableProperty<MixedValue<int>> _endSample;
        private readonly ObservableProperty<MixedValue<int>> _loopStartSample;
        private readonly ObservableProperty<MixedValue<bool>> _isLoop;
        private readonly ObservableProperty<MixedValue<int>> _randomWeight;
        private readonly ObservableProperty<MixedValue<int>> _priority;
        private readonly ObservableProperty<MixedValue<float>> _fadeTime;

        private readonly TrackPreviewModel _trackPreviewModel;

        public TrackInspectorModel([NotNull] ItemTrack[] items,
                                   [NotNull] AutoIncrementHistory history,
                                   [NotNull] IAssetSaveService assetSaveService)
        {
            Assert.IsTrue(items.Length > 0);

            _tag = $"[SelectionTrack:{IdentifierFactory.Create()}]";

            _items = items;
            _itemIds = new HashSet<int>(_items.Length);
            _target = new Track[_items.Length];
            for (var i = 0; i < _items.Length; i++)
            {
                var item = _items[i];
                _itemIds.Add(item.id);
                _target[i] = item.RawData;
            }

            _history = history;
            _assetSaveService = assetSaveService;

            var mixed = new bool[15];
            foreach (var track in _target)
            {
                mixed[0] |= Name != track.name;
                mixed[1] |= Color != track.colorId;
                mixed[2] |= AudioClip != track.audioClip;
                mixed[3] |= !Mathf.Approximately(Volume, track.volume);
                mixed[4] |= !Mathf.Approximately(VolumeRange, track.volumeRange);
                mixed[5] |= !Mathf.Approximately(Pitch, track.pitch);
                mixed[6] |= !Mathf.Approximately(PitchRange, track.pitchRange);
                mixed[7] |= PitchInvert != track.pitchInvert;
                mixed[8] |= StartSample != track.startSample;
                mixed[9] |= EndSample != track.endSample;
                mixed[10] |= LoopStartSample != track.loopStartSample;
                mixed[11] |= IsLoop != track.isLoop;
                mixed[12] |= RandomWeight != track.randomWeight;
                mixed[13] |= Priority != track.priority;
                mixed[14] |= !Mathf.Approximately(FadeTime, track.fadeTime);
            }

            // ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
            _name = new(new(Name, mixed[0]));
            _color = new(new(Color, mixed[1]));
            _audioClip = new(new(AudioClip, mixed[2]));
            _volume = new(new(Volume, mixed[3]));
            _volumeRange = new(new(VolumeRange, mixed[4]));
            _pitch = new(new(Pitch, mixed[5]));
            _pitchRange = new(new(PitchRange, mixed[6]));
            _pitchInvert = new(new(PitchInvert, mixed[7]));
            _startSample = new(new(StartSample, mixed[8]));
            _endSample = new(new(EndSample, mixed[9]));
            _loopStartSample = new(new(LoopStartSample, mixed[10]));
            _isLoop = new(new(IsLoop, mixed[11]));
            _randomWeight = new(new(RandomWeight, mixed[12]));
            _priority = new(new(Priority, mixed[13]));
            _fadeTime = new(new(FadeTime, mixed[14]));
            // ReSharper enable ArrangeObjectCreationWhenTypeNotEvident

            if (CanPreview)
                _trackPreviewModel = new TrackPreviewModel(_items[0]);
        }

        private readonly struct AudioClipAndDependencyValueBackup
        {
            public readonly AudioClip audioClip;
            public readonly int startSample;
            public readonly int endSample;
            public readonly int loopStartSample;

            public AudioClipAndDependencyValueBackup(Track track)
            {
                audioClip = track.audioClip;
                startSample = track.startSample;
                endSample = track.endSample;
                loopStartSample = track.loopStartSample;
            }
        }

        private readonly struct LoopInfoBackup
        {
            public readonly bool isLoop;
            public readonly int endSample;
            public readonly int loopStartSample;

            public LoopInfoBackup(Track track)
            {
                isLoop = track.isLoop;
                endSample = track.endSample;
                loopStartSample = track.loopStartSample;
            }
        }

        private Track TypicalTarget => _target[0];

        public bool CanPreview => _items.Length == 1;

        #region Name

        public string Name
        {
            get => TypicalTarget.name;
            set
            {
                var old = _target.Select(track => track.name).ToArray();
                _history.Register($"{_tag} Set Track {nameof(Name)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.name = value;
                    _name.Value = new MixedValue<string>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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
                var old = _target.Select(track => track.colorId).ToArray();
                _history.Register($"{_tag} Set Track {nameof(Color)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.colorId = value;
                    _color.Value = new MixedValue<string>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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

        #region AudioClip

        public AudioClip AudioClip
        {
            get => TypicalTarget.audioClip;
            set
            {
                var actionTypeId = value == null
                    ? $"{_tag} Unset Track {nameof(AudioClip)} {AudioClip.GetInstanceID()}"
                    : $"{_tag} Set Track {nameof(AudioClip)} {value.GetInstanceID()}";

                var old = _target.Select(track => new AudioClipAndDependencyValueBackup(track)).ToArray();
                _history.Register(actionTypeId, Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    var samples = value == null ? 0 : value.samples;
                    foreach (var track in _target)
                    {
                        track.audioClip = value;
                        track.startSample = 0;
                        track.endSample = samples;
                        track.loopStartSample = ValueRangeConst.LoopStartSample.Clamp(track.loopStartSample, samples);
                    }

                    _audioClip.Value = new MixedValue<AudioClip>(value, false);
                    _startSample.Value = new MixedValue<int>(StartSample, false);
                    _endSample.Value = new MixedValue<int>(EndSample, false);
                    _loopStartSample.Value = new MixedValue<int>(LoopStartSample, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = new bool[4];
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].audioClip = old[i].audioClip;
                        mixed[0] |= AudioClip != _target[i].audioClip;
                        _target[i].startSample = old[i].startSample;
                        mixed[1] |= StartSample != _target[i].startSample;
                        _target[i].endSample = old[i].endSample;
                        mixed[2] |= EndSample != _target[i].endSample;
                        _target[i].loopStartSample = old[i].loopStartSample;
                        mixed[3] |= LoopStartSample != _target[i].loopStartSample;
                    }

                    _audioClip.Value = new MixedValue<AudioClip>(AudioClip, mixed[0]);
                    _startSample.Value = new MixedValue<int>(StartSample, mixed[1]);
                    _endSample.Value = new MixedValue<int>(EndSample, mixed[2]);
                    _loopStartSample.Value = new MixedValue<int>(LoopStartSample, mixed[3]);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<AudioClip>> AudioClipObservable => _audioClip;

        #endregion

        #region Volume

        public float Volume
        {
            get => TypicalTarget.volume;
            set
            {
                value = ValueRangeConst.Volume.Clamp(value);
                var old = _target.Select(track => track.volume).ToArray();
                _history.Register($"{_tag} Set Track {nameof(Volume)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.volume = value;
                    _volume.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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
                var old = _target.Select(track => track.volumeRange).ToArray();
                _history.Register($"{_tag} Set Track {nameof(VolumeRange)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.volumeRange = value;
                    _volumeRange.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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
                var old = _target.Select(track => track.pitch).ToArray();
                _history.Register($"{_tag} Set Track {nameof(Pitch)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.pitch = value;
                    _pitch.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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
                var old = _target.Select(track => track.pitchRange).ToArray();
                _history.Register($"{_tag} Set Track {nameof(PitchRange)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.pitchRange = value;
                    _pitchRange.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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
                var old = _target.Select(track => track.pitchInvert).ToArray();
                _history.Register($"{_tag} Set CueSheet {nameof(PitchInvert)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.pitchInvert = value;
                    _pitchInvert.Value = new MixedValue<bool>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
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

        #region StartSample

        public int StartSample
        {
            get => TypicalTarget.startSample;
            set
            {
                var old = _target.Select(track => track.startSample).ToArray();
                _history.Register($"{_tag} Set Track {nameof(StartSample)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    var mixed = false;
                    foreach (var track in _target)
                    {
                        var samples = track.audioClip == null ? 0 : track.audioClip.samples;
                        track.startSample = ValueRangeConst.StartSample.Clamp(value, samples);
                        mixed |= StartSample != track.startSample;
                    }

                    _startSample.Value = new MixedValue<int>(StartSample, mixed);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].startSample = old[i];
                        mixed |= StartSample != _target[i].startSample;
                    }

                    _startSample.Value = new MixedValue<int>(StartSample, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> StartSampleObservable => _startSample;

        #endregion

        #region EndSample

        public int EndSample
        {
            get => TypicalTarget.endSample;
            set
            {
                var old = _target.Select(track => track.endSample).ToArray();
                _history.Register($"{_tag} Set Track {nameof(EndSample)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    var mixed = false;
                    foreach (var track in _target)
                    {
                        var samples = track.audioClip == null ? 0 : track.audioClip.samples;
                        track.endSample = Mathf.Clamp(value, 0, samples);
                        mixed |= EndSample != track.endSample;
                    }

                    _endSample.Value = new MixedValue<int>(EndSample, mixed);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].endSample = old[i];
                        mixed |= EndSample != _target[i].endSample;
                    }

                    _endSample.Value = new MixedValue<int>(EndSample, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> EndSampleObservable => _endSample;

        #endregion

        #region LoopStartSample

        public int LoopStartSample
        {
            get => TypicalTarget.loopStartSample;
            set
            {
                var old = _target.Select(track => track.loopStartSample).ToArray();
                _history.Register($"{_tag} Set Track {nameof(LoopStartSample)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    var mixed = false;
                    foreach (var track in _target)
                    {
                        var samples = track.audioClip == null ? 0 : track.audioClip.samples;
                        track.loopStartSample = ValueRangeConst.LoopStartSample.Clamp(value, samples);
                        mixed |= LoopStartSample != track.loopStartSample;
                    }

                    _loopStartSample.Value = new MixedValue<int>(LoopStartSample, mixed);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].loopStartSample = old[i];
                        mixed |= LoopStartSample != _target[i].loopStartSample;
                    }

                    _loopStartSample.Value = new MixedValue<int>(LoopStartSample, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> LoopStartSampleObservable => _loopStartSample;

        #endregion

        #region IsLoop

        public bool IsLoop
        {
            get => TypicalTarget.isLoop;
            set
            {
                var old = _target.Select(track => track.isLoop).ToArray();
                _history.Register($"{_tag} Set Track {nameof(IsLoop)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.isLoop = value;
                    _isLoop.Value = new MixedValue<bool>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].isLoop = old[i];
                        mixed |= IsLoop != _target[i].isLoop;
                    }

                    _isLoop.Value = new MixedValue<bool>(IsLoop, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<bool>> IsLoopObservable => _isLoop;

        #endregion

        #region RandomWeight

        public int RandomWeight
        {
            get => TypicalTarget.randomWeight;
            set
            {
                value = ValueRangeConst.RandomWeight.Clamp(value);
                var old = _target.Select(track => track.randomWeight).ToArray();
                _history.Register($"{_tag} Set Track {nameof(RandomWeight)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.randomWeight = value;
                    _randomWeight.Value = new MixedValue<int>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].randomWeight = old[i];
                        mixed |= RandomWeight != _target[i].randomWeight;
                    }

                    _randomWeight.Value = new MixedValue<int>(RandomWeight, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> RandomWeightObservable => _randomWeight;

        #endregion

        #region Priority

        public int Priority
        {
            get => TypicalTarget.priority;
            set
            {
                var old = _target.Select(track => track.priority).ToArray();
                _history.Register($"{_tag} Set Track {nameof(Priority)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.priority = value;
                    _priority.Value = new MixedValue<int>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].priority = old[i];
                        mixed |= Priority != _target[i].priority;
                    }

                    _priority.Value = new MixedValue<int>(Priority, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<int>> PriorityObservable => _priority;

        #endregion

        #region FadeTime

        public float FadeTime
        {
            get => TypicalTarget.fadeTime;
            set
            {
                value = ValueRangeConst.FadeTime.Clamp(value);
                var old = _target.Select(track => track.fadeTime).ToArray();
                _history.Register($"{_tag} Set Track {nameof(FadeTime)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    foreach (var track in _target)
                        track.fadeTime = value;
                    _fadeTime.Value = new MixedValue<float>(value, false);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    var mixed = false;
                    for (var i = 0; i < _target.Length; ++i)
                    {
                        _target[i].fadeTime = old[i];
                        mixed |= !Mathf.Approximately(FadeTime, _target[i].fadeTime);
                    }

                    _fadeTime.Value = new MixedValue<float>(FadeTime, mixed);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<MixedValue<float>> FadeTimeObservable => _fadeTime;

        #endregion

        public void AnalyzeWaveChunk()
        {
            var old = _target.Select(track => new LoopInfoBackup(track)).ToArray();
            _history.Register($"{_tag} Analyze Wave Chunk", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                var mixed = new bool[3];
                foreach (var track in _target)
                {
                    Apply(track);
                    mixed[0] |= IsLoop != track.isLoop;
                    mixed[1] |= EndSample != track.endSample;
                    mixed[2] |= LoopStartSample != track.loopStartSample;
                }

                _isLoop.Value = new MixedValue<bool>(IsLoop, mixed[0]);
                _endSample.Value = new MixedValue<int>(EndSample, mixed[1]);
                _loopStartSample.Value = new MixedValue<int>(LoopStartSample, mixed[2]);
                _assetSaveService.Save();

                #region LocalMethods

                void Apply(Track track)
                {
                    var waveChunkReader = new WaveChunkReader.WaveChunkReader();
                    try
                    {
                        waveChunkReader.Execute(track.audioClip);
                    }
                    catch (Exception)
                    {
                        Debug.LogWarning($"{track.name} hasn't Loop information.");
                        return;
                    }

                    if (waveChunkReader.HasLoop() == false)
                        return;

                    var loopInfoList = waveChunkReader.LoopInfoList;

                    track.isLoop = true;
                    track.endSample = (int)loopInfoList[0].end;
                    track.loopStartSample = (int)loopInfoList[0].start;
                }

                #endregion
            }

            void Undo()
            {
                var mixed = new bool[3];
                for (var i = 0; i < _target.Length; ++i)
                {
                    _target[i].isLoop = old[i].isLoop;
                    mixed[0] |= IsLoop != _target[i].isLoop;
                    _target[i].endSample = old[i].endSample;
                    mixed[1] |= EndSample != _target[i].endSample;
                    _target[i].loopStartSample = old[i].loopStartSample;
                    mixed[2] |= LoopStartSample != _target[i].loopStartSample;
                }

                _isLoop.Value = new MixedValue<bool>(IsLoop, mixed[0]);
                _endSample.Value = new MixedValue<int>(EndSample, mixed[1]);
                _loopStartSample.Value = new MixedValue<int>(LoopStartSample, mixed[2]);
                _assetSaveService.Save();
            }

            #endregion
        }

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
                case CueListTreeView.ColumnType.Volume:
                    Volume = (float)value;
                    break;
                case CueListTreeView.ColumnType.VolumeRange:
                    VolumeRange = (float)value;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(columnType), columnType, null);
            }
        }

        public TrackPreviewController PlayClip(int? sample)
            => _trackPreviewModel?.Play(sample);
    }
}
