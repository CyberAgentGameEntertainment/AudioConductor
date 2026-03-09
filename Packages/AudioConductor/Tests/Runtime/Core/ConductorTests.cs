// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Core.Tests
{
    public class ConductorTests
    {
        private AudioConductorSettings _settings = null!;

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
            using var conductor = new Conductor(_settings);

            var rootObject = GameObject.Find(nameof(Conductor));
            Assert.That(rootObject, Is.Not.Null);
        }

        [Test]
        public void Constructor_AttachesConductorBehaviour()
        {
            using var conductor = new Conductor(_settings);

            var rootObject = GameObject.Find(nameof(Conductor));
            Assert.That(rootObject, Is.Not.Null);
            var behaviour = rootObject.GetComponent<ConductorBehaviour>();
            Assert.That(behaviour, Is.Not.Null);
        }

        [Test]
        public void Dispose_DestroysRootGameObject()
        {
            GameObject rootObject;
            using (var conductor = new Conductor(_settings))
            {
                rootObject = GameObject.Find(nameof(Conductor));
                Assert.That(rootObject, Is.Not.Null);
            }

            // In EditMode, Object.DestroyImmediate is used, so the reference is immediately null-marked.
            Assert.That(rootObject == null, Is.True);
        }

        [Test]
        public void Dispose_DisconnectsBehaviourDelegate()
        {
            var conductor = new Conductor(_settings);
            var rootObject = GameObject.Find(nameof(Conductor));
            var behaviour = rootObject.GetComponent<ConductorBehaviour>();
            Assert.That(behaviour.Conductor, Is.Not.Null);

            conductor.Dispose();

            // After Dispose, the delegate is disconnected before the GameObject is destroyed.
            Assert.That(behaviour.Conductor, Is.Null);
        }

        [Test]
        public void GetAudioMixerGroup_WithUnknownCategoryId_ReturnsNull()
        {
            using var conductor = new Conductor(_settings);

            var mixerGroup = conductor.GetAudioMixerGroup(999);

            Assert.That(mixerGroup, Is.Null);
        }
    }
}
