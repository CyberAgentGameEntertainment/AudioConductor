// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Linq;
using AudioConductor.Runtime.Core;
using AudioConductor.Runtime.Core.Models;
using UnityEngine;
using UnityEngine.UI;

namespace AudioConductor.Sample
{
    public class SampleScene : MonoBehaviour
    {
        [SerializeField]
        private AudioConductorSettings _setting;

        [SerializeField]
        private CueSheetAsset _sheetAsset;

        [SerializeField]
        private Dropdown _cueNames;

        [SerializeField]
        private Dropdown _trackIds;

        [SerializeField]
        private Dropdown _trackNames;

        [SerializeField]
        private Button _playButton;

        [SerializeField]
        private Button _idPlayButton;

        [SerializeField]
        private Button _namePlayButton;

        [SerializeField]
        private Button _stopButton;

        [SerializeField]
        private Button _pauseUnPauseButton;

        [SerializeField]
        private Button _disposeControllerButton;

        private ICueController[] _controllers;

        private int _currentIndex;
        private bool _isPaused;

        private void Awake()
        {
            AudioConductorInterface.Setup(_setting, OnCueSheetUnused);

            var cueList = _sheetAsset.cueSheet.cueList;

            _controllers = new ICueController[cueList.Count];

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
            foreach (var controller in _controllers)
                controller?.Dispose();
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
            _controllers[_currentIndex] ??= AudioConductorInterface.CreateController(_sheetAsset, _currentIndex);
            _controllers[_currentIndex].Play();
        }

        private void Play(string trackName)
        {
            _isPaused = false;
            _controllers[_currentIndex] ??= AudioConductorInterface.CreateController(_sheetAsset, _currentIndex);
            _controllers[_currentIndex].Play(trackName);
        }

        private void Play(int index)
        {
            _isPaused = false;
            _controllers[_currentIndex] ??= AudioConductorInterface.CreateController(_sheetAsset, _currentIndex);
            _controllers[_currentIndex].Play(index);
        }

        private void Stop()
        {
            _isPaused = false;
            if (_controllers[_currentIndex] == null)
                return;

            _controllers[_currentIndex].Stop(true);
        }

        private void PauseOrResume()
        {
            if (_controllers[_currentIndex] == null)
                return;

            if (!_isPaused)
            {
                _controllers[_currentIndex].Pause();
                _isPaused = true;
            }
            else
            {
                _controllers[_currentIndex].Resume();
                _isPaused = false;
            }
        }

        private void DisposeController()
        {
            foreach (var controller in _controllers)
                controller?.Dispose();
        }

        private static void OnCueSheetUnused(CueSheetAsset sheetAsset)
        {
            Resources.UnloadAsset(sheetAsset);
        }
    }
}
