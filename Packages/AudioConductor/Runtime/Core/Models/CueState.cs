// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Runtime.Core.Enums;

namespace AudioConductor.Runtime.Core.Models
{
    internal sealed class CueState
    {
        private readonly ITrackSelector _selector;

        public CueState(uint cueSheetManageNumber, Cue cue)
        {
            CueSheetManageNumber = cueSheetManageNumber;
            Cue = cue;

            switch (Cue.playType)
            {
                case CuePlayType.Random:
                    _selector = new RandomTrackSelector();
                    break;
                case CuePlayType.Sequential:
                default:
                    _selector = new SequentialTrackSelector();
                    break;
            }

            _selector.Setup(Cue.trackList);
        }

        public uint CueSheetManageNumber { get; }
        public Cue Cue { get; }

        public Track NextTrack()
        {
            var index = _selector.NextTrackIndex();
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
