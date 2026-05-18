// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using static AudioConductor.Core.Conductor;

namespace AudioConductor.Core
{
    internal ref struct ThrottleContext
    {
        private readonly uint _targetCueSheetId;
        private readonly Cue _targetCue;
        private readonly int _targetCategoryId;

        internal ThrottleScopeState Cue;
        internal ThrottleScopeState Sheet;
        internal ThrottleScopeState Category;
        internal ThrottleScopeState Global;

        internal ThrottleContext(uint cueSheetId, Cue cue, int categoryId)
        {
            _targetCueSheetId = cueSheetId;
            _targetCue = cue;
            _targetCategoryId = categoryId;
            Cue = default;
            Sheet = default;
            Category = default;
            Global = default;
        }

        internal void Accumulate(in Playback p)
        {
            if (p.Player.State == PlayerState.Stopped)
                return;
            Global.Accumulate(in p);
            if (BelongsToSheet(in p)) Sheet.Accumulate(in p);
            if (BelongsToCue(in p)) Cue.Accumulate(in p);
            if (BelongsToCategory(in p)) Category.Accumulate(in p);
        }

        internal void AdjustAfterEviction(Playback? eviction)
        {
            if (!eviction.HasValue)
                return;
            var e = eviction.Value;
            Global.Decrement();
            if (BelongsToSheet(in e)) Sheet.Decrement();
            if (BelongsToCue(in e)) Cue.Decrement();
            if (BelongsToCategory(in e)) Category.Decrement();
        }

        internal bool ResolveAndAdjust(ref ThrottleScopeState state, ThrottleType type,
            int limit, int incomingPriority, out Playback? eviction)
        {
            if (!state.Resolve(type, limit, incomingPriority, out eviction))
                return false;
            AdjustAfterEviction(eviction);
            return true;
        }

        private readonly bool BelongsToSheet(in Playback p)
        {
            return p.CueSheetId == _targetCueSheetId;
        }

        private readonly bool BelongsToCue(in Playback p)
        {
            return p.Cue == _targetCue;
        }

        private readonly bool BelongsToCategory(in Playback p)
        {
            return p.Cue.categoryId == _targetCategoryId;
        }
    }
}
