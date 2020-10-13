using System;
using UnityEngine;

namespace Sge.Sound
{
    [Serializable]
    public class AudioClipSoundCue : ISoundCue
    {
        public string Name => _name;
        public string Category => _category;
        public ISoundTrack[] Tracks => _tracks;
        public SoundCuePlayType PlayType => _playType;

        [SerializeField]
        private string _name;

        [SerializeField]
        private string _category;
        
        [SerializeField]
        private AudioClipSoundTrack[] _tracks;
        
        [SerializeField]
        private SoundCuePlayType _playType;
    }
}