using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Sge.Sound 
{
    /// <summary>
    /// キューシート
    /// </summary>
    [CreateAssetMenu(fileName = nameof(AudioClipSoundCueSheet), menuName = "Sge/Sound/" + nameof(AudioClipSoundCueSheet))]
    public class AudioClipSoundCueSheet : ScriptableObject, ISoundCueSheet
    {
        [SerializeField] 
        private AudioClipSoundCue[] _cues;
        
        [SerializeField]
        public string name;

        public string Name => name;

        public ISoundCue[] Cues => _cues;
    }
}
