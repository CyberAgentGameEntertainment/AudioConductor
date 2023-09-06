// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class CuePreviewModel
    {
        private readonly Cue _cue;
        private readonly CueSheet _cueSheet;

        private ICueController _controller;

        public CuePreviewModel(ItemCue item)
        {
            _cue = item.RawData;

            var cueSheetItem = (ItemCueSheet)item.parent;
            _cueSheet = cueSheetItem.RawData;
        }

        public ICueController Play()
        {
            var settings = AudioConductorSettingsRepository.instance.Settings;
            if (settings == null)
                return null;

            if (_controller == null)
            {
                AudioConductorInterface.ForceSetup(settings);
                var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
                asset.cueSheet = _cueSheet;
                _controller = AudioConductorInterface.CreateController(asset, _cue.name);
            }

            _controller.Play();
            return _controller;
        }

        public void Stop()
        {
            _controller?.Dispose();
            _controller = null;
        }
    }
}
