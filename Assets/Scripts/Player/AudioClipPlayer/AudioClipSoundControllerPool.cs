using System;
using Sge.Sound.Utility;
using UnityEngine;

namespace Sge.Sound
{
    public class AudioClipSoundControllerPool : ComponentPool<AudioClipSoundController>
    {
        protected override AudioClipSoundController CreateInstance()
        {
            var gameObject = new GameObject(nameof(AudioClipSoundController));
            return gameObject.AddComponent<AudioClipSoundController>();
        }
    }
}