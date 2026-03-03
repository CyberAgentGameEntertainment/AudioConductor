// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using NUnit.Framework;
using UnityEngine;
using CoreAudioConductor = AudioConductor.Runtime.Core.AudioConductor;
using ConductorBehaviour = AudioConductor.Runtime.Core.ConductorBehaviour;
using AudioConductorSettings = AudioConductor.Runtime.Core.Models.AudioConductorSettings;

namespace AudioConductor.Tests.Runtime.Core
{
    public class AudioConductorTests
    {
        private AudioConductorSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<AudioConductorSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Constructor_CreatesRootGameObject()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var rootObject = GameObject.Find("AudioConductor");
            Assert.That(rootObject, Is.Not.Null);
        }

        [Test]
        public void Constructor_AttachesConductorBehaviour()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var rootObject = GameObject.Find("AudioConductor");
            Assert.That(rootObject, Is.Not.Null);
            var behaviour = rootObject.GetComponent<ConductorBehaviour>();
            Assert.That(behaviour, Is.Not.Null);
        }

        [Test]
        public void Dispose_DestroysRootGameObject()
        {
            GameObject rootObject;
            using (var conductor = new CoreAudioConductor(_settings))
            {
                rootObject = GameObject.Find("AudioConductor");
                Assert.That(rootObject, Is.Not.Null);
            }

            // In EditMode, Object.DestroyImmediate is used, so the reference is immediately null-marked.
            Assert.That(rootObject == null, Is.True);
        }

        [Test]
        public void Dispose_DisconnectsBehaviourDelegate()
        {
            var conductor = new CoreAudioConductor(_settings);
            var rootObject = GameObject.Find("AudioConductor");
            var behaviour = rootObject.GetComponent<ConductorBehaviour>();
            Assert.That(behaviour.Conductor, Is.Not.Null);

            conductor.Dispose();

            // After Dispose, the delegate is disconnected before the GameObject is destroyed.
            Assert.That(behaviour.Conductor, Is.Null);
        }

        [Test]
        public void GetAudioMixerGroup_WithUnknownCategoryId_ReturnsNull()
        {
            using var conductor = new CoreAudioConductor(_settings);

            var mixerGroup = conductor.GetAudioMixerGroup(999);

            Assert.That(mixerGroup, Is.Null);
        }
    }
}
