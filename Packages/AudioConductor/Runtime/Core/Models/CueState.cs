// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Runtime.Core.Enums;

namespace AudioConductor.Runtime.Core.Models
{
    internal sealed class CueState
    {
        private readonly TrackSelectionContext _context;
        private readonly ITrackSelector _trackSelector;

        public CueState(uint cueSheetManageNumber, Cue cue)
        {
            CueSheetManageNumber = cueSheetManageNumber;
            Cue = cue;
            _context = new TrackSelectionContext(cue.trackList);
            _trackSelector = cue.playType == CuePlayType.Random
                ? TrackSelectors.Random
                : TrackSelectors.Sequential;
        }

        public uint CueSheetManageNumber { get; }
        public Cue Cue { get; }

        public Track NextTrack(ITrackSelector selectorOverride = null)
        {
            if (Cue.trackList.Count == 0)
                return null;

            var selector = selectorOverride ?? _trackSelector;
            var index = selector.SelectNext(_context);
            if (index < 0 || index >= Cue.trackList.Count)
                return null;

            return Cue.trackList[index];
        }

        public Track GetTrack(int index)
        {
            if (index < 0 || Cue.trackList.Count <= index)
                return null;

            return Cue.trackList[index];
        }

        public Track GetTrack(string name)
        {
            return Cue.trackList.FirstOrDefault(track => track.name == name);
        }
    }
}
