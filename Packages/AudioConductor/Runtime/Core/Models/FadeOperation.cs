// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using UnityEngine;

namespace AudioConductor.Core.Models
{
    internal sealed class FadeOperation
    {
        private IFader _fader = null!;

        public uint Id { get; private set; }
        public IFadeable Fadeable { get; private set; } = null!;
        public float FadeTime { get; private set; }
        public float StartVolume { get; private set; }
        public float TargetVolume { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsFinished { get; private set; }

        public void Setup(uint id, IFadeable fadeable, IFader fader, float startVolume, float targetVolume, float time)
        {
            Id = id;
            Fadeable = fadeable;
            _fader = fader;
            StartVolume = startVolume;
            TargetVolume = targetVolume;
            FadeTime = time;
            ElapsedTime = 0f;
            IsFinished = false;
        }

        public bool Elapsed(float time)
        {
            if (FadeTime <= 0f)
            {
                Fadeable.SetVolumeFade(TargetVolume);
                IsFinished = true;
                return true;
            }

            ElapsedTime += time;

            var elapsedRate = Mathf.Clamp01(ElapsedTime / FadeTime);
            var volume = _fader.Evaluate(elapsedRate, StartVolume, TargetVolume);
            Fadeable.SetVolumeFade(volume);

            IsFinished = elapsedRate >= 1f;

            return IsFinished;
        }
    }
}
