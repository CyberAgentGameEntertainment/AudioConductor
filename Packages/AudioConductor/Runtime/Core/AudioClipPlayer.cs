// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Provide functions for AudioClip playing.
    /// </summary>
    internal sealed class AudioClipPlayer : MonoBehaviour, IAudioClipPlayer, IFadeable
    {
        private const int AudioSourceNum = 2;
        private const float MinimumDuration = 1.0f;

        [SerializeField]
        private AudioSource[] _source = new AudioSource[AudioSourceNum];

        private float _volumeExternal;
        private float _pitchExternal;

        private bool _isLoop;
        private int _frequency;

        private Action _onEnd;
        private Action _onStop;

        private int _startSample;
        private int _loopStartSample;
        private int _endSample;

        private int _nextPlayAudioSourceIndex;
        private double _nextEventTime;
        private int _pausedIndex;
        private double _pauseStartTime;
        private double _pauseEndTime;
        private double _scheduledEndTime;

        internal float VolumeInternal { get; private set; }

        internal float PitchInternal { get; private set; }

        /// <inheritdoc />
        public int ClipSamples { get; private set; }

        /// <inheritdoc />
        public int CategoryId { get; private set; }

        /// <inheritdoc />
        public bool IsPlaying
        {
            get
            {
                bool IsPlaying(AudioSource source)
                    => source != null && source.isPlaying;

                return IsPlaying(_source[0]) || IsPlaying(_source[1]);
            }
        }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
        public void Setup(AudioMixerGroup audioMixerGroup,
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

            if (_source[0] == null || _source[1] == null)
            {
                Debug.LogError("AudioSource is null.");
                return;
            }

            _isLoop = isLoop;

            ResetState();

            _source[0].outputAudioMixerGroup = audioMixerGroup;
            _source[0].clip = clip;
            _source[0].playOnAwake = false;
            _source[0].loop = false;
            _source[0].timeSamples = startSample;
            _source[1].outputAudioMixerGroup = audioMixerGroup;
            _source[1].clip = clip;
            _source[1].playOnAwake = false;
            _source[1].loop = false;

            _frequency = clip.frequency;
            ClipSamples = clip.samples;
            CategoryId = categoryId;

            _volumeExternal = _pitchExternal = 1f;
            SetPitchInternal(pitch);
            SetVolumeInternal(volume);

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

        /// <inheritdoc />
        public void Play()
        {
            if (_source[0] == null || _source[1] == null)
                return;

            _source[0].Stop();
            _source[1].Stop();

            _source[1].enabled = _isLoop;

            // for smooth switching of AudioSource
            // https://qiita.com/tatmos/items/4c78c127291a0c3b74ed
            const float delay = 0.1f;
            SetupPlayLoopSchedule(AudioSettings.dspTime + delay, _startSample);
        }

        /// <inheritdoc />
        public void Restart()
        {
            Play();
        }

        /// <inheritdoc />
        public void Pause()
        {
            if (IsPaused)
                return;

            _pauseStartTime = AudioSettings.dspTime;

            if (_isLoop)
            {
                if (_source[0] == null || _source[1] == null)
                    return;

                if (_source[0].isPlaying)
                {
                    _source[0].Pause();
                    _pausedIndex = 0;
                    _source[1].Stop();
                }
                else if (_source[1].isPlaying)
                {
                    _source[0].Stop();
                    _source[1].Pause();
                    _pausedIndex = 1;
                }

                IsPaused = true;
                return;
            }

            if (_source[0] == null)
                return;

            _source[0].Pause();
            IsPaused = true;
        }

        /// <inheritdoc />
        public void Resume()
        {
            if (!IsPaused)
                return;

            _pauseEndTime = AudioSettings.dspTime;
            var pausedDuration = _pauseEndTime - _pauseStartTime;
            RescheduleEndTime(pausedDuration);

            if (_isLoop)
            {
                if (_source[0] == null || _source[1] == null)
                    return;

                _source[_pausedIndex].UnPause();
                IsPaused = false;
                return;
            }

            if (_source[0] == null)
                return;

            _source[0].UnPause();
            IsPaused = false;
        }

        /// <inheritdoc cref="IAudioClipPlayer.Stop" />
        public void Stop()
        {
            if (_source[0] != null)
                _source[0].Stop();

            if (_isLoop && _source[1] != null)
                _source[1].Stop();

            InvokeStopAction();
            IsPaused = false;
        }

        /// <inheritdoc />
        public float GetActualVolume()
            => ValueRangeConst.Volume.Clamp(VolumeInternal * _volumeExternal);

        /// <inheritdoc />
        public float GetVolume() => _volumeExternal;

        /// <inheritdoc />
        public void SetVolume(float volume)
        {
            _volumeExternal = volume;
            UpdateVolume();
        }

        /// <inheritdoc />
        public float GetActualPitch()
            => Mathf.Clamp(PitchInternal * _pitchExternal, -ValueRangeConst.Pitch.Max, ValueRangeConst.Pitch.Max);

        /// <inheritdoc />
        public float GetPitch() => _pitchExternal;

        /// <inheritdoc />
        public void SetPitch(float pitch)
        {
            _pitchExternal = pitch;
            UpdatePitch();
        }

        /// <inheritdoc />
        public void AddStopAction(Action onStop)
        {
            _onStop += onStop;
        }

        /// <inheritdoc />
        public void AddEndAction(Action onEnd)
        {
            _onEnd += onEnd;
        }

        /// <inheritdoc />
        public int GetCurrentSample()
        {
            if (_isLoop)
            {
                var source = GetPlayingSource();
                return source == null ? 0 : source.timeSamples;
            }

            return _source[0] == null ? 0 : _source[0].timeSamples;
        }

        /// <inheritdoc />
        public void SetCurrentSample(int sample)
        {
            if (_isLoop)
            {
                var source = GetPlayingSource();
                if (source == null)
                    return;
                source.timeSamples = sample;
            }
            else
                _source[0].timeSamples = sample;

            RescheduleEndTime();
        }

        public void SetVolumeInternal(float volume)
        {
            VolumeInternal = volume;
            UpdateVolume();
        }

        private void PlayLoop()
        {
            if (_source[0] == null || _source[1] == null)
                return;

            SetupPlayLoopSchedule(_nextEventTime + MinimumDuration, _loopStartSample);
        }

        private void SetupPlayLoopSchedule(double playStartTime, int startSample)
        {
            var samples = _endSample > startSample ? _endSample - startSample : startSample - _endSample;
            var duration = (float)samples / _frequency;
            var pitch = Mathf.Abs(GetActualPitch());
            _scheduledEndTime = playStartTime + duration / pitch;

            _source[_nextPlayAudioSourceIndex].timeSamples = startSample;
            _source[_nextPlayAudioSourceIndex].PlayScheduled(playStartTime);
            _source[_nextPlayAudioSourceIndex].SetScheduledEndTime(_scheduledEndTime);

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
            var nowSample = source.timeSamples;
            var samples = _endSample > nowSample ? _endSample - nowSample : nowSample - _endSample;
            var duration = (float)samples / _frequency;
            _scheduledEndTime = AudioSettings.dspTime + duration / pitch;

            source.SetScheduledEndTime(_scheduledEndTime);
            var minusDuration = _isLoop ? MinimumDuration : 0;
            _nextEventTime = _scheduledEndTime - minusDuration;
        }

        private void RescheduleEndTime(double pausedDuration)
        {
            _source[_pausedIndex].SetScheduledEndTime(_scheduledEndTime + pausedDuration);
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
            if (_source[0] == null)
                return;

            var volume = GetActualVolume();
            _source[0].volume = volume;

            if (_isLoop && _source[1] != null)
                _source[1].volume = volume;
        }

        private void UpdatePitch()
        {
            if (_source[0] == null)
                return;

            var pitch = GetActualPitch();
            _source[0].pitch = pitch;
            RescheduleEndTime();

            if (_isLoop && _source[1] != null)
                _source[1].pitch = pitch;
        }

        internal void ManualUpdate(float _)
        {
            UpdateVolume();

            if (IsPaused)
                return;

            if (AudioSettings.dspTime < _nextEventTime)
                return;

            if (_isLoop)
                PlayLoop();
            else
            {
                _onEnd?.Invoke();
                Stop();
            }
        }

        private void InvokeStopAction()
        {
            _onStop?.Invoke();
            _onStop = null;
        }

        public void ResetState()
        {
            if (_source[0] != null)
                _source[0].Stop();

            if (_source[1] != null)
                _source[1].Stop();

            _startSample = _endSample = 0;

            _onStop = _onEnd = null;
        }

        private AudioSource GetPlayingSource()
        {
            var playing0 = _source[0] != null && _source[0].isPlaying;
            var playing1 = _source[1] != null && _source[1].isPlaying;

            // AudioSource.isPlaying may both be true near the loop.
            if (playing0 && playing1)
                return _source[0].timeSamples > _source[1].timeSamples ? _source[0] : _source[1];

            return playing0 ? _source[0] : playing1 ? _source[1] : null;
        }
    }
}
