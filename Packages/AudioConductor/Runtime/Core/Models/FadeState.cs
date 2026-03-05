// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Runtime.Core.Models
{
    internal sealed class FadeState
    {
        private IFader _fader;

        public IFadeable Fadeable { get; private set; }
        public float FadeTime { get; private set; }
        public bool IsStopTarget { get; private set; }
        public float StartVolume { get; private set; }
        public float TargetVolume { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished { get; private set; }

        public void Setup(IFadeable fadeable, IFader fader, float startVolume, float targetVolume, float time,
            bool isStopTarget)
        {
            Fadeable = fadeable;
            _fader = fader;
            StartVolume = startVolume;
            TargetVolume = targetVolume;
            FadeTime = time;
            ElapsedTime = 0f;
            IsStopTarget = isStopTarget;
            IsFinished = false;
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
