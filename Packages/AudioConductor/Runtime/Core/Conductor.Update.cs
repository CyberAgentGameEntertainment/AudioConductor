// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;

namespace AudioConductor.Core
{
    public sealed partial class Conductor
    {
        internal void Update(float deltaTime)
        {
            _fadeManager.Update(deltaTime);

            // Iterate _playbacks.Values directly; collect removal keys in _removeKeyBuffer.
            _removeKeyBuffer.Clear();
            foreach (var playback in _playbacks.Values)
            {
                if (playback.Player == null)
                    continue;

                if (playback.Player.FadeState == FadeState.FadingOutComplete)
                {
                    playback.Player.Stop();
                    _playerProvider.Return(playback.Player);
                    _removeKeyBuffer.Add(playback.Id);
                    continue;
                }

                playback.Player.ManualUpdate(deltaTime);

                if (playback.Player is { State: PlayerState.Stopped, FadeState: FadeState.None })
                {
                    _playerProvider.Return(playback.Player);
                    _removeKeyBuffer.Add(playback.Id);
                }
            }

            for (var i = 0; i < _removeKeyBuffer.Count; i++)
                _playbacks.Remove(_removeKeyBuffer[i]);

            // Process one-shot states (swap-remove for O(1) removal).
            for (var i = 0; i < _oneShotStates.Count; i++)
            {
                var state = _oneShotStates[i];
                if (state.Player == null)
                    continue;

                state.Player.ManualUpdate(deltaTime);

                if (state.Player.State == PlayerState.Stopped)
                {
                    _oneShotProvider.Return(state.Player);
                    _oneShotStates[i] = _oneShotStates[^1];
                    _oneShotStates.RemoveAt(_oneShotStates.Count - 1);
                    i--;
                }
            }
        }
    }
}
