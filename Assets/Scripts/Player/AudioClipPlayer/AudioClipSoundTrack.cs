using System;
using UnityEngine;

namespace Sge.Sound
{
    /// <summary>
    /// 音声情報
    /// </summary>
    [Serializable]
    public class AudioClipSoundTrack : ISoundTrack
    {
        public string Name => name;
        
        /// <summary>
        /// 再生に使用する名前
        /// </summary>
        public string name;

        /// <summary>
        /// 読み込むための情報
        /// </summary>
        public AudioClip audioClip;
        
        /// <summary>
        /// ボリューム
        /// </summary>
        public float volume;
        
        /// <summary>
        /// ピッチ
        /// </summary>
        public float pitch;
    }
}