using System.Collections.Generic;
using UnityEngine;

namespace Sge.Sound.Internal
{
    internal class AudioClipSoundProvider : ISoundControllerProvider
    {
        private AudioClipSoundControllerPool _pool = new AudioClipSoundControllerPool();
        private Dictionary<string, AudioClip> _audioClips;
        
        public void Setup(ISoundCueSheet sheet)
        {
            // 特にやることなし？
        }

        public ISoundController RentController(ISoundCue cue)
        {
            var controller = _pool.Rent();
            return controller;
        }

        public void ReturnController(ISoundController controller)
        {
            _pool.Return(controller as AudioClipSoundController);
        }
    }
}