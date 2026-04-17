// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using UnityEngine;

namespace AudioConductor.Core.Models
{
    internal sealed class CueState
    {
        private readonly TrackSelectionContext _context;
        private readonly ITrackSelector _trackSelector;
        private Dictionary<string, int>? _trackNameLookup;

        public CueState(uint cueSheetId, Cue cue)
        {
            CueSheetId = cueSheetId;
            Cue = cue;
            _context = new TrackSelectionContext(cue.trackList);
            _trackSelector = cue.playType == CuePlayType.Random
                ? TrackSelectors.Random
                : TrackSelectors.Sequential;
        }

        public uint CueSheetId { get; }
        public Cue Cue { get; }

        public Track? NextTrack(ITrackSelector? selectorOverride = null)
        {
            if (Cue.trackList.Count == 0)
                return null;

            var selector = selectorOverride ?? _trackSelector;
            var index = selector.SelectNext(_context);
            if (index < 0 || index >= Cue.trackList.Count)
                return null;

            return Cue.trackList[index];
        }

        public Track? GetTrack(int index)
        {
            if (index < 0 || Cue.trackList.Count <= index)
                return null;

            return Cue.trackList[index];
        }

        public Track? GetTrack(string name)
        {
            if (_trackNameLookup == null)
            {
                _trackNameLookup = new Dictionary<string, int>(Cue.trackList.Count);
                for (var i = 0; i < Cue.trackList.Count; i++)
                {
                    var trackName = Cue.trackList[i].name;
                    var added = _trackNameLookup.TryAdd(trackName, i);
                    Debug.Assert(added, $"Duplicate track name detected: {trackName}");
                }
            }

            return _trackNameLookup.TryGetValue(name, out var index) ? Cue.trackList[index] : null;
        }
    }
}
