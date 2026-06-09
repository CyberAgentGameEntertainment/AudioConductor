// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Linq;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using JetBrains.Annotations;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueSheetParameterPaneModel : ICueSheetParameterPaneModel
    {
        private readonly IAssetSaveService _assetSaveService;
        private readonly AutoIncrementHistory _history;
        private readonly CueSheet _rawCueSheet;
        private readonly ObservableCueSheet _target;

        public CueSheetParameterPaneModel([NotNull] CueSheet cueSheet,
            [NotNull] AutoIncrementHistory history,
            [NotNull] IAssetSaveService assetSaveService)
        {
            _rawCueSheet = cueSheet;
            _target = new ObservableCueSheet(cueSheet);
            _history = history;
            _assetSaveService = assetSaveService;
        }

        public IReadOnlyObservableProperty<int> ReferenceSampleRateObservable => _target.ReferenceSampleRateObservable;

        public bool CanApplyReferenceSampleRate => CollectClipFrequencies().Count == 1;

        public void ApplyReferenceSampleRate()
        {
            var frequencies = CollectClipFrequencies();
            if (frequencies.Count != 1)
                return;
            var frequency = frequencies.First();
            var old = _target.ReferenceSampleRate;
            _history.Register($"Set CueSheet {nameof(CueSheet.referenceSampleRate)} {frequency}", Redo, Undo);

            #region LocalMethods

            void Redo()
            {
                _target.ReferenceSampleRate = frequency;
                _assetSaveService.Save();
            }

            void Undo()
            {
                _target.ReferenceSampleRate = old;
                _assetSaveService.Save();
            }

            #endregion
        }

        private HashSet<int> CollectClipFrequencies()
        {
            var frequencies = new HashSet<int>();
            foreach (var cue in _rawCueSheet.cueList)
            foreach (var track in cue.trackList)
                if (track.audioClip != null)
                    frequencies.Add(track.audioClip.frequency);
            return frequencies;
        }

        #region Name

        public string Name
        {
            get => _target.Name;
            set
            {
                var old = _target.Name;
                _history.Register($"Set CueSheet {nameof(Name)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _target.Name = value;
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _target.Name = old;
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<string> NameObservable => _target.NameObservable;

        #endregion

        #region ThrottleType

        public ThrottleType ThrottleType
        {
            get => _target.ThrottleType;
            set
            {
                var old = _target.ThrottleType;
                _history.Register($"Set CueSheet {nameof(ThrottleType)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _target.ThrottleType = value;
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _target.ThrottleType = old;
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<ThrottleType> ThrottleTypeObservable => _target.ThrottleTypeObservable;

        #endregion

        #region ThrottleLimit

        public int ThrottleLimit
        {
            get => _target.ThrottleLimit;
            set
            {
                value = ValueRangeConst.ThrottleLimit.Clamp(value);
                var old = _target.ThrottleLimit;
                _history.Register($"Set CueSheet {nameof(ThrottleLimit)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _target.ThrottleLimit = value;
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _target.ThrottleLimit = old;
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<int> ThrottleLimitObservable => _target.ThrottleLimitObservable;

        #endregion

        #region Volume

        public float Volume
        {
            get => _target.Volume;
            set
            {
                value = ValueRangeConst.Volume.Clamp(value);
                var old = _target.Volume;
                _history.Register($"Set CueSheet {nameof(Volume)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _target.Volume = value;
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _target.Volume = old;
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<float> VolumeObservable => _target.VolumeObservable;

        #endregion

        #region Pitch

        public float Pitch
        {
            get => _target.Pitch;
            set
            {
                value = ValueRangeConst.Pitch.Clamp(value);
                var old = _target.Pitch;
                _history.Register($"Set CueSheet {nameof(Pitch)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _target.Pitch = value;
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _target.Pitch = old;
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<float> PitchObservable => _target.PitchObservable;

        #endregion

        #region PitchInvert

        public bool PitchInvert
        {
            get => _target.PitchInvert;
            set
            {
                var old = _target.PitchInvert;
                _history.Register($"Set CueSheet PitchInvert {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _target.PitchInvert = value;
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _target.PitchInvert = old;
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<bool> PitchInvertObservable => _target.PitchInvertObservable;

        #endregion
    }
}
