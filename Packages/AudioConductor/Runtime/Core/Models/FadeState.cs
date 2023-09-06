// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    internal sealed class FadeState
    {
        public FadeState(IFadeable fadeable)
        {
            Fadeable = fadeable;
        }

        public IFadeable Fadeable { get; }
        public float FadeTime { get; private set; }
        public bool IsStopTarget { get; private set; }
        public float StartVolume { get; private set; }
        public float TargetVolume { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished { get; private set; }

        public void Setup(float startVolume, float targetVolume, float time, bool isStopTarget)
        {
            StartVolume = startVolume;
            TargetVolume = targetVolume;
            FadeTime = time;
            ElapsedTime = 0f;
            IsStopTarget = isStopTarget;
        }

        public bool Elapsed(float time)
        {
            ElapsedTime += time;

            var elapsedRate = Mathf.Clamp01(ElapsedTime / FadeTime);
            var volume = Mathf.Lerp(StartVolume, TargetVolume, elapsedRate);
            Fadeable.SetVolumeInternal(volume);

            IsFinished = elapsedRate >= 1f;
            if (IsFinished && IsStopTarget)
                Fadeable.Stop();

            return IsFinished;
        }
    }
}
