// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Core;
using AudioConductor.Core.Models;
using UnityEngine;
using UnityEngine.UI;

namespace AudioConductor.Sample
{
    public class SampleScene : MonoBehaviour
    {
        [SerializeField] private AudioConductorSettings _setting;

        [SerializeField] private CueSheetAsset _sheetAsset;

        [SerializeField] private Dropdown _cueNames;

        [SerializeField] private Dropdown _trackIds;

        [SerializeField] private Dropdown _trackNames;

        [SerializeField] private Button _playButton;

        [SerializeField] private Button _idPlayButton;

        [SerializeField] private Button _namePlayButton;

        [SerializeField] private Button _stopButton;

        [SerializeField] private Button _pauseUnPauseButton;

        [SerializeField] private Button _disposeControllerButton;

        private Conductor _conductor;
        private CueSheetHandle _cueSheetHandle;

        private int _currentIndex;
        private bool _isPaused;
        private PlaybackHandle _playbackHandle;

        private void Awake()
        {
            _conductor = new Conductor(_setting);
            _cueSheetHandle = _conductor.RegisterCueSheet(_sheetAsset);

            var cueList = _sheetAsset.cueSheet.cueList;

            _cueNames.options.AddRange(cueList.Select(cue => new Dropdown.OptionData(cue.name)));
            _cueNames.onValueChanged.AddListener(SelectIndex);
            _playButton.onClick.AddListener(Play);
            _idPlayButton.onClick.AddListener(() => { Play(_trackIds.value); });
            _namePlayButton.onClick.AddListener(() =>
            {
                var cue = cueList[_currentIndex];
                Play(cue.trackList[_trackIds.value].name);
            });

            _stopButton.onClick.AddListener(Stop);
            _pauseUnPauseButton.onClick.AddListener(PauseOrResume);
            _disposeControllerButton.onClick.AddListener(DisposeController);

            SelectIndex(0);
        }

        private void OnDestroy()
        {
            _conductor?.Dispose();
        }

        private void SelectIndex(int index)
        {
            _currentIndex = index;
            var cue = _sheetAsset.cueSheet.cueList[index];
            _trackIds.options.Clear();
            _trackIds.options.AddRange(Enumerable.Range(0, cue.trackList.Count)
                .Select(i => new Dropdown.OptionData(i.ToString())));
            _trackIds.value = 0;
            _trackIds.RefreshShownValue();
            _trackNames.options.Clear();
            _trackNames.options.AddRange(cue.trackList.Select(track => new Dropdown.OptionData(track.name)));
            _trackNames.value = 0;
            _trackNames.RefreshShownValue();
        }

        private void Play()
        {
            _isPaused = false;
            var cueName = _sheetAsset.cueSheet.cueList[_currentIndex].name;
            _playbackHandle = _conductor.Play(_cueSheetHandle, cueName);
        }

        private void Play(string trackName)
        {
            _isPaused = false;
            var cueName = _sheetAsset.cueSheet.cueList[_currentIndex].name;
            _playbackHandle = _conductor.Play(_cueSheetHandle, cueName, new PlayOptions { TrackName = trackName });
        }

        private void Play(int index)
        {
            _isPaused = false;
            var cueName = _sheetAsset.cueSheet.cueList[_currentIndex].name;
            _playbackHandle = _conductor.Play(_cueSheetHandle, cueName, new PlayOptions { TrackIndex = index });
        }

        private void Stop()
        {
            _isPaused = false;
            _conductor.Stop(_playbackHandle);
        }

        private void PauseOrResume()
        {
            if (!_isPaused)
            {
                _conductor.Pause(_playbackHandle);
                _isPaused = true;
            }
            else
            {
                _conductor.Resume(_playbackHandle);
                _isPaused = false;
            }
        }

        private void DisposeController()
        {
            _conductor.UnregisterCueSheet(_cueSheetHandle);
        }
    }
}
