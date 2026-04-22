// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System;
using System.Runtime.CompilerServices;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Shared;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioConductor.Core
{
    internal sealed class AudioClipPlayer : IFadeable
    {
        private const int SourceNum = 2;
        private const float LoopLookaheadDuration = 1.0f;
        private const int VolumeScale = 10000;
        private readonly IDspClock _dspClock;
        private readonly IAudioPlayerLifecycle _lifecycle;

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

        internal AudioClipPlayer(IAudioSourceWrapper[] sources, IDspClock dspClock,
            IAudioPlayerLifecycle lifecycle)
        {
            _sources = sources;
            _dspClock = dspClock;
            _lifecycle = lifecycle;
        }

        internal float VolumeAsset { get; private set; }
        internal float PitchInternal { get; private set; }

        private bool IsPaused { get; set; }
        public int ClipSamples { get; private set; }
        public int CategoryId { get; private set; }

        public PlayerState State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsPaused ? PlayerState.Paused
                : _sources[0].IsPlaying || _sources[1].IsPlaying ? PlayerState.Playing
                : PlayerState.Stopped;
        }

        public uint ActiveFadeId { get; set; }
        public FadeState FadeState { get; set; }

        public float VolumeFade { get; private set; } = 1f;

        public void SetVolumeFade(float fade)
        {
            VolumeFade = fade;
            UpdateVolume();
        }

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

            ResetState();

            _isLoop = isLoop;

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

            _volumeRuntime = 1f;
            SetPitchInternal(pitch);
            VolumeAsset = volume;
            UpdateVolume();

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
            SchedulePlayback(_dspClock.DspTime + delay, _startSample);
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
            ShiftScheduleByPauseDuration(pausedDuration);

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

        public void SetStopAction(Action onStop)
        {
            _onStop = onStop;
        }

        public void SetEndAction(Action onEnd)
        {
            _onEnd = onEnd;
        }

        internal void ClearEndAction()
        {
            _onEnd = null;
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

            RecalculateScheduledEndTime();
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
                ScheduleNextLoop();
            }
            else
            {
                var onEnd = _onEnd;
                _onEnd = null;
                if (onEnd != null)
                    try
                    {
                        onEnd();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }

                Stop();
            }
        }

        internal static AudioClipPlayer Create(Transform parent)
        {
            var root = new GameObject(nameof(AudioClipPlayer));
            root.transform.SetParent(parent);
            var sources = new IAudioSourceWrapper[SourceNum];
            for (var i = 0; i < SourceNum; i++)
                sources[i] = new AudioSourceWrapper(root.AddComponent<AudioSource>());
            return new AudioClipPlayer(sources, new DspClock(), new GameObjectLifecycle(root));
        }

        internal void SetActive(bool active)
        {
            _lifecycle.SetActive(active);
        }

        internal void Destroy()
        {
            _lifecycle.Destroy();
        }

        public void ResetState()
        {
            _sources[0].Stop();
            _sources[1].Stop();

            _isLoop = false;
            _startSample = _endSample = 0;
            _volumeCategory = 1f;
            _volumeMaster = 1f;
            VolumeFade = 1f;
            _lastAppliedVolumeScaled = -1;
            ActiveFadeId = 0;
            FadeState = FadeState.None;
            _isPlaybackActive = false;
            _pitchExternal = 1f;
            _nextPlayAudioSourceIndex = 0;
            IsPaused = false;
            _nextEventTime = 0;
            _pausedIndex = 0;
            _pauseStartTime = 0;
            _pauseEndTime = 0;
            _scheduledEndTime = 0;

            _onStop = _onEnd = null;
        }

        private void ScheduleNextLoop()
        {
            SchedulePlayback(_scheduledEndTime, _loopStartSample);
        }

        private void SchedulePlayback(double playStartTime, int startSample)
        {
            var pitch = Mathf.Abs(GetActualPitch());
            if (pitch == 0f)
                return;

            // In loop mode, zero-length region would cause _nextEventTime to never advance,
            // resulting in ScheduleNextLoop being called every ManualUpdate frame indefinitely.
            if (_isLoop && _endSample == startSample)
            {
                _nextEventTime = double.MaxValue;
                return;
            }

            _scheduledEndTime = CalculateScheduledEndTime(playStartTime, startSample, pitch);

            _sources[_nextPlayAudioSourceIndex].TimeSamples = startSample;
            _sources[_nextPlayAudioSourceIndex].PlayScheduled(playStartTime);
            _sources[_nextPlayAudioSourceIndex].SetScheduledEndTime(_scheduledEndTime);

            UpdateNextEventTime();

            FlipNextPlayAudioSourceIndex();
        }

        private void RecalculateScheduledEndTime()
        {
            var source = GetPlayingSource();
            if (source == null)
                return;

            var pitch = Mathf.Abs(GetActualPitch());
            if (pitch == 0f)
                return;

            var nowSample = source.TimeSamples;
            _scheduledEndTime = CalculateScheduledEndTime(_dspClock.DspTime, nowSample, pitch);

            source.SetScheduledEndTime(_scheduledEndTime);
            UpdateNextEventTime();
        }

        private void ShiftScheduleByPauseDuration(double pausedDuration)
        {
            _scheduledEndTime += pausedDuration;
            _sources[_pausedIndex].SetScheduledEndTime(_scheduledEndTime);
            _nextEventTime += pausedDuration;
        }

        private void UpdateNextEventTime()
        {
            var lookaheadBuffer = _isLoop ? LoopLookaheadDuration : 0;
            _nextEventTime = _scheduledEndTime - lookaheadBuffer;
        }

        private double CalculateScheduledEndTime(double baseTime, int currentSample, float absPitch)
        {
            var samples = _endSample > currentSample
                ? _endSample - currentSample
                : currentSample - _endSample;
            var duration = (float)samples / _frequency;
            return baseTime + duration / absPitch;
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
            RecalculateScheduledEndTime();

            if (_isLoop)
                _sources[1].Pitch = pitch;
        }

        private void InvokeStopAction()
        {
            var onStop = _onStop;
            _onStop = null;
            if (onStop == null) return;
            try
            {
                onStop();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private IAudioSourceWrapper? GetPlayingSource()
        {
            var playing0 = _sources[0].IsPlaying;
            var playing1 = _sources[1].IsPlaying;

            // Both AudioSources may be playing simultaneously near a loop boundary.
            // Prefer the source with higher TimeSamples (the outgoing source) to maintain
            // continuity: the incoming source starts from loopStartSample and should not
            // become current until the outgoing source finishes.
            if (playing0 && playing1)
                return _sources[0].TimeSamples > _sources[1].TimeSamples ? _sources[0] : _sources[1];

            return playing0 ? _sources[0] : playing1 ? _sources[1] : null;
        }
    }
}
