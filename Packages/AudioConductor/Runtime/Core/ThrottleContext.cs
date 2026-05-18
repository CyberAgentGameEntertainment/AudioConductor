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

        private ThrottleScopeState _cue;
        private ThrottleScopeState _sheet;
        private ThrottleScopeState _category;
        private ThrottleScopeState _global;

        internal int CueCount => _cue.Count;
        internal int SheetCount => _sheet.Count;
        internal int CategoryCount => _category.Count;
        internal int GlobalCount => _global.Count;

        internal ThrottleContext(uint cueSheetId, Cue cue)
        {
            _targetCueSheetId = cueSheetId;
            _targetCue = cue;
            _targetCategoryId = cue.categoryId;
            _cue = default;
            _sheet = default;
            _category = default;
            _global = default;
        }

        internal void Accumulate(in Playback p)
        {
            if (p.Player.State == PlayerState.Stopped)
                return;
            _global.Accumulate(in p);
            if (BelongsToSheet(in p)) _sheet.Accumulate(in p);
            if (BelongsToCue(in p)) _cue.Accumulate(in p);
            if (BelongsToCategory(in p)) _category.Accumulate(in p);
        }

        internal void AdjustAfterEviction(Playback? eviction)
        {
            if (!eviction.HasValue)
                return;
            var e = eviction.Value;
            _global.Decrement();
            if (BelongsToSheet(in e)) _sheet.Decrement();
            if (BelongsToCue(in e)) _cue.Decrement();
            if (BelongsToCategory(in e)) _category.Decrement();
        }

        internal bool ResolveCue(ThrottleType type, int limit, int incomingPriority, out Playback? eviction)
        {
            if (!_cue.Resolve(type, limit, incomingPriority, out eviction))
                return false;
            AdjustAfterEviction(eviction);
            return true;
        }

        internal bool ResolveSheet(ThrottleType type, int limit, int incomingPriority, out Playback? eviction)
        {
            if (!_sheet.Resolve(type, limit, incomingPriority, out eviction))
                return false;
            AdjustAfterEviction(eviction);
            return true;
        }

        internal bool ResolveCategory(ThrottleType type, int limit, int incomingPriority, out Playback? eviction)
        {
            if (!_category.Resolve(type, limit, incomingPriority, out eviction))
                return false;
            AdjustAfterEviction(eviction);
            return true;
        }

        internal bool ResolveGlobal(ThrottleType type, int limit, int incomingPriority, out Playback? eviction)
        {
            return _global.Resolve(type, limit, incomingPriority, out eviction);
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
