// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

namespace AudioConductor.Runtime.Core
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

                if (!playback.Player.IsFading && _fadeManager.IsFadingOut(playback.Player))
                {
                    playback.Player.Stop();
                    _fadeManager.RemoveFadeOutTarget(playback.Player);
                    _playerProvider.Return(playback.Player);
                    _removeKeyBuffer.Add(playback.Id);
                    continue;
                }

                playback.Player.ManualUpdate(deltaTime);

                if (!playback.Player.IsPlaying && !playback.Player.IsPaused && !playback.Player.IsFading)
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

                if (!state.Player.IsPlaying && !state.Player.IsPaused)
                {
                    _oneShotProvider.Return(state.Player);
                    _oneShotStates[i] = _oneShotStates[_oneShotStates.Count - 1];
                    _oneShotStates.RemoveAt(_oneShotStates.Count - 1);
                    i--;
                }
            }
        }
    }
}
