// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
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

        // Physical storage stays as fields (Playback contains a reference, so it cannot be
        // stackalloc'd). Only GetState/SetState/AccumulateInto touch these fields directly.
        private ThrottleScopeState _cue;
        private ThrottleScopeState _sheet;
        private ThrottleScopeState _category;
        private ThrottleScopeState _global;

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
            for (var i = 0; i < (int)ThrottleScopeKind.Count; i++)
            {
                var kind = (ThrottleScopeKind)i;
                if (Belongs(kind, in p))
                    AccumulateInto(kind, in p);
            }
        }

        internal bool Resolve(ThrottleScopeKind kind, ThrottleSetting setting, int incomingPriority)
        {
            var state = GetState(kind);
            if (!state.Resolve(setting.Type, setting.Limit, incomingPriority, out var eviction))
                return false;
            state.PendingEviction = eviction;
            SetState(kind, state);
            AdjustAfterEviction(eviction);
            return true;
        }

        internal readonly int Count(ThrottleScopeKind kind)
        {
            return GetState(kind).Count;
        }

        internal readonly Playback? PendingEviction(ThrottleScopeKind kind)
        {
            return GetState(kind).PendingEviction;
        }

        private void AdjustAfterEviction(Playback? eviction)
        {
            if (!eviction.HasValue)
                return;
            var e = eviction.Value;
            for (var i = 0; i < (int)ThrottleScopeKind.Count; i++)
            {
                var kind = (ThrottleScopeKind)i;
                if (!Belongs(kind, in e))
                    continue;
                var state = GetState(kind);
                state.Decrement();
                SetState(kind, state);
            }
        }

        private readonly bool Belongs(ThrottleScopeKind kind, in Playback p)
        {
            return kind switch
            {
                ThrottleScopeKind.Cue => p.Cue == _targetCue,
                ThrottleScopeKind.Sheet => p.CueSheetId == _targetCueSheetId,
                ThrottleScopeKind.Category => p.Cue.categoryId == _targetCategoryId,
                ThrottleScopeKind.Global => true,
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        private void AccumulateInto(ThrottleScopeKind kind, in Playback p)
        {
            switch (kind)
            {
                case ThrottleScopeKind.Cue:
                    _cue.Accumulate(in p);
                    break;
                case ThrottleScopeKind.Sheet:
                    _sheet.Accumulate(in p);
                    break;
                case ThrottleScopeKind.Category:
                    _category.Accumulate(in p);
                    break;
                case ThrottleScopeKind.Global:
                    _global.Accumulate(in p);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        private readonly ThrottleScopeState GetState(ThrottleScopeKind kind)
        {
            return kind switch
            {
                ThrottleScopeKind.Cue => _cue,
                ThrottleScopeKind.Sheet => _sheet,
                ThrottleScopeKind.Category => _category,
                ThrottleScopeKind.Global => _global,
                _ => throw new ArgumentOutOfRangeException(nameof(kind))
            };
        }

        private void SetState(ThrottleScopeKind kind, in ThrottleScopeState state)
        {
            switch (kind)
            {
                case ThrottleScopeKind.Cue:
                    _cue = state;
                    break;
                case ThrottleScopeKind.Sheet:
                    _sheet = state;
                    break;
                case ThrottleScopeKind.Category:
                    _category = state;
                    break;
                case ThrottleScopeKind.Global:
                    _global = state;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
    }
}
