using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace Sge.Sound
{
    /// <summary>
    /// 音声の操作(内部はPlayableGraph))
    /// </summary>
    public class AudioClipSoundController : MonoBehaviour, ISoundController
    {
        private PlayableGraph _graph;
        private ScriptPlayable<AudioClipSoundPlayable> _playable;
        private AudioClipSoundPlayable _behaviour;

        private void Awake()
        {
            var source = gameObject.AddComponent<AudioSource>();
            _graph = PlayableGraph.Create();

            _playable = ScriptPlayable<AudioClipSoundPlayable>.Create(_graph);
            _behaviour = _playable.GetBehaviour();
            _behaviour.SetOwner(_playable);

            var output = AudioPlayableOutput.Create(_graph, "", source);
            output.SetSourcePlayable(_playable);
        }

        public void SetAudioClip(AudioClip clip)
        {
            _behaviour.ResetState();
            _behaviour.SetAudioClip(clip);
            
        }

        public void Play()
        {
            _graph.Play();
            _behaviour.Restart();
        }

        public void Pause()
        {
            _playable.Pause();
        }

        public void UnPause()
        {
            _playable.Play();
        }

        public void Stop()
        {
            _behaviour.InvokeStopAction();
            _graph.Stop();
        }
        
        public string GetCategory()
        {
            return "None";
        }

        public void SetStopAction(Action onStop)
        {
            _behaviour.OnStop(onStop);
        }

        private void OnDestroy()
        {
            _graph.Destroy();
        }

    }
}
