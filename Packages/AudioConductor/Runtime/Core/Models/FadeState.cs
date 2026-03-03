// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    internal sealed class FadeState
    {
        private readonly IFader _fader;

        public FadeState(IFadeable fadeable, IFader fader)
        {
            Fadeable = fadeable;
            _fader = fader;
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
            if (FadeTime <= 0f)
            {
                Fadeable.SetVolumeInternal(TargetVolume);
                IsFinished = true;
                if (IsStopTarget)
                    Fadeable.Stop();
                return true;
            }

            ElapsedTime += time;

            var elapsedRate = Mathf.Clamp01(ElapsedTime / FadeTime);
            var volume = _fader.Evaluate(elapsedRate, StartVolume, TargetVolume);
            Fadeable.SetVolumeInternal(volume);

            IsFinished = elapsedRate >= 1f;
            if (IsFinished && IsStopTarget)
                Fadeable.Stop();

            return IsFinished;
        }
    }
}
