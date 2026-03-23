// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Runtime.CompilerServices;
using AudioConductor.Core.Shared;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core
{
    internal sealed class AudioClipPlayerCore : IInternalPlayer
    {
        private const int SourceNum = 2;
        private const float MinimumDuration = 1.0f;
        private const int VolumeScale = 10000;
        private readonly IDspClock _dspClock;

        private readonly IAudioSourceWrapper[] _sources;

        private int _endSample;
        private int _frequency;
        private bool _isLoop;
        private bool _isPlaybackActive;
        private int _lastAppliedVolumeScaled = -1;
        private int _loopStartSample;
        private double _nextEventTime;
        private int _nextPlayAudioSourceIndex;
        private Action? _onEnd;
        private Action? _onStop;
        private double _pauseEndTime;
        private double _pauseStartTime;
        private int _pausedIndex;
        private float _pitchExternal;
        private double _scheduledEndTime;
        private int _startSample;
        private float _volumeCategory = 1f;
        private float _volumeMaster = 1f;
        private float _volumeRuntime;

        internal AudioClipPlayerCore(IAudioSourceWrapper[] sources, IDspClock dspClock)
        {
            _sources = sources;
            _dspClock = dspClock;
        }

        internal float VolumeAsset { get; private set; }
        internal float PitchInternal { get; private set; }

        public uint ActiveFadeId { get; set; }
        public bool IsFading { get; set; }
        public int ClipSamples { get; private set; }
        public int CategoryId { get; private set; }

        public bool IsPlaying
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _sources[0].IsPlaying || _sources[1].IsPlaying;
        }

        public bool IsPaused { get; private set; }

        public void Setup(AudioMixerGroup? audioMixerGroup,
            AudioClip clip,
            int categoryId,
            float volume,
            float pitch,
            bool isLoop,
            int startSample,
            int loopStartSample,
            int endSample)
        {
            if (clip == null)
                return;

            _isLoop = isLoop;

            ResetState();

            _sources[0].OutputAudioMixerGroup = audioMixerGroup;
            _sources[0].Clip = clip;
            _sources[0].PlayOnAwake = false;
            _sources[0].Loop = false;
            _sources[0].TimeSamples = startSample;
            _sources[1].OutputAudioMixerGroup = audioMixerGroup;
            _sources[1].Clip = clip;
            _sources[1].PlayOnAwake = false;
            _sources[1].Loop = false;

            _frequency = clip.frequency;
            ClipSamples = clip.samples;
            CategoryId = categoryId;

            _volumeRuntime = _pitchExternal = 1f;
            SetPitchInternal(pitch);
            VolumeAsset = volume;
            VolumeFade = 1f;
            UpdateVolume();

            _onEnd = _onStop = null;

            _nextPlayAudioSourceIndex = 0;
            IsPaused = false;
            _nextEventTime = 0;
            _pausedIndex = 0;
            _pauseStartTime = 0;
            _pauseEndTime = 0;
            _scheduledEndTime = 0;

            _startSample = ValueRangeConst.StartSample.Clamp(startSample, ClipSamples);
            _loopStartSample = ValueRangeConst.LoopStartSample.Clamp(loopStartSample, ClipSamples);
            _endSample = ValueRangeConst.EndSample.Clamp(endSample, ClipSamples);
        }

        public void Play()
        {
            _sources[0].Stop();
            _sources[1].Stop();

            _isPlaybackActive = true;
            _sources[1].Enabled = _isLoop;

            // for smooth switching of AudioSource
            // https://qiita.com/tatmos/items/4c78c127291a0c3b74ed
            const float delay = 0.1f;
            SetupPlayLoopSchedule(_dspClock.DspTime + delay, _startSample);
        }

        public void Restart()
        {
            Play();
        }

        public void Pause()
        {
            if (IsPaused)
                return;

            _pauseStartTime = _dspClock.DspTime;

            if (_isLoop)
            {
                if (_sources[0].IsPlaying)
                {
                    _sources[0].Pause();
                    _pausedIndex = 0;
                    _sources[1].Stop();
                }
                else if (_sources[1].IsPlaying)
                {
                    _sources[0].Stop();
                    _sources[1].Pause();
                    _pausedIndex = 1;
                }

                IsPaused = true;
                return;
            }

            _sources[0].Pause();
            IsPaused = true;
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            _pauseEndTime = _dspClock.DspTime;
            var pausedDuration = _pauseEndTime - _pauseStartTime;
            RescheduleEndTime(pausedDuration);

            if (_isLoop)
            {
                _sources[_pausedIndex].UnPause();
                IsPaused = false;
                return;
            }

            _sources[0].UnPause();
            IsPaused = false;
        }

        public void Stop()
        {
            _sources[0].Stop();

            if (_isLoop)
                _sources[1].Stop();

            _isPlaybackActive = false;
            InvokeStopAction();
            IsPaused = false;
        }

        public float GetActualVolume()
        {
            return ValueRangeConst.Volume.Clamp(VolumeAsset * VolumeFade * _volumeRuntime * _volumeMaster *
                                                _volumeCategory);
        }

        public float GetVolume()
        {
            return _volumeRuntime;
        }

        public void SetVolume(float volume)
        {
            _volumeRuntime = volume;
            UpdateVolume();
        }

        public float GetActualPitch()
        {
            return Mathf.Clamp(PitchInternal * _pitchExternal, -ValueRangeConst.Pitch.Max, ValueRangeConst.Pitch.Max);
        }

        public float GetPitch()
        {
            return _pitchExternal;
        }

        public void SetPitch(float pitch)
        {
            _pitchExternal = pitch;
            UpdatePitch();
        }

        public void AddStopAction(Action onStop)
        {
            _onStop += onStop;
        }

        public void AddEndAction(Action onEnd)
        {
            _onEnd += onEnd;
        }

        public int GetCurrentSample()
        {
            if (_isLoop)
            {
                var source = GetPlayingSource();
                return source == null ? 0 : source.TimeSamples;
            }

            return _sources[0].TimeSamples;
        }

        public void SetCurrentSample(int sample)
        {
            if (_isLoop)
            {
                var source = GetPlayingSource();
                if (source == null)
                    return;
                source.TimeSamples = sample;
            }
            else
            {
                _sources[0].TimeSamples = sample;
            }

            RescheduleEndTime();
        }

        public float VolumeFade { get; private set; } = 1f;

        public void SetVolumeFade(float fade)
        {
            VolumeFade = fade;
            UpdateVolume();
        }

        public void SetMasterVolume(float volume)
        {
            _volumeMaster = volume;
            UpdateVolume();
        }

        public void SetCategoryVolume(float volume)
        {
            _volumeCategory = volume;
            UpdateVolume();
        }

        public void ManualUpdate(float _)
        {
            if (!_isPlaybackActive && !IsPaused)
                return;

            UpdateVolume();

            if (IsPaused)
                return;

            if (_dspClock.DspTime < _nextEventTime)
                return;

            if (_isLoop)
            {
                PlayLoop();
            }
            else
            {
                _onEnd?.Invoke();
                Stop();
            }
        }

        public void ResetState()
        {
            _sources[0].Stop();
            _sources[1].Stop();

            _startSample = _endSample = 0;
            _volumeCategory = 1f;
            _volumeMaster = 1f;
            VolumeFade = 1f;
            _lastAppliedVolumeScaled = -1;
            ActiveFadeId = 0;
            IsFading = false;
            _isPlaybackActive = false;
            _pitchExternal = 1f;

            _onStop = _onEnd = null;
        }

        private void PlayLoop()
        {
            SetupPlayLoopSchedule(_nextEventTime + MinimumDuration, _loopStartSample);
        }

        private void SetupPlayLoopSchedule(double playStartTime, int startSample)
        {
            var pitch = Mathf.Abs(GetActualPitch());
            if (pitch == 0f)
                return;

            var samples = _endSample > startSample ? _endSample - startSample : startSample - _endSample;
            // In loop mode, zero-length region would cause _nextEventTime to never advance,
            // resulting in PlayLoop being called every ManualUpdate frame indefinitely.
            if (_isLoop && samples == 0)
            {
                _nextEventTime = double.MaxValue;
                return;
            }

            var duration = (float)samples / _frequency;
            _scheduledEndTime = playStartTime + duration / pitch;

            _sources[_nextPlayAudioSourceIndex].TimeSamples = startSample;
            _sources[_nextPlayAudioSourceIndex].PlayScheduled(playStartTime);
            _sources[_nextPlayAudioSourceIndex].SetScheduledEndTime(_scheduledEndTime);

            var minusDuration = _isLoop ? MinimumDuration : 0;
            _nextEventTime = _scheduledEndTime - minusDuration;

            FlipNextPlayAudioSourceIndex();
        }

        private void RescheduleEndTime()
        {
            var source = GetPlayingSource();
            if (source == null)
                return;

            var pitch = Mathf.Abs(GetActualPitch());
            if (pitch == 0f)
                return;

            var nowSample = source.TimeSamples;
            var samples = _endSample > nowSample ? _endSample - nowSample : nowSample - _endSample;
            var duration = (float)samples / _frequency;
            _scheduledEndTime = _dspClock.DspTime + duration / pitch;

            source.SetScheduledEndTime(_scheduledEndTime);
            var minusDuration = _isLoop ? MinimumDuration : 0;
            _nextEventTime = _scheduledEndTime - minusDuration;
        }

        private void RescheduleEndTime(double pausedDuration)
        {
            _scheduledEndTime += pausedDuration;
            _sources[_pausedIndex].SetScheduledEndTime(_scheduledEndTime);
            _nextEventTime += pausedDuration;
        }

        private void FlipNextPlayAudioSourceIndex()
        {
            _nextPlayAudioSourceIndex = _nextPlayAudioSourceIndex == 0 ? 1 : 0;
        }

        private void SetPitchInternal(float pitch)
        {
            PitchInternal = pitch;
            UpdatePitch();
        }

        private void UpdateVolume()
        {
            var volume = GetActualVolume();
            var volumeScaled = (int)(volume * VolumeScale);
            if (volumeScaled == _lastAppliedVolumeScaled)
                return;

            _lastAppliedVolumeScaled = volumeScaled;
            _sources[0].Volume = volume;

            if (_isLoop)
                _sources[1].Volume = volume;
        }

        private void UpdatePitch()
        {
            var pitch = GetActualPitch();
            _sources[0].Pitch = pitch;
            RescheduleEndTime();

            if (_isLoop)
                _sources[1].Pitch = pitch;
        }

        private void InvokeStopAction()
        {
            _onStop?.Invoke();
            _onStop = null;
        }

        private IAudioSourceWrapper? GetPlayingSource()
        {
            var playing0 = _sources[0].IsPlaying;
            var playing1 = _sources[1].IsPlaying;

            // AudioSource.isPlaying may both be true near the loop.
            if (playing0 && playing1)
                return _sources[0].TimeSamples > _sources[1].TimeSamples ? _sources[0] : _sources[1];

            return playing0 ? _sources[0] : playing1 ? _sources[1] : null;
        }
    }
}
