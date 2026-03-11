// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;
using AudioConductor.Editor.Core.Tools.CodeGen;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Interfaces;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx.ObservableProperty;
using JetBrains.Annotations;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CueSheetParameterPaneModel : ICueSheetParameterPaneModel
    {
        private readonly CueSheetAsset _asset;
        private readonly IAssetSaveService _assetSaveService;
        private readonly ObservableProperty<string> _codeGenClassSuffix;

        private readonly ObservableProperty<bool> _codeGenEnabled;
        private readonly ObservableProperty<CueSheetCodeGenMode> _codeGenMode;
        private readonly ObservableProperty<string> _codeGenNamespace;
        private readonly ObservableProperty<string> _codeGenOutputPath;
        private readonly AutoIncrementHistory _history;
        private readonly ObservableCueSheet _target;
        private readonly ObservableProperty<bool> _useDefaultCodeGenClassSuffix;
        private readonly ObservableProperty<bool> _useDefaultCodeGenNamespace;
        private readonly ObservableProperty<bool> _useDefaultCodeGenOutputPath;

        public CueSheetParameterPaneModel([NotNull] CueSheet cueSheet,
            [NotNull] AutoIncrementHistory history,
            [NotNull] IAssetSaveService assetSaveService,
            [NotNull] CueSheetAsset asset)
        {
            _target = new ObservableCueSheet(cueSheet);
            _history = history;
            _assetSaveService = assetSaveService;
            _asset = asset;

            // ReSharper disable ArrangeObjectCreationWhenTypeNotEvident
            _codeGenEnabled = new(_asset.codeGenEnabled);
            _codeGenMode = new(_asset.codeGenMode);
            _useDefaultCodeGenOutputPath = new(_asset.useDefaultCodeGenOutputPath);
            _codeGenOutputPath = new(GetDisplayCodeGenOutputPath());
            _useDefaultCodeGenNamespace = new(_asset.useDefaultCodeGenNamespace);
            _codeGenNamespace = new(GetDisplayCodeGenNamespace());
            _useDefaultCodeGenClassSuffix = new(_asset.useDefaultCodeGenClassSuffix);
            _codeGenClassSuffix = new(GetDisplayCodeGenClassSuffix());
            // ReSharper enable ArrangeObjectCreationWhenTypeNotEvident
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

        #region CodeGenEnabled

        public bool CodeGenEnabled
        {
            get => _codeGenEnabled.Value;
            set
            {
                var old = _codeGenEnabled.Value;
                _history.Register($"Set CueSheetAsset {nameof(CodeGenEnabled)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _codeGenEnabled.SetValueAndNotify(_asset.codeGenEnabled = value);
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _codeGenEnabled.SetValueAndNotify(_asset.codeGenEnabled = old);
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<bool> CodeGenEnabledObservable => _codeGenEnabled;

        #endregion

        #region CodeGenMode

        public CueSheetCodeGenMode CodeGenMode
        {
            get => _codeGenMode.Value;
            set
            {
                var old = _codeGenMode.Value;
                _history.Register($"Set CueSheetAsset {nameof(CodeGenMode)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _codeGenMode.SetValueAndNotify(_asset.codeGenMode = value);
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _codeGenMode.SetValueAndNotify(_asset.codeGenMode = old);
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<CueSheetCodeGenMode> CodeGenModeObservable => _codeGenMode;

        #endregion

        #region CodeGenOutputPath

        public bool UseDefaultCodeGenOutputPath
        {
            get => _useDefaultCodeGenOutputPath.Value;
            set
            {
                var old = _useDefaultCodeGenOutputPath.Value;
                _history.Register($"Set CueSheetAsset {nameof(UseDefaultCodeGenOutputPath)} {value}", Redo, Undo);

                void Redo()
                {
                    _useDefaultCodeGenOutputPath.SetValueAndNotify(_asset.useDefaultCodeGenOutputPath = value);
                    _codeGenOutputPath.SetValueAndNotify(GetDisplayCodeGenOutputPath());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _useDefaultCodeGenOutputPath.SetValueAndNotify(_asset.useDefaultCodeGenOutputPath = old);
                    _codeGenOutputPath.SetValueAndNotify(GetDisplayCodeGenOutputPath());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }
            }
        }

        public IReadOnlyObservableProperty<bool> UseDefaultCodeGenOutputPathObservable => _useDefaultCodeGenOutputPath;

        public string CodeGenOutputPath
        {
            get => _codeGenOutputPath.Value;
            set
            {
                var old = _codeGenOutputPath.Value;
                _history.Register($"Set CueSheetAsset {nameof(CodeGenOutputPath)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _asset.codeGenOutputPath = value;
                    _codeGenOutputPath.SetValueAndNotify(GetDisplayCodeGenOutputPath());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _asset.codeGenOutputPath = old;
                    _codeGenOutputPath.SetValueAndNotify(GetDisplayCodeGenOutputPath());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<string> CodeGenOutputPathObservable => _codeGenOutputPath;

        #endregion

        #region CodeGenNamespace

        public bool UseDefaultCodeGenNamespace
        {
            get => _useDefaultCodeGenNamespace.Value;
            set
            {
                var old = _useDefaultCodeGenNamespace.Value;
                _history.Register($"Set CueSheetAsset {nameof(UseDefaultCodeGenNamespace)} {value}", Redo, Undo);

                void Redo()
                {
                    _useDefaultCodeGenNamespace.SetValueAndNotify(_asset.useDefaultCodeGenNamespace = value);
                    _codeGenNamespace.SetValueAndNotify(GetDisplayCodeGenNamespace());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _useDefaultCodeGenNamespace.SetValueAndNotify(_asset.useDefaultCodeGenNamespace = old);
                    _codeGenNamespace.SetValueAndNotify(GetDisplayCodeGenNamespace());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }
            }
        }

        public IReadOnlyObservableProperty<bool> UseDefaultCodeGenNamespaceObservable => _useDefaultCodeGenNamespace;

        public string CodeGenNamespace
        {
            get => _codeGenNamespace.Value;
            set
            {
                var old = _codeGenNamespace.Value;
                _history.Register($"Set CueSheetAsset {nameof(CodeGenNamespace)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _asset.codeGenNamespace = value;
                    _codeGenNamespace.SetValueAndNotify(GetDisplayCodeGenNamespace());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _asset.codeGenNamespace = old;
                    _codeGenNamespace.SetValueAndNotify(GetDisplayCodeGenNamespace());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<string> CodeGenNamespaceObservable => _codeGenNamespace;

        #endregion

        #region CodeGenClassSuffix

        public bool UseDefaultCodeGenClassSuffix
        {
            get => _useDefaultCodeGenClassSuffix.Value;
            set
            {
                var old = _useDefaultCodeGenClassSuffix.Value;
                _history.Register($"Set CueSheetAsset {nameof(UseDefaultCodeGenClassSuffix)} {value}", Redo, Undo);

                void Redo()
                {
                    _useDefaultCodeGenClassSuffix.SetValueAndNotify(_asset.useDefaultCodeGenClassSuffix = value);
                    _codeGenClassSuffix.SetValueAndNotify(GetDisplayCodeGenClassSuffix());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _useDefaultCodeGenClassSuffix.SetValueAndNotify(_asset.useDefaultCodeGenClassSuffix = old);
                    _codeGenClassSuffix.SetValueAndNotify(GetDisplayCodeGenClassSuffix());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }
            }
        }

        public IReadOnlyObservableProperty<bool> UseDefaultCodeGenClassSuffixObservable =>
            _useDefaultCodeGenClassSuffix;

        public string CodeGenClassSuffix
        {
            get => _codeGenClassSuffix.Value;
            set
            {
                var old = _codeGenClassSuffix.Value;
                _history.Register($"Set CueSheetAsset {nameof(CodeGenClassSuffix)} {value}", Redo, Undo);

                #region LocalMethods

                void Redo()
                {
                    _asset.codeGenClassSuffix = value;
                    _codeGenClassSuffix.SetValueAndNotify(GetDisplayCodeGenClassSuffix());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                void Undo()
                {
                    _asset.codeGenClassSuffix = old;
                    _codeGenClassSuffix.SetValueAndNotify(GetDisplayCodeGenClassSuffix());
                    EditorUtility.SetDirty(_asset);
                    _assetSaveService.Save();
                }

                #endregion
            }
        }

        public IReadOnlyObservableProperty<string> CodeGenClassSuffixObservable => _codeGenClassSuffix;

        #endregion

        public CueEnumCodeWriter.WriteResult GenerateCode()
        {
            return CueEnumCodeWriter.Write(_asset);
        }

        public void RefreshResolvedCodeGenDefaults()
        {
            if (_asset.useDefaultCodeGenOutputPath)
                _codeGenOutputPath.SetValueAndNotify(GetDisplayCodeGenOutputPath());

            if (_asset.useDefaultCodeGenNamespace)
                _codeGenNamespace.SetValueAndNotify(GetDisplayCodeGenNamespace());

            if (_asset.useDefaultCodeGenClassSuffix)
                _codeGenClassSuffix.SetValueAndNotify(GetDisplayCodeGenClassSuffix());
        }

        private string GetDisplayCodeGenClassSuffix()
        {
            return _asset.useDefaultCodeGenClassSuffix
                ? CueEnumCodeGenSettingsResolver
                    .Resolve(_asset, AudioConductorEditorSettingsRepository.instance.Settings).ClassSuffix
                : _asset.codeGenClassSuffix ?? string.Empty;
        }

        private string GetDisplayCodeGenNamespace()
        {
            return _asset.useDefaultCodeGenNamespace
                ? CueEnumCodeGenSettingsResolver
                    .Resolve(_asset, AudioConductorEditorSettingsRepository.instance.Settings).Namespace
                : _asset.codeGenNamespace ?? string.Empty;
        }

        private string GetDisplayCodeGenOutputPath()
        {
            return _asset.useDefaultCodeGenOutputPath
                ? CueEnumCodeGenSettingsResolver
                    .Resolve(_asset, AudioConductorEditorSettingsRepository.instance.Settings).OutputPath
                : _asset.codeGenOutputPath ?? string.Empty;
        }
    }
}
