// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class TrackPreviewModel
    {
        private readonly Track _track;
        private readonly Cue _cue;
        private readonly CueSheet _cueSheet;

        public TrackPreviewModel(ItemTrack item)
        {
            _track = item.RawData;

            var cueItem = (ItemCue)item.parent;
            _cue = cueItem.RawData;

            var cueSheetItem = (ItemCueSheet)cueItem.parent;
            _cueSheet = cueSheetItem.RawData;
        }

        public TrackPreviewController Play(int? sample)
        {
            var clip = _track.audioClip;
            if (clip == null)
                return null;

            var volume = Calculator.CalcVolume(_cueSheet, _cue, _track);
            var pitch = Calculator.CalcPitch(_cueSheet, _cue, _track);
            var isLoop = _track.isLoop;
            var startSample = _track.startSample;
            var loopStartSample = _track.loopStartSample;
            var endSample = _track.endSample;

            var controller = new TrackPreviewController(clip, _cue.categoryId, volume, pitch, isLoop,
                                                        sample ?? startSample, loopStartSample, endSample);
            controller.Play();
            return controller;
        }
    }
}
