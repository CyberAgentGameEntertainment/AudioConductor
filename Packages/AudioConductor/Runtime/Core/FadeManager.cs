// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Models;

namespace AudioConductor.Core
{
    /// <summary>
    ///     Manages fade state lifecycle: start, update, cancel, and fade-out target tracking.
    /// </summary>
    internal sealed class FadeManager
    {
        private const int PoolInitialCapacity = 8;
        private readonly HashSet<IFadeable> _fadeOutTargets = new(PoolInitialCapacity);
        private readonly Stack<FadeState> _fadePool = new(PoolInitialCapacity);
        private readonly List<FadeState> _fadeStates = new();
        private uint _fadeIdCounter;

        internal FadeManager()
        {
            for (var i = 0; i < PoolInitialCapacity; i++)
                _fadePool.Push(new FadeState());
        }

        internal void StartFade(IFadeable target, IFader fader, float from, float to, float duration)
        {
            var fadeId = NextFadeId();
            var fadeState = RentFadeState();
            fadeState.Setup(fadeId, target, fader, from, to, duration);
            target.ActiveFadeId = fadeId;
            target.IsFading = true;
            _fadeStates.Add(fadeState);
        }

        internal bool IsFadingOut(IFadeable target)
        {
            return _fadeOutTargets.Contains(target);
        }

        internal void MarkFadeOut(IFadeable target)
        {
            _fadeOutTargets.Add(target);
        }

        internal void RemoveFadeOutTarget(IFadeable target)
        {
            _fadeOutTargets.Remove(target);
        }

        internal void CancelFade(IFadeable target)
        {
            target.ActiveFadeId = 0;
            target.IsFading = false;
            _fadeOutTargets.Remove(target);
        }

        internal void Update(float deltaTime)
        {
            for (var i = 0; i < _fadeStates.Count; i++)
            {
                var fade = _fadeStates[i];

                // Stale check: the fade was invalidated by CancelFade.
                if (fade.Fadeable.ActiveFadeId != fade.Id)
                {
                    _fadeStates[i] = _fadeStates[^1];
                    _fadeStates.RemoveAt(_fadeStates.Count - 1);
                    _fadePool.Push(fade);
                    i--;
                    continue;
                }

                var finished = fade.Elapsed(deltaTime);
                if (finished)
                {
                    fade.Fadeable.IsFading = false;
                    fade.Fadeable.ActiveFadeId = 0;
                    _fadeStates[i] = _fadeStates[^1];
                    _fadeStates.RemoveAt(_fadeStates.Count - 1);
                    _fadePool.Push(fade);
                    i--;
                }
            }
        }

        internal void Dispose()
        {
            _fadeStates.Clear();
            _fadeOutTargets.Clear();
        }

        private FadeState RentFadeState()
        {
            return _fadePool.Count > 0 ? _fadePool.Pop() : new FadeState();
        }

        private uint NextFadeId()
        {
            var id = ++_fadeIdCounter;
            // 0 is the sentinel value meaning "no active fade".
            if (id == 0)
                id = ++_fadeIdCounter;
            return id;
        }
    }
}
