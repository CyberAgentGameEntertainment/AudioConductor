using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace Sge.Sound
{
    /// <summary>
    /// 音声のPlayable
    /// - 部分再生機能
    /// - ループ機能
    /// </summary>
    public class AudioClipSoundPlayable : PlayableBehaviour
    {
        private Playable _source;
        private Playable _owner;
        private float _duration;

        private Action _onEnd;
        private Action _onStop;
        private bool _isLoop;
        private float _currentTime;
        private float _startTime;
        private float _endTime;

        /// <summary>
        /// Playback Time
        /// </summary>
        public float Time
        {
            get => _currentTime;
            internal set
            {
                _source.SetTime(value);
                _owner.SetTime(value);
                _currentTime = value;
            }
        }

        /// <summary>
        /// 自身のPlayableを設定
        /// </summary>
        /// <param name="owner">自身のPlayable</param>
        public void SetOwner(Playable owner) => _owner = owner;

        /// <summary>
        /// InputのAudioClipを設定
        /// </summary>
        /// <param name="clip">接続したいAudioClip</param>
        public void SetAudioClip(AudioClip clip)
        {
            DestroySource();

            _source = AudioClipPlayable.Create(_owner.GetGraph(), clip, false);
            _duration = clip.length;

            if (_owner.GetInputCount() != 1) _owner.SetInputCount(1);
            _owner.ConnectInput(0, _source, 0, 1);
        }

        /// <summary>
        /// ループ設定
        /// </summary>
        /// <param name="isLoop">ループする</param>
        public void SetLoop(bool isLoop) => _isLoop = isLoop;

        /// <summary>
        /// 部分再生の時間設定(ABパーツ)
        /// </summary>
        /// <param name="startTime">開始時間</param>
        /// <param name="endTime">終了時間</param>
        public void SetFragmentTime(float startTime, float endTime)
        {
            _startTime = Mathf.Max(startTime, 0);
            _endTime = Mathf.Min(endTime, _duration);
        }

        /// <summary>
        /// 再生終了のコールバック
        /// (単発再生終了で一回呼ばれるか、ループの場合は毎回呼ばれる)
        /// </summary>
        /// <param name="onEnd">コールバック</param>
        public void OnEnd(Action onEnd) => _onEnd = onEnd;

        /// <summary>
        /// 停止のコールバック
        /// (途中で停止されるか、再生終了で必ず一回呼ばれる)
        /// </summary>
        /// <param name="onEnd">コールバック</param>
        public void OnStop(Action onStop) => _onStop = onStop;

        /// <summary>
        /// 全部のコールバックをクリア
        /// </summary>
        public void ClearAllListener()
        {
            _onEnd = _onStop = null;
        }

        private void DestroySource()
        {
            if (_source.IsNull() == false)
            {
                _owner.DisconnectInput(0);
                _source.Destroy();
                _source = Playable.Null;
            }
        }

        /// <summary>
        /// 全部の内部ステートをリセットする
        /// </summary>
        public void ResetState()
        {
            DestroySource();
            ClearAllListener();
            _isLoop = false;
            _currentTime = 0;
            _duration = _startTime = _endTime = 0;
        }

        /// <summary>
        /// 再生リスタート
        /// </summary>
        public void Restart()
        {
            Time = _startTime;
        }

        /// <summary>
        /// StopActionを実行する
        /// OnGraphStopは非同期のため、停止のコールバックをすぐ呼べるように
        /// </summary>
        public void InvokeStopAction()
        {
            _onStop?.Invoke();
            _onStop = null;
        }

        public override void PrepareFrame(Playable owner, FrameData info)
        {
            // 0だと最後まで再生する
            var endTime = _endTime == 0 ? _duration : _endTime;

            _currentTime = (float)owner.GetTime();

            if (_currentTime >= endTime)
            {
                _onEnd?.Invoke();

                if (_isLoop == false)
                {
                    InvokeStopAction();
                    owner.GetGraph().Stop();
                    return;
                }

                Restart();
            }
        }
    }
}
