// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using UnityEngine.Audio;

namespace AudioConductor.Runtime.Core
{
    public sealed partial class Conductor
    {
        /// <summary>
        ///     Returns information about all currently registered CueSheets.
        /// </summary>
        public List<CueSheetInfo> GetCueSheetInfos()
        {
            var list = new List<CueSheetInfo>();
            GetCueSheetInfos(list);
            return list;
        }

        /// <summary>
        ///     Fills the provided list with information about all currently registered CueSheets.
        ///     The list is cleared before filling.
        /// </summary>
        /// <param name="result">The list to fill with CueSheet information.</param>
        public void GetCueSheetInfos(List<CueSheetInfo> result)
        {
            result.Clear();
            if (result.Capacity < _cueSheets.Count)
                result.Capacity = _cueSheets.Count;
            foreach (var kv in _cueSheets)
                result.Add(new CueSheetInfo(new CueSheetHandle(kv.Key), kv.Value.Asset.cueSheet.name));
        }

        /// <summary>
        ///     Returns information about all Cues in the specified CueSheet.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <returns>Cue information list, or an empty collection if the handle is invalid.</returns>
        public List<CueInfo> GetCueInfos(CueSheetHandle sheetHandle)
        {
            var list = new List<CueInfo>();
            GetCueInfos(sheetHandle, list);
            return list;
        }

        /// <summary>
        ///     Fills the provided list with information about all Cues in the specified CueSheet.
        ///     The list is cleared before filling.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="result">The list to fill with Cue information.</param>
        public void GetCueInfos(CueSheetHandle sheetHandle, List<CueInfo> result)
        {
            result.Clear();
            if (!sheetHandle.IsValid || !_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cueList = registration.Asset.cueSheet.cueList;
            if (result.Capacity < cueList.Count)
                result.Capacity = cueList.Count;
            for (var i = 0; i < cueList.Count; i++)
                result.Add(new CueInfo(cueList[i].name, cueList[i].categoryId));
        }

        /// <summary>
        ///     Returns information about all Tracks in the specified Cue.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue.</param>
        /// <returns>Track information list, or an empty collection if the handle or cue name is invalid.</returns>
        public List<TrackInfo> GetTrackInfos(CueSheetHandle sheetHandle, string cueName)
        {
            var list = new List<TrackInfo>();
            GetTrackInfos(sheetHandle, cueName, list);
            return list;
        }

        /// <summary>
        ///     Fills the provided list with information about all Tracks in the specified Cue.
        ///     The list is cleared before filling.
        /// </summary>
        /// <param name="sheetHandle">The handle identifying the registered CueSheet.</param>
        /// <param name="cueName">The name of the cue.</param>
        /// <param name="result">The list to fill with Track information.</param>
        public void GetTrackInfos(CueSheetHandle sheetHandle, string cueName, List<TrackInfo> result)
        {
            result.Clear();
            if (!sheetHandle.IsValid || !_cueSheets.TryGetValue(sheetHandle.Id, out var registration))
                return;

            var cue = registration.FindCue(cueName);
            if (cue == null)
                return;

            var trackList = cue.trackList;
            if (result.Capacity < trackList.Count)
                result.Capacity = trackList.Count;
            for (var i = 0; i < trackList.Count; i++)
                result.Add(new TrackInfo(trackList[i].name, trackList[i].audioClip, trackList[i].isLoop,
                    trackList[i].priority));
        }

        /// <summary>
        ///     Gets the AudioMixerGroup assigned to the specified category.
        /// </summary>
        /// <param name="categoryId">The category ID.</param>
        /// <returns>The AudioMixerGroup, or null if not found.</returns>
        public AudioMixerGroup? GetAudioMixerGroup(int categoryId)
        {
            if (_categories.TryGetValue(categoryId, out var category))
                return category.audioMixerGroup;

            return null;
        }
    }
}
