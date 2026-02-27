// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Runtime.Core.Models;
using UnityEngine;

namespace AudioConductor.Runtime.Core
{
    internal sealed class CueController : ICueController
    {
        private uint _managerNumber;

        /// <inheritdoc />
        public bool IsPlaying => GetManager().IsPlaying(_managerNumber);

        /// <inheritdoc />
        public ITrackController Play(bool isForceLoop = false)
        {
            return GetManager().Play(_managerNumber, isForceLoop);
        }

        /// <inheritdoc />
        public ITrackController Play(int id, bool isForceLoop = false)
        {
            return GetManager().Play(_managerNumber, id, isForceLoop);
        }

        /// <inheritdoc />
        public ITrackController Play(string name, bool isForceLoop = false)
        {
            return GetManager().Play(_managerNumber, name, isForceLoop);
        }

        /// <inheritdoc />
        public void Pause()
        {
            GetManager().Pause(_managerNumber);
        }

        /// <inheritdoc />
        public void Resume()
        {
            GetManager().Resume(_managerNumber);
        }

        /// <inheritdoc />
        public void Stop(bool isFade)
        {
            GetManager().Stop(_managerNumber, isFade);
        }

        /// <inheritdoc />
        public int GetCategoryId()
        {
            return GetManager().GetCategoryId(_managerNumber);
        }

        /// <inheritdoc />
        public void SetVolume(float volume)
        {
            GetManager().SetVolume(_managerNumber, volume);
        }

        /// <inheritdoc />
        public void SetPitch(float pitch)
        {
            GetManager().SetPitch(_managerNumber, pitch);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GetManager().DisposeCueState(_managerNumber);
        }

        /// <inheritdoc />
        public bool HasTrack(string name)
        {
            return GetManager().HasTrack(_managerNumber, name);
        }

        public void Setup(CueSheetAsset sheetAsset, int cueIndex)
        {
            if (sheetAsset == null)
            {
                Debug.LogWarning($"{nameof(sheetAsset)} is null.");
                return;
            }

            if (cueIndex < 0 || sheetAsset.cueSheet.cueList.Count <= cueIndex)
            {
                Debug.LogWarning($"cue not found. {nameof(cueIndex)} = {cueIndex}");
                return;
            }

            _managerNumber = GetManager().CreateCueState(sheetAsset, sheetAsset.cueSheet.cueList[cueIndex]);
        }

        public void Setup(CueSheetAsset sheetAsset, string cueName)
        {
            if (sheetAsset == null)
            {
                Debug.LogWarning($"{nameof(sheetAsset)} is null.");
                return;
            }

            var cueIndex = sheetAsset.cueSheet.cueList.FindIndex(cue => cue.name == cueName);
            if (cueIndex < 0)
            {
                Debug.LogWarning($"cue not found. {nameof(cueName)} = {cueName}");
                return;
            }

            Setup(sheetAsset, cueIndex);
        }

        private static AudioConductorInternal GetManager()
        {
            return AudioConductorInternal.Instance;
        }
    }
}
