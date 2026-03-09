// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CuePreviewModel
    {
        private readonly Cue _cue;
        private readonly CueSheet _cueSheet;
        private readonly Func<AudioConductorSettings?> _settingsProvider;

        private Conductor? _conductor;
        private AudioConductorSettings? _conductorSettings;
        private CueSheetHandle _cueSheetHandle;
        private CueSheetAsset? _tempAsset;

        public CuePreviewModel(ItemCue item, Func<AudioConductorSettings?> settingsProvider)
        {
            _cue = item.RawData;
            _settingsProvider = settingsProvider;

            var cueSheetItem = (ItemCueSheet)item.parent;
            _cueSheet = cueSheetItem.RawData;
        }

        public void Play()
        {
            var settings = _settingsProvider();
            if (settings == null)
                return;

            // Recreate the conductor when the selected Settings have changed.
            if (!ReferenceEquals(_conductorSettings, settings))
                Stop();

            if (_conductor == null)
            {
                _conductorSettings = settings;
                _conductor = new Conductor(settings);
                _tempAsset = ScriptableObject.CreateInstance<CueSheetAsset>();
                _tempAsset.cueSheet = _cueSheet;
                _cueSheetHandle = _conductor.RegisterCueSheet(_tempAsset);
            }

            _conductor.Play(_cueSheetHandle, _cue.name);
        }

        public void Stop()
        {
            _conductor?.Dispose();
            _conductor = null;
            _conductorSettings = null;
            _cueSheetHandle = default;

            if (_tempAsset != null)
            {
                Object.DestroyImmediate(_tempAsset);
                _tempAsset = null;
            }
        }
    }
}
