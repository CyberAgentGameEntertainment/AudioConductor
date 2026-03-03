// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class RandomTrackSelector : ITrackSelector
    {
        public int SelectNext(TrackSelectionContext context)
        {
            if (context.Tracks.Count == 0)
                return -1;

            var weightTotal = 0;
            foreach (var track in context.Tracks)
                weightTotal += track.randomWeight;

            int index;
            if (weightTotal == 0)
            {
                index = Random.Range(0, context.Tracks.Count);
            }
            else
            {
                var total = 0;
                var randomValue = Random.Range(0, weightTotal);
                index = 0;
                for (var i = 0; i < context.Tracks.Count; i++)
                {
                    total += context.Tracks[i].randomWeight;
                    if (randomValue < total)
                    {
                        index = i;
                        break;
                    }
                }
            }

            context.CurrentIndex = index;
            context.PlayCount++;
            return index;
        }
    }
}
