// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
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
            => GetManager().Play(_managerNumber, isForceLoop);

        /// <inheritdoc />
        public ITrackController Play(int id, bool isForceLoop = false)
            => GetManager().Play(_managerNumber, id, isForceLoop);

        /// <inheritdoc />
        public ITrackController Play(string name, bool isForceLoop = false)
            => GetManager().Play(_managerNumber, name, isForceLoop);

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
            => GetManager().GetCategoryId(_managerNumber);

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

        public void Setup(CueSheetAsset sheetAsset, int cueIndex)
        {
            if (cueIndex < 0 || sheetAsset.cueSheet.cueList.Count <= cueIndex)
            {
                Debug.LogWarning($"cue not found. {nameof(cueIndex)} = {cueIndex}");
                return;
            }

            _managerNumber = GetManager().CreateCueState(sheetAsset, sheetAsset.cueSheet.cueList[cueIndex]);
        }

        public void Setup(CueSheetAsset sheetAsset, string cueName)
        {
            Setup(sheetAsset, sheetAsset.cueSheet.cueList.FindIndex(cue => cue.name == cueName));
        }

        private static AudioConductorInternal GetManager()
            => AudioConductorInternal.Instance;
    }
}
