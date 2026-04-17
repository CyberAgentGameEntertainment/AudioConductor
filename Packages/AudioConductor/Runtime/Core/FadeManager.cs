// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Manages fade operation lifecycle: start, update, and cancel.
    /// </summary>
    internal sealed class FadeManager
    {
        private const int PoolInitialCapacity = 8;
        private readonly Stack<FadeOperation> _operationPool = new(PoolInitialCapacity);
        private readonly List<FadeOperation> _operations = new();
        private NonZeroSequence _fadeIdCounter;

        internal FadeManager()
        {
            for (var i = 0; i < PoolInitialCapacity; i++)
                _operationPool.Push(new FadeOperation());
        }

        internal void StartFade(IFadeable target, IFader fader, float from, float to, float duration)
        {
            var id = _fadeIdCounter.Next();
            var fadeState = RentOperation();
            fadeState.Setup(id, target, fader, from, to, duration);
            target.ActiveFadeId = id;
            target.FadeState = to > from ? FadeState.FadingIn : FadeState.FadingOut;
            _operations.Add(fadeState);
        }

        internal void CancelFade(IFadeable target)
        {
            target.ActiveFadeId = 0;
            target.FadeState = FadeState.None;
        }

        internal void Update(float deltaTime)
        {
            for (var i = 0; i < _operations.Count; i++)
            {
                var fade = _operations[i];

                // Stale check: the fade was invalidated by CancelFade.
                if (fade.Fadeable.ActiveFadeId != fade.Id)
                {
                    _operations[i] = _operations[^1];
                    _operations.RemoveAt(_operations.Count - 1);
                    _operationPool.Push(fade);
                    i--;
                    continue;
                }

                var finished = fade.Elapsed(deltaTime);
                if (finished)
                {
                    // FadingOut → FadingOutComplete so Conductor.Update can stop the player.
                    // FadingIn → None; the player continues playing normally with no further action.
                    fade.Fadeable.FadeState = fade.Fadeable.FadeState == FadeState.FadingOut
                        ? FadeState.FadingOutComplete
                        : FadeState.None;
                    fade.Fadeable.ActiveFadeId = 0;
                    _operations[i] = _operations[^1];
                    _operations.RemoveAt(_operations.Count - 1);
                    _operationPool.Push(fade);
                    i--;
                }
            }
        }

        internal void Dispose()
        {
            _operations.Clear();
        }

        private FadeOperation RentOperation()
        {
            return _operationPool.Count > 0 ? _operationPool.Pop() : new FadeOperation();
        }
    }
}
