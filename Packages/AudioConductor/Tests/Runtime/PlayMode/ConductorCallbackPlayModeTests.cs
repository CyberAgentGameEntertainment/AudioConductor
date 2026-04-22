// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    internal sealed class ConductorCallbackPlayModeTests
    {
        private CueSheetAsset _cueSheetAsset = null!;
        private GameObject _listenerGo = null!;
        private AudioConductorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _listenerGo = new GameObject("AudioListener");
            _listenerGo.AddComponent<AudioListener>();
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _cueSheetAsset = ScriptableObject.CreateInstance<CueSheetAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.Destroy(_listenerGo);
            Object.Destroy(_settings);
            Object.Destroy(_cueSheetAsset);
        }

        private static AudioClip CreatePlayableClip(int samples = 4410)
        {
            var clip = AudioClip.Create("test", samples, 1, 44100, false);
            var data = new float[samples];
            for (var i = 0; i < data.Length; i++)
                data[i] = Mathf.Sin(2f * Mathf.PI * 440f * i / 44100f);
            clip.SetData(data, 0);
            return clip;
        }

        [UnityTest]
        public IEnumerator Play_WithOnEnd_InvokesAtActualEnd()
        {
            var clip = CreatePlayableClip(); // 0.1s clip
            var track = new Track { name = "track", audioClip = clip, endSample = clip.samples };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            var conductor = new Conductor(_settings);
            var sheet = conductor.RegisterCueSheet(_cueSheetAsset);

            var endCalled = false;
            conductor.Play(sheet, "cue1", new PlayOptions { OnEnd = () => endCalled = true });

            var deadline = Time.realtimeSinceStartup + 2.0f;
            yield return new WaitUntil(() => endCalled || Time.realtimeSinceStartup >= deadline);

            Assert.That(endCalled, Is.True);

            conductor.Dispose();
            Object.Destroy(clip);
        }

        [UnityTest]
        public IEnumerator Stop_WithFadeOut_InvokesOnStop_AfterRealFadeCompletes()
        {
            var clip = CreatePlayableClip(44100); // 1s clip
            var track = new Track { name = "track", audioClip = clip, endSample = clip.samples };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            var conductor = new Conductor(_settings);
            var sheet = conductor.RegisterCueSheet(_cueSheetAsset);

            var stopCalled = false;
            var handle = conductor.Play(sheet, "cue1", new PlayOptions { OnStop = () => stopCalled = true });

            yield return new WaitForSeconds(0.2f);

            conductor.Stop(handle, 0.3f);
            Assert.That(stopCalled, Is.False);

            var deadline = Time.realtimeSinceStartup + 1.0f;
            yield return new WaitUntil(() => stopCalled || Time.realtimeSinceStartup >= deadline);

            Assert.That(stopCalled, Is.True);

            conductor.Dispose();
            Object.Destroy(clip);
        }

        [UnityTest]
        public IEnumerator PlayOneShot_WithOnEnd_InvokesAtRealEnd()
        {
            var clip = CreatePlayableClip(); // 0.1s clip
            var track = new Track { name = "track", audioClip = clip, endSample = clip.samples };
            var cue = new Cue { name = "cue1" };
            cue.trackList.Add(track);
            _cueSheetAsset.cueSheet.cueList.Add(cue);

            var conductor = new Conductor(_settings);
            var sheet = conductor.RegisterCueSheet(_cueSheetAsset);

            var endCalled = false;
            conductor.PlayOneShot(sheet, "cue1", new PlayOneShotOptions { OnEnd = () => endCalled = true });

            var deadline = Time.realtimeSinceStartup + 2.0f;
            yield return new WaitUntil(() => endCalled || Time.realtimeSinceStartup >= deadline);

            Assert.That(endCalled, Is.True);

            conductor.Dispose();
            Object.Destroy(clip);
        }
    }
}
