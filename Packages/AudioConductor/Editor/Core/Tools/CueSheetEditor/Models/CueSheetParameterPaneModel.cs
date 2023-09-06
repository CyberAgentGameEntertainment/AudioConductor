// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using JetBrains.Annotations;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueSheetParameterPaneModel : ICueSheetParameterPaneModel
    {
        private readonly ObservableCueSheet _target;
        private readonly AutoIncrementHistory _history;
        private readonly IAssetSaveService _assetSaveService;

        public CueSheetParameterPaneModel([NotNull] CueSheet cueSheet,
                                          [NotNull] AutoIncrementHistory history,
                                          [NotNull] IAssetSaveService assetSaveService)
        {
            _target = new ObservableCueSheet(cueSheet);
            _history = history;
            _assetSaveService = assetSaveService;
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
