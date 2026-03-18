// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using System.Collections.Generic;
using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Tests.Fakes;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests
{
    public partial class ConductorThrottleTests
    {
        private readonly List<Object> _created = new();
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
            foreach (var obj in _created)
                if (obj != null)
                    Object.DestroyImmediate(obj);
            _created.Clear();
            Object.DestroyImmediate(_settings);
        }

        private Conductor CreateConductor()
        {
            return new Conductor(_settings, _managedProvider, _oneShotProvider);
        }

        private AudioClip CreateClip()
        {
            var clip = AudioClip.Create("test", 44100, 1, 44100, false);
            _created.Add(clip);
            return clip;
        }

        private CueSheetAsset CreateSheetAsset(params Cue[] cues)
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            foreach (var cue in cues)
                asset.cueSheet.cueList.Add(cue);
            _created.Add(asset);
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
