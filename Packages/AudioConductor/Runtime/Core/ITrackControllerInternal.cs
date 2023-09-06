// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

namespace AudioConductor.Runtime.Core
{
    internal interface ITrackControllerInternal : ITrackController
    {
        IAudioClipPlayer Player { get; }
        void Play();
        void Stop(bool isFade);
        void ReleasePlayer();
    }
}
