// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal sealed class SequentialTrackSelector : ITrackSelector
    {
        public int SelectNext(TrackSelectionContext context)
        {
            if (context.Tracks.Count == 0)
                return -1;

            var next = context.CurrentIndex + 1;
            if (next >= context.Tracks.Count)
                next = 0;

            context.CurrentIndex = next;
            context.PlayCount++;
            return next;
        }
    }
}
