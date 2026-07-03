// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using AudioConductor.Core;
using AudioConductor.Core.Models;
using AudioConductor.Core.Providers;
using UnityEngine;

namespace AudioConductor.Development
{
    // Dev-only WebGL loop verification scene; not part of the distributed package sample.
    public class BgmLoopTest : MonoBehaviour
    {
        // One cue per AudioClipLoadType, all sharing the same intro + partial-loop
        // settings, so seamless-loop behavior can be compared across load types in one scene.
        private const string CueSheetName = "CueSheets/BgmLoopTestCueSheet";
        private static readonly string[] CueNames = { "DecompressOnLoad", "CompressedInMemory", "Streaming" };
        private readonly Dictionary<string, PlaybackHandle> _playbacks = new();

        private Conductor? _conductor;
        private string _log = "";
        private CueSheetHandle _sheetHandle;
        private bool _sheetRegistered;

        private void Start()
        {
            var settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            // Each looping cue uses 2 sources for crossover; 3 cues at once need 6, plus margin.
            settings.managedPoolCapacity = 8;
            _conductor = new Conductor(settings, new ResourcesCueSheetProvider());
        }

        private void OnDestroy()
        {
            _conductor?.Dispose();
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 460, 700));

            GUILayout.Label("BGM Loop Test — loadType comparison (intro + partial loop)");
            GUILayout.Space(10);

            foreach (var cueName in CueNames)
            {
                var playing = _playbacks.ContainsKey(cueName);

                GUILayout.BeginHorizontal();
                GUILayout.Label(cueName + (playing ? "  [playing]" : ""), GUILayout.Width(230));
                if (GUILayout.Button(playing ? "■ Stop" : "▶ Play", GUILayout.Height(44)))
                {
                    if (playing)
                        StopCue(cueName);
                    else
                        _ = PlayCue(cueName);
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(6);
            }

            GUILayout.Space(10);
            if (GUILayout.Button("■ Stop All"))
                StopAll();

            GUILayout.Space(10);
            GUILayout.Label(_log);
            GUILayout.EndArea();
        }

        private async Task PlayCue(string cueName)
        {
            if (_conductor == null)
                return;

            if (!_sheetRegistered)
            {
                _sheetHandle = await _conductor.RegisterCueSheetAsync(CueSheetName);
                _sheetRegistered = true;
                _log = "Sheet registered.";
            }

            var handle = _conductor.Play(_sheetHandle, cueName, new PlayOptions { IsLoop = true });
            _playbacks[cueName] = handle;
            _log += "\nPlay: " + cueName;
        }

        private void StopCue(string cueName)
        {
            if (_conductor == null)
                return;

            if (!_playbacks.TryGetValue(cueName, out var handle))
                return;

            _conductor.Stop(handle);
            _playbacks.Remove(cueName);
            _log += "\nStop: " + cueName;
        }

        private void StopAll()
        {
            foreach (var cueName in new List<string>(_playbacks.Keys))
                StopCue(cueName);
        }
    }
}
