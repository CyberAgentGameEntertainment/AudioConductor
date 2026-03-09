// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;

namespace AudioConductor.Core
{
    public sealed partial class Conductor
    {
        internal readonly struct EvictionResult
        {
            public readonly uint Id;
            public readonly uint CueSheetId;
            public readonly Cue Cue;
            public readonly bool IsManaged;

            public EvictionResult(uint id, uint cueSheetId, Cue cue, bool isManaged)
            {
                Id = id;
                CueSheetId = cueSheetId;
                Cue = cue;
                IsManaged = isManaged;
            }
        }

        private sealed class CueSheetRegistration
        {
            private readonly Dictionary<string, Cue> _cueNameLookup;
            private readonly Dictionary<Cue, CueState> _cueStateCache = new();

            internal CueSheetRegistration(CueSheetAsset asset)
            {
                Asset = asset;
                var cueList = asset.cueSheet.cueList;
                _cueNameLookup = new Dictionary<string, Cue>(cueList.Count);
                for (var i = 0; i < cueList.Count; i++)
                    _cueNameLookup[cueList[i].name] = cueList[i];
            }

            internal CueSheetAsset Asset { get; }

            internal Cue? FindCue(string cueName)
            {
                _cueNameLookup.TryGetValue(cueName, out var cue);
                return cue;
            }

            internal CueState GetOrCreateCueState(uint cueSheetId, Cue cue)
            {
                if (!_cueStateCache.TryGetValue(cue, out var state))
                {
                    state = new CueState(cueSheetId, cue);
                    _cueStateCache[cue] = state;
                }

                return state;
            }
        }

        internal readonly struct PlaybackState
        {
            internal PlaybackState(uint id, uint cueSheetId, Cue cue, IInternalPlayer player, int priority)
            {
                Id = id;
                CueSheetId = cueSheetId;
                Cue = cue;
                Player = player;
                Priority = priority;
            }

            internal uint Id { get; }
            internal uint CueSheetId { get; }
            internal Cue Cue { get; }
            internal IInternalPlayer Player { get; }
            internal int Priority { get; }
        }

        internal readonly struct OneShotState
        {
            internal OneShotState(uint id, uint cueSheetId, Cue cue, IInternalPlayer player, int priority)
            {
                Id = id;
                CueSheetId = cueSheetId;
                Cue = cue;
                Player = player;
                Priority = priority;
            }

            internal uint Id { get; }
            internal uint CueSheetId { get; }
            internal Cue Cue { get; }
            internal IInternalPlayer Player { get; }
            internal int Priority { get; }
        }
    }
}
