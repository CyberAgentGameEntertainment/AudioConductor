// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Foundation.TinyRx;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal sealed class ObservableCueSheetTests
    {
        private static readonly string[] TestNameSource =
        {
            Utility.RandomString,
            Utility.RandomString,
            Utility.RandomString
        };

        [Test]
        public void CueSheetId()
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(observableCueSheet.Id, Is.EqualTo(cueSheet.Id));
        }

        [Test]
        [TestCaseSource(nameof(TestNameSource))]
        public void ChangeName(string testValue)
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(cueSheet.name, Is.EqualTo(default(string)));
            Assert.That(observableCueSheet.Name, Is.EqualTo(cueSheet.name));

            using (observableCueSheet.NameObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(testValue)); }))
            {
                observableCueSheet.Name = testValue;

                Assert.That(cueSheet.name, Is.EqualTo(testValue));
                Assert.That(observableCueSheet.Name, Is.EqualTo(cueSheet.name));
            }
        }

        [Test]
        public void ChangeThrottleType([Values] ThrottleType testValue)
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(cueSheet.throttleType, Is.EqualTo(default(ThrottleType)));
            Assert.That(observableCueSheet.ThrottleType, Is.EqualTo(cueSheet.throttleType));

            using (observableCueSheet.ThrottleTypeObservable.Skip(1)
                                     .Subscribe(v => { Assert.That(v, Is.EqualTo(testValue)); }))
            {
                observableCueSheet.ThrottleType = testValue;

                Assert.That(cueSheet.throttleType, Is.EqualTo(testValue));
                Assert.That(observableCueSheet.ThrottleType, Is.EqualTo(cueSheet.throttleType));
            }
        }

        [Test]
        public void ChangeThrottleLimit([Random(3)] int testValue)
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(cueSheet.throttleLimit, Is.EqualTo(default(int)));
            Assert.That(observableCueSheet.ThrottleLimit, Is.EqualTo(cueSheet.throttleLimit));

            using (observableCueSheet.ThrottleLimitObservable.Skip(1)
                                     .Subscribe(v => { Assert.That(v, Is.EqualTo(testValue)); }))
            {
                observableCueSheet.ThrottleLimit = testValue;

                Assert.That(cueSheet.throttleLimit, Is.EqualTo(testValue));
                Assert.That(observableCueSheet.ThrottleLimit, Is.EqualTo(cueSheet.throttleLimit));
            }
        }

        [Test]
        public void ChangeVolume([Random(3)] float testValue)
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(cueSheet.volume, Is.EqualTo(1));
            Assert.That(observableCueSheet.Volume, Is.EqualTo(cueSheet.volume));

            using (observableCueSheet.VolumeObservable.Skip(1)
                                     .Subscribe(v => { Assert.That(v, Is.EqualTo(testValue)); }))
            {
                observableCueSheet.Volume = testValue;

                Assert.That(cueSheet.volume, Is.EqualTo(testValue));
                Assert.That(observableCueSheet.Volume, Is.EqualTo(cueSheet.volume));
            }
        }

        [Test]
        public void ChangePitch([Random(3)] float testValue)
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(cueSheet.pitch, Is.EqualTo(1));
            Assert.That(observableCueSheet.Pitch, Is.EqualTo(cueSheet.pitch));

            using (observableCueSheet.PitchObservable.Skip(1)
                                     .Subscribe(v => { Assert.That(v, Is.EqualTo(testValue)); }))
            {
                observableCueSheet.Pitch = testValue;

                Assert.That(cueSheet.pitch, Is.EqualTo(testValue));
                Assert.That(observableCueSheet.Pitch, Is.EqualTo(cueSheet.pitch));
            }
        }

        [Test]
        public void ChangePitchInvert([Values] bool testValue)
        {
            var cueSheet = new CueSheet();
            var observableCueSheet = new ObservableCueSheet(cueSheet);

            Assert.That(cueSheet.pitchInvert, Is.EqualTo(default(bool)));
            Assert.That(observableCueSheet.PitchInvert, Is.EqualTo(cueSheet.pitchInvert));

            using (observableCueSheet.PitchInvertObservable.Skip(1)
                                     .Subscribe(v => { Assert.That(v, Is.EqualTo(testValue)); }))
            {
                observableCueSheet.PitchInvert = testValue;

                Assert.That(cueSheet.pitchInvert, Is.EqualTo(testValue));
                Assert.That(observableCueSheet.PitchInvert, Is.EqualTo(cueSheet.pitchInvert));
            }
        }
    }
}
