// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Runtime.Core.Shared;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed partial class AudioConductorInternal
    {
        private static AudioConductorInternal _instance;
        private readonly AudioClipPlayerProvider _provider = new();
        private readonly Dictionary<uint, CueSheetAsset> _cueSheetAssets = new();
        private readonly Dictionary<int, Category> _categories = new();
        private readonly Dictionary<uint, CueState> _cueStates = new();
        private readonly List<TrackController> _controllerList = new();
        private readonly List<TrackController> _tempControllerList = new();
        private readonly List<AudioClipPlayer> _unmanagedPlayerList = new();
        private readonly List<FadeState> _fadeStateList = new();
        private readonly List<FadeState> _tempFadeStateList = new();

        private uint _sheetStateCounter;
        private uint _cueStateCounter;
        private uint _controllerCounter;
        private Action<CueSheetAsset> _callback;

        private AudioConductorInternal()
        {
        }

        public static AudioConductorInternal Instance => _instance ??= new AudioConductorInternal();

        public AudioConductorSettings AudioConductorSettings { get; private set; }

        public void Setup(AudioConductorSettings settings, Action<CueSheetAsset> callback)
        {
            Reset();
            AudioConductorSettings = settings;
            _callback = callback;

            foreach (var category in AudioConductorSettings.categoryList)
                _categories.Add(category.id, category);
        }

        internal void Update(float deltaTime)
        {
            UpdateController(deltaTime);
            UpdateFade(deltaTime);
        }

        private void UpdateController(float deltaTime)
        {
            _tempControllerList.Clear();
            _tempControllerList.AddRange(_controllerList);
            foreach (var controller in _tempControllerList)
                controller.Player.ManualUpdate(deltaTime);

            foreach (var player in _unmanagedPlayerList)
                player.ManualUpdate(deltaTime);
        }

        private void UpdateFade(float deltaTime)
        {
            if (_fadeStateList.Count == 0)
                return;

            _tempFadeStateList.Clear();
            foreach (var fadeState in _fadeStateList)
                if (fadeState.Elapsed(deltaTime))
                    _tempFadeStateList.Add(fadeState);

            foreach (var fadeState in _tempFadeStateList)
                _fadeStateList.Remove(fadeState);
        }

        public uint CreateCueState(CueSheetAsset sheetAsset, Cue cue)
        {
            var sheetStateManagerNumber = SetupCueSheet(sheetAsset);

            if (_cueStateCounter >= uint.MaxValue)
                _cueStateCounter = 0;

            _cueStateCounter++;
            _cueStates.Add(_cueStateCounter, new CueState(sheetStateManagerNumber, cue));

            return _cueStateCounter;
        }

        public void DisposeCueState(uint manageNumber)
        {
            if (_cueStates.TryGetValue(manageNumber, out var state) == false)
                return;

            Stop(manageNumber, true);
            _cueStates.Remove(manageNumber);

            foreach (var cueState in _cueStates.Values)
                if (cueState.CueSheetManageNumber == state.CueSheetManageNumber)
                    return;

            var ownerCueSheetAsset = _cueSheetAssets[state.CueSheetManageNumber];
            _cueSheetAssets.Remove(state.CueSheetManageNumber);

            foreach (var cueSheetAsset in _cueSheetAssets.Values)
                if (cueSheetAsset == ownerCueSheetAsset)
                    return;

            _callback?.Invoke(ownerCueSheetAsset);
        }

        public bool HasTrack(uint manageNumber, string name)
        {
            var state = GetState(manageNumber);
            return state?.GetTrack(name) != null;
        }

        private uint SetupCueSheet(CueSheetAsset sheetAsset)
        {
            foreach (var pair in _cueSheetAssets)
                if (pair.Value == sheetAsset)
                    return pair.Key;

            if (_sheetStateCounter >= uint.MaxValue)
                _sheetStateCounter = 0;

            _sheetStateCounter++;
            _cueSheetAssets.Add(_sheetStateCounter, sheetAsset);

            return _sheetStateCounter;
        }

        public AudioClipPlayer RentPlayer(bool isUnmanaged)
        {
            var player = _provider.Rent();
            if (isUnmanaged)
                _unmanagedPlayerList.Add(player);
            return player;
        }

        public void ReturnPlayer(AudioClipPlayer player)
        {
            _unmanagedPlayerList.Remove(player);
            _provider.Return(player);
        }

        public bool IsCueSheetUsed(CueSheetAsset sheetAsset)
        {
            foreach (var cueSheetAsset in _cueSheetAssets.Values)
                if (cueSheetAsset == sheetAsset)
                    return true;

            return false;
        }

        private TrackController SetupController(uint manageNumber, CueState state)
        {
            if (_controllerCounter >= uint.MaxValue)
                _controllerCounter = 0;

            _controllerCounter++;

            var soundController = RentPlayer(false);
            var controller = new TrackController(_controllerCounter,
                                                 soundController,
                                                 state.CueSheetManageNumber,
                                                 manageNumber);
            _controllerList.Add(controller);

            return controller;
        }

        private TrackController Play(uint manageNumber, CueState state, Track track, bool isForceLoop)
        {
            if (CanPlay(manageNumber, state, track) == false)
                return null;

            var controller = SetupController(manageNumber, state);
            var category = GetCategory(state.Cue.categoryId);
            var sheet = _cueSheetAssets[controller.CueSheetManageNumber].cueSheet;
            var volume = Calculator.CalcVolume(sheet, state.Cue, track);
            var pitch = Calculator.CalcPitch(sheet, state.Cue, track);
            var hasFade = track.fadeTime > 0;
            var isLoop = isForceLoop || track.isLoop;
            controller.Setup(category, track, hasFade ? 0f : volume, pitch, isLoop);
            controller.Player.AddStopAction(() =>
            {
                _controllerList.Remove(controller);
                ReturnPlayer(controller.Player);
                controller.ReleasePlayer();
            });
            controller.Player.Play();

            if (track.fadeTime > 0)
            {
                var fadeState = new FadeState(controller.Player);
                fadeState.Setup(controller.Player.VolumeInternal, volume, track.fadeTime, false);
                _fadeStateList.Add(fadeState);
            }

            return controller;
        }

        public TrackController Play(uint manageNumber, string name, bool isForceLoop)
        {
            var state = GetState(manageNumber);
            if (state == null)
                return null;

            var track = state.GetTrack(name);
            return track == null ? null : Play(manageNumber, state, track, isForceLoop);
        }

        public TrackController Play(uint manageNumber, int id, bool isForceLoop)
        {
            var state = GetState(manageNumber);
            if (state == null)
                return null;

            var track = state.GetTrack(id);
            return track == null ? null : Play(manageNumber, state, track, isForceLoop);
        }

        public TrackController Play(uint manageNumber, bool isForceLoop)
        {
            var state = GetState(manageNumber);
            if (state == null)
                return null;

            var track = state.NextTrack();
            return Play(manageNumber, state, track, isForceLoop);
        }

        private bool CanPlay(uint manageNumber, CueState state, Track track)
        {
            if (track.audioClip == null)
                return false;

            if (_controllerList.Count == 0)
                return true;

            var minPriority = _controllerList[0].Priority;
            foreach (var controller in _controllerList)
                if (minPriority > controller.Priority)
                    minPriority = controller.Priority;

            _tempControllerList.Clear();
            foreach (var controller in _controllerList)
                if (controller.Priority == minPriority)
                    _tempControllerList.Add(controller);

            for (var i = 0; i < (int)LimitCheckType.Count; ++i)
            {
                ThrottleType throttleType = default;
                int throttleLimit = default;
                switch ((LimitCheckType)i)
                {
                    case LimitCheckType.Cue:
                        throttleType = state.Cue.throttleType;
                        throttleLimit = state.Cue.throttleLimit;
                        break;
                    case LimitCheckType.CueSheet:
                        var cueSheet = _cueSheetAssets[state.CueSheetManageNumber].cueSheet;
                        throttleType = cueSheet.throttleType;
                        throttleLimit = cueSheet.throttleLimit;
                        break;
                    case LimitCheckType.Category:
                        var category = GetCategory(state.Cue.categoryId);
                        if (category == null)
                            break;

                        throttleType = category.throttleType;
                        throttleLimit = category.throttleLimit;
                        break;
                    case LimitCheckType.Global:
                        throttleType = AudioConductorSettings.throttleType;
                        throttleLimit = AudioConductorSettings.throttleLimit;
                        break;
                }

                if (throttleLimit <= 0)
                    continue;

                var playNum = (LimitCheckType)i switch
                {
                    LimitCheckType.Cue      => GetPlayNumFromCueNumber(manageNumber),
                    LimitCheckType.CueSheet => GetPlayNumFromSheetNumber(state.CueSheetManageNumber),
                    LimitCheckType.Category => GetPlayNumFromCategory(state.Cue.categoryId),
                    LimitCheckType.Global   => _controllerList.Count,
                    _                       => 0
                };

                if (playNum < throttleLimit)
                    continue;

                if (minPriority > track.priority)
                    return false;

                if (minPriority < track.priority)
                    throttleType = ThrottleType.PriorityOrder; // To standardize process

                switch (throttleType)
                {
                    case ThrottleType.PriorityOrder:
                        var candidateController = (LimitCheckType)i switch
                        {
                            LimitCheckType.Cue
                                => FirstControllerFromCueNumber(manageNumber, _tempControllerList),
                            LimitCheckType.CueSheet
                                => FirstControllerFromSheetNumber(state.CueSheetManageNumber, _tempControllerList),
                            LimitCheckType.Category
                                => FirstControllerFromCategory(state.Cue.categoryId, _tempControllerList),
                            LimitCheckType.Global
                                => _tempControllerList[0],
                            _
                                => default
                        };

                        if (candidateController?.Player != null)
                            candidateController.Stop(true);

                        break;
                    case ThrottleType.FirstComeFirstServed:
                        return false;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
        }

        internal void StopController(ITrackController controller, bool isFade)
        {
            var state = controller as TrackController;
            if (state == null)
                return;

            _controllerList.Remove(state);

            if (state.Player == null)
                return;

            if (isFade == false || state.Track.fadeTime <= 0f)
            {
                state.Player.Stop();
                return;
            }

            var fadeState = new FadeState(state.Player);
            fadeState.Setup(state.Player.VolumeInternal, 0f, state.Track.fadeTime, true);
            _fadeStateList.Add(fadeState);
        }

        public void Pause(uint manageNumber)
        {
            var state = GetState(manageNumber);
            if (state == null)
                return;

            ControllerAction(manageNumber, controller => controller.Player.Pause());
        }

        public void Resume(uint manageNumber)
        {
            ControllerAction(manageNumber, controller => controller.Player.Resume());
        }

        public void Stop(uint manageNumber, bool isFade)
        {
            ControllerAction(manageNumber, controller => controller.Stop(isFade));
        }

        public void StopAll(bool isFade)
        {
            ControllerAction(controller => controller.Stop(isFade));
        }

        public void SetVolume(uint manageNumber, float volume)
        {
            ControllerAction(manageNumber, controller => controller.Player.SetVolume(volume));
        }

        public void SetPitch(uint manageNumber, float pitch)
        {
            ControllerAction(manageNumber, controller => controller.Player.SetPitch(pitch));
        }

        public bool IsPlaying(uint manageNumber)
        {
            _tempControllerList.Clear();
            foreach (var controller in _controllerList)
                if (controller.CueManageNumber == manageNumber)
                    _tempControllerList.Add(controller);

            foreach (var controller in _tempControllerList)
                if (controller.Player.IsPlaying)
                    return true;

            return false;
        }

        public int GetCategoryId(uint manageNumber)
        {
            var state = GetState(manageNumber);
            return state?.Cue.categoryId ?? -1;
        }

        private CueState GetState(uint manageNumber)
        {
            if (_cueStates.TryGetValue(manageNumber, out var state) == false)
            {
                Debug.LogWarning("Manage Number Not Found");
                return null;
            }

            return state;
        }

        private int GetPlayNumFromCategory(int category)
        {
            var num = 0;
            foreach (var sheetState in _cueStates)
            {
                if (sheetState.Value.Cue.categoryId != category)
                    continue;

                foreach (var controller in _controllerList)
                    if (controller.CueManageNumber == sheetState.Key)
                        num++;
            }

            return num;
        }

        private int GetPlayNumFromSheetNumber(uint manageNumber)
        {
            var num = 0;
            foreach (var controller in _controllerList)
                if (controller.CueSheetManageNumber == manageNumber)
                    num++;

            return num;
        }

        private int GetPlayNumFromCueNumber(uint manageNumber)
        {
            var num = 0;
            foreach (var controller in _controllerList)
                if (controller.CueManageNumber == manageNumber)
                    num++;

            return num;
        }

        private static TrackController FirstControllerFromCategory(int categoryId,
                                                                   IReadOnlyList<TrackController> controllerList)
        {
            foreach (var controller in controllerList)
                if (controller.Player.CategoryId == categoryId)
                    return controller;

            return default;
        }

        private static TrackController FirstControllerFromSheetNumber(uint manageNumber,
                                                                      IReadOnlyList<TrackController> controllerList)
        {
            foreach (var controller in controllerList)
                if (controller.CueSheetManageNumber == manageNumber)
                    return controller;

            return default;
        }

        private static TrackController FirstControllerFromCueNumber(uint manageNumber,
                                                                    IReadOnlyList<TrackController> controllerList)
        {
            foreach (var controller in controllerList)
                if (controller.CueManageNumber == manageNumber)
                    return controller;

            return default;
        }

        private void ControllerAction(uint manageNumber, Action<TrackController> action)
        {
            _tempControllerList.Clear();
            foreach (var controller in _controllerList)
                if (controller.CueManageNumber == manageNumber)
                    _tempControllerList.Add(controller);

            foreach (var controller in _tempControllerList)
                action.Invoke(controller);
        }

        private void ControllerAction(Action<TrackController> action)
        {
            _tempControllerList.Clear();
            _tempControllerList.AddRange(_controllerList);

            foreach (var controller in _tempControllerList)
                action.Invoke(controller);
        }

        private Category GetCategory(int categoryId)
        {
            if (_categories.TryGetValue(categoryId, out var category))
                return category;

            return null;
        }

        internal void Reset()
        {
            StopAll(false);

            _callback = null;
            _cueSheetAssets.Clear();
            _categories.Clear();
            _cueStates.Clear();
            _controllerList.Clear();
            _unmanagedPlayerList.Clear();
        }

        private enum LimitCheckType
        {
            Cue,
            CueSheet,
            Category,
            Global,

            Count
        }
    }
}
