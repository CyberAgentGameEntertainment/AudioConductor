// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using AudioConductor.Tests.Runtime.Core.Fakes;
using NUnit.Framework;
using UnityEngine;
using CoreAudioConductor = AudioConductor.Runtime.Core.AudioConductor;
using AudioConductorSettings = AudioConductor.Runtime.Core.Models.AudioConductorSettings;
using Object = UnityEngine.Object;

namespace AudioConductor.Tests.Runtime.Core
{
    public partial class AudioConductorThrottleTests
    {
        private FakePlayerProvider _managedProvider = null!;
        private FakePlayerProvider _oneShotProvider = null!;
        private AudioConductorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
            _managedProvider = new FakePlayerProvider();
            _oneShotProvider = new FakePlayerProvider();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        private CoreAudioConductor CreateConductor()
        {
            return new CoreAudioConductor(_settings, _managedProvider, _oneShotProvider);
        }

        private static AudioClip CreateClip()
        {
            return AudioClip.Create("test", 44100, 1, 44100, false);
        }

        private static CueSheetAsset CreateSheetAsset(params Cue[] cues)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            foreach (var cue in cues)
                asset.cueSheet.cueList.Add(cue);
            return asset;
        }

        private static Cue CreateCue(string name, ThrottleType throttleType = ThrottleType.FirstComeFirstServed,
            int throttleLimit = 0, int categoryId = 0)
        {
            return new Cue
            {
                name = name,
                throttleType = throttleType,
                throttleLimit = throttleLimit,
                categoryId = categoryId
            };
        }

        private static Track CreateTrack(AudioClip clip, int priority = 0)
        {
            return new Track
            {
                name = "track",
                audioClip = clip,
                priority = priority
            };
        }
    }
}
