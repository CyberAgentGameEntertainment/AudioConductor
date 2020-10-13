using System;

namespace Sge.Sound
{
    /// <summary>
    /// サウンド制御インターフェース
    /// </summary>
    public interface ISoundController
    {
        void Play();
        void Pause();
        void UnPause();
        void Stop();
        string GetCategory();

        void SetStopAction(Action onStop);
    }
}