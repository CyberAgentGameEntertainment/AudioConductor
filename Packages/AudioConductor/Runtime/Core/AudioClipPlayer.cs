// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Runtime.CompilerServices;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Runtime.Core
{
    /// <summary>
    ///     Provide functions for AudioClip playing.
    /// </summary>
    internal sealed class AudioClipPlayer : MonoBehaviour, IInternalPlayer
    {
        private const int AudioSourceNum = 2;
        private const float MinimumDuration = 1.0f;

        private const int VolumeScale = 10000;
        private static readonly string[] AudioSourceNames = { "AudioSource1", "AudioSource2" };

        private readonly AudioSource[] _source = new AudioSource[AudioSourceNum];

        private int _endSample;
        private int _frequency;

        private bool _isLoop;
        private bool _isVolumeDirty;
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
        private float _volumeMaster = 1f;

        private float _volumeRuntime;

        internal float VolumeAsset { get; private set; }

        internal float PitchInternal { get; private set; }

        public uint ActiveFadeId { get; set; }

        public bool IsFading { get; set; }

        /// <inheritdoc />
        public int ClipSamples { get; private set; }

        /// <inheritdoc />
        public int CategoryId { get; private set; }

        /// <inheritdoc />
        public bool IsPlaying
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _source[0] != null && _source[0].isPlaying || _source[1] != null && _source[1].isPlaying;
        }

        /// <inheritdoc />
        public bool IsPaused { get; private set; }

        /// <inheritdoc />
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
        {
            return ValueRangeConst.Volume.Clamp(VolumeAsset * VolumeFade * _volumeRuntime * _volumeMaster);
        }

        /// <inheritdoc />
        public float GetVolume()
        {
            return _volumeRuntime;
        }

        /// <inheritdoc />
        public void SetVolume(float volume)
        {
            _volumeRuntime = volume;
            UpdateVolume();
        }

        /// <inheritdoc />
        public float GetActualPitch()
        {
            return Mathf.Clamp(PitchInternal * _pitchExternal, -ValueRangeConst.Pitch.Max, ValueRangeConst.Pitch.Max);
        }

        /// <inheritdoc />
        public float GetPitch()
        {
            return _pitchExternal;
        }

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
            else if (_source[0] != null)
            {
                _source[0].timeSamples = sample;
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

        public void ManualUpdate(float _)
        {
            if (!IsPlaying && !IsPaused)
                return;

            UpdateVolume();

            if (IsPaused)
                return;

            if (AudioSettings.dspTime < _nextEventTime)
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
            if (_source[0] != null)
                _source[0].Stop();

            if (_source[1] != null)
                _source[1].Stop();

            _startSample = _endSample = 0;
            _volumeMaster = 1f;
            VolumeFade = 1f;
            _lastAppliedVolumeScaled = -1;
            ActiveFadeId = 0;
            IsFading = false;

            _onStop = _onEnd = null;
        }

        internal static AudioClipPlayer Create(Transform parent)
        {
            var root = new GameObject(nameof(AudioClipPlayer));
            root.transform.SetParent(parent);

            var player = root.AddComponent<AudioClipPlayer>();
            for (var i = 0; i < AudioSourceNum; i++)
            {
                var child = new GameObject(AudioSourceNames[i]);
                child.transform.SetParent(root.transform);
                player._source[i] = child.AddComponent<AudioSource>();
            }

            return player;
        }

        private void PlayLoop()
        {
            if (_source[0] == null || _source[1] == null)
                return;

            SetupPlayLoopSchedule(_nextEventTime + MinimumDuration, _loopStartSample);
        }

        private void SetupPlayLoopSchedule(double playStartTime, int startSample)
        {
            var pitch = Mathf.Abs(GetActualPitch());
            if (pitch == 0f)
                return;

            var samples = _endSample > startSample ? _endSample - startSample : startSample - _endSample;
            var duration = (float)samples / _frequency;
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
            if (pitch == 0f)
                return;

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
            _scheduledEndTime += pausedDuration;
            _source[_pausedIndex].SetScheduledEndTime(_scheduledEndTime);
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
            var volumeScaled = (int)(volume * VolumeScale);
            if (volumeScaled == _lastAppliedVolumeScaled)
                return;

            _lastAppliedVolumeScaled = volumeScaled;
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

        private void InvokeStopAction()
        {
            _onStop?.Invoke();
            _onStop = null;
        }

        private AudioSource? GetPlayingSource()
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
