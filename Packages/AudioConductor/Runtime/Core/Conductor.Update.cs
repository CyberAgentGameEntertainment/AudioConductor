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

            // Iterate _managedPlaybacks.Values directly; collect removal keys in _removeKeyBuffer.
            _removeKeyBuffer.Clear();
            foreach (var playback in _managedPlaybacks.Values)
            {
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
                _managedPlaybacks.Remove(_removeKeyBuffer[i]);

            // Process one-shot states (swap-remove for O(1) removal).
            for (var i = 0; i < _oneShotPlaybacks.Count; i++)
            {
                var playback = _oneShotPlaybacks[i];
                playback.Player.ManualUpdate(deltaTime);

                if (playback.Player.State == PlayerState.Stopped)
                {
                    _oneShotProvider.Return(playback.Player);
                    _oneShotPlaybacks[i] = _oneShotPlaybacks[^1];
                    _oneShotPlaybacks.RemoveAt(_oneShotPlaybacks.Count - 1);
                    i--;
                }
            }
        }
    }
}
