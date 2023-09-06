// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System.Collections.Generic;
using AudioConductor.Editor.Core.Tools.CueSheetEditor.Models;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Runtime.Core.Enums;
using AudioConductor.Runtime.Core.Models;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AudioConductor.Tests.Editor.Core.Tools.CueSheetEditor.Models
{
    internal class OtherOperationPaneModelTests
    {
        private const string ExpectedTestCueSheetCsvText = @"CueSheetParameters,,,,,,,,,,,,,,,,,,,,,,,,,,
CueSheetName,ThrottleLimit,ThrottleType,Volume,Pitch,PitchInvert,,,,,,,,,,,,,,,,,,,,,
TestCueSheet,0,PriorityOrder,1,1,False,,,,,,,,,,,,,,,,,,,,,

CueListParameters,,,,,,,,,,,,,,,,,,,,,,,,,,
CueName,ColorId,CategoryId,ThrottleLimit,ThrottleType,Volume,VolumeRange,Pitch,PitchRange,PitchInvert,PlayType,TrackCount,TrackName,ColorId,ClipName,Volume,VolumeRange,Pitch,PitchRange,PitchInvert,StartSample,EndSample,LoopStartSample,IsLoop,RandomWeight,Priority,FadeTime
TestCue0,TestColorA,0,0,PriorityOrder,1,0,1,0,False,Sequential,2,TestTrackA,,,1,0,1,0,False,0,0,0,False,0,0,0
,,,,,,,,,,,,TestTrackB,,,1,0,1,0,False,0,0,0,False,0,0,0
TestCue1,,0,0,PriorityOrder,1,0,1,0,False,Sequential,3,TestTrackC,,,1,0,1,0,False,0,0,0,False,0,0,0
,,,,,,,,,,,,TestTrackD,TestColorB,,1,0,1,0,False,0,0,0,False,0,0,0
,,,,,,,,,,,,TestTrackE,,,1,0,1,0,False,0,0,0,False,0,0,0
TestCue2,,0,0,PriorityOrder,1,0,1,0,False,Sequential,1,TestTrackF,,,1,0,1,0,False,0,0,0,False,0,0,0
";

        private const string ImportTestCsvTest = @"CueSheetParameters,,,,,,,,,,,,,,,,,,,,,,,,,,
CueSheetName,ThrottleLimit,ThrottleType,Volume,Pitch,PitchInvert,,,,,,,,,,,,,,,,,,,,,
ImportTest CueSheet,3,FirstComeFirstServed,1,1,FALSE,,,,,,,,,,,,,,,,,,,,,
,,,,,,,,,,,,,,,,,,,,,,,,,,
CueListParameters,,,,,,,,,,,,,,,,,,,,,,,,,,
CueName,ColorId,CategoryId,ThrottleLimit,ThrottleType,Volume,VolumeRange,Pitch,PitchRange,PitchInvert,PlayType,TrackCount,TrackName,ColorId,ClipName,Volume,VolumeRange,Pitch,PitchRange,PitchInvert,StartSample,EndSample,LoopStartSample,IsLoop,RandomWeight,Priority,FadeTime
HomeBGM,Color1,1,0,PriorityOrder,0.9,0.2,1,0.1,FALSE,Sequential,2,BGM1,Color1,testClip1,1,0.1,1,0,FALSE,0,512768,0,TRUE,0,9,1
,,,,,,,,,,,,BGM2,Color2,testClip3,0.8,0,1,0,TRUE,2,18151,0,TRUE,1,8,1
CarSE,,2,0,FirstComeFirstServed,0.85,0.3,1,0,FALSE,Random,1,SE1,,testClip2,1,0,0.9,0.1,FALSE,0,68864,2,FALSE,2,7,2
CatVoice,,3,1,PriorityOrder,1,0,1.065574,0,TRUE,Sequential,2,Voice1,,,0.6,0,1,0,FALSE,0,0,0,FALSE,4,5,0
,,,,,,,,,,,,Voice2,,testClip5,1,0,1,0,FALSE,0,0,0,FALSE,5,0,0
";

        private static CueSheet CreateTestCueSheet() => new()
        {
            name = "TestCueSheet",
            cueList = new List<Cue>
            {
                new()
                {
                    name = "TestCue0",
                    colorId = "TestColorA",
                    trackList = new List<Track>
                    {
                        new()
                        {
                            name = "TestTrackA"
                        },
                        new()
                        {
                            name = "TestTrackB"
                        }
                    }
                },
                new()
                {
                    name = "TestCue1",
                    trackList = new List<Track>
                    {
                        new()
                        {
                            name = "TestTrackC"
                        },
                        new()
                        {
                            name = "TestTrackD",
                            colorId = "TestColorB"
                        },
                        new()
                        {
                            name = "TestTrackE"
                        }
                    }
                },
                new()
                {
                    name = "TestCue2",
                    trackList = new List<Track>
                    {
                        new()
                        {
                            name = "TestTrackF"
                        }
                    }
                }
            }
        };

        [Test]
        public void Properties()
        {
            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new OtherOperationPaneModel(cueSheet, history, new AssetSaveService());

            Assert.That(cueSheet.name, Is.EqualTo(model.CueSheetName));
        }

        [Test]
        public void CreateCsvText()
        {
            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var model = new OtherOperationPaneModel(cueSheet, history, new AssetSaveService());

            var csvText = model.CreateCsvText();

            Assert.IsNotNull(csvText);
            Assert.IsNotEmpty(csvText);
            Assert.That(csvText, Is.EqualTo(ExpectedTestCueSheetCsvText));
        }

        [Test]
        public void ImportCsv()
        {
            var history = new AutoIncrementHistory();
            var cueSheet = CreateTestCueSheet();
            var initialCueSheet = CreateTestCueSheet();
            var model = new OtherOperationPaneModel(cueSheet, history, new AssetSaveService());

            LogAssert.Expect(LogType.Warning, "AudioClip not found : testClip5");

            var success = model.ImportCsv(ImportTestCsvTest.Split('\n'));

            Assert.True(success);
            Assert.That(cueSheet.name, Is.EqualTo("ImportTest CueSheet"));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(3));
            Assert.That(cueSheet.throttleType, Is.EqualTo(ThrottleType.FirstComeFirstServed));
            Assert.That(cueSheet.volume, Is.EqualTo(1));
            Assert.That(cueSheet.pitch, Is.EqualTo(1));
            Assert.That(cueSheet.pitchInvert, Is.False);
            Assert.That(cueSheet.cueList.Count, Is.EqualTo(3));
            Assert.That(cueSheet.cueList[0].name, Is.EqualTo("HomeBGM"));
            Assert.That(cueSheet.cueList[0].colorId, Is.EqualTo("Color1"));
            Assert.That(cueSheet.cueList[0].categoryId, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].throttleLimit, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].throttleType, Is.EqualTo(ThrottleType.PriorityOrder));
            Assert.That(cueSheet.cueList[0].volume, Is.EqualTo(0.9f));
            Assert.That(cueSheet.cueList[0].volumeRange, Is.EqualTo(0.2f));
            Assert.That(cueSheet.cueList[0].pitch, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].pitchRange, Is.EqualTo(0.1f));
            Assert.That(cueSheet.cueList[0].pitchInvert, Is.False);
            Assert.That(cueSheet.cueList[0].playType, Is.EqualTo(CuePlayType.Sequential));
            Assert.That(cueSheet.cueList[0].trackList.Count, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[0].trackList[0].name, Is.EqualTo("BGM1"));
            Assert.That(cueSheet.cueList[0].trackList[0].colorId, Is.EqualTo("Color1"));
            Assert.That(cueSheet.cueList[0].trackList[0].audioClip.name, Is.EqualTo("testClip1"));
            Assert.That(cueSheet.cueList[0].trackList[0].volume, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].trackList[0].volumeRange, Is.EqualTo(0.1f));
            Assert.That(cueSheet.cueList[0].trackList[0].pitch, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].trackList[0].pitchRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[0].pitchInvert, Is.False);
            Assert.That(cueSheet.cueList[0].trackList[0].startSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[0].endSample, Is.EqualTo(512768));
            Assert.That(cueSheet.cueList[0].trackList[0].loopStartSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[0].isLoop, Is.True);
            Assert.That(cueSheet.cueList[0].trackList[0].randomWeight, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[0].priority, Is.EqualTo(9));
            Assert.That(cueSheet.cueList[0].trackList[0].fadeTime, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].trackList[1].name, Is.EqualTo("BGM2"));
            Assert.That(cueSheet.cueList[0].trackList[1].audioClip.name, Is.EqualTo("testClip3"));
            Assert.That(cueSheet.cueList[0].trackList[1].colorId, Is.EqualTo("Color2"));
            Assert.That(cueSheet.cueList[0].trackList[1].volume, Is.EqualTo(0.8f));
            Assert.That(cueSheet.cueList[0].trackList[1].volumeRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[1].pitch, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].trackList[1].pitchRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[1].pitchInvert, Is.True);
            Assert.That(cueSheet.cueList[0].trackList[1].startSample, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[0].trackList[1].endSample, Is.EqualTo(18151));
            Assert.That(cueSheet.cueList[0].trackList[1].loopStartSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[0].trackList[1].isLoop, Is.True);
            Assert.That(cueSheet.cueList[0].trackList[1].randomWeight, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[0].trackList[1].priority, Is.EqualTo(8));
            Assert.That(cueSheet.cueList[0].trackList[1].fadeTime, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].name, Is.EqualTo("CarSE"));
            Assert.That(cueSheet.cueList[1].categoryId, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[1].throttleLimit, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[1].throttleType, Is.EqualTo(ThrottleType.FirstComeFirstServed));
            Assert.That(cueSheet.cueList[1].volume, Is.EqualTo(0.85f));
            Assert.That(cueSheet.cueList[1].volumeRange, Is.EqualTo(0.3f));
            Assert.That(cueSheet.cueList[1].pitch, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].pitchRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[1].pitchInvert, Is.False);
            Assert.That(cueSheet.cueList[1].playType, Is.EqualTo(CuePlayType.Random));
            Assert.That(cueSheet.cueList[1].trackList.Count, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].trackList[0].name, Is.EqualTo("SE1"));
            Assert.That(cueSheet.cueList[1].trackList[0].audioClip.name, Is.EqualTo("testClip2"));
            Assert.That(cueSheet.cueList[1].trackList[0].volume, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[1].trackList[0].volumeRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[1].trackList[0].pitch, Is.EqualTo(0.9f));
            Assert.That(cueSheet.cueList[1].trackList[0].pitchRange, Is.EqualTo(0.1f));
            Assert.That(cueSheet.cueList[1].trackList[0].pitchInvert, Is.False);
            Assert.That(cueSheet.cueList[1].trackList[0].startSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[1].trackList[0].endSample, Is.EqualTo(68864));
            Assert.That(cueSheet.cueList[1].trackList[0].loopStartSample, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[1].trackList[0].isLoop, Is.False);
            Assert.That(cueSheet.cueList[1].trackList[0].randomWeight, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[1].trackList[0].priority, Is.EqualTo(7));
            Assert.That(cueSheet.cueList[1].trackList[0].fadeTime, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[2].name, Is.EqualTo("CatVoice"));
            Assert.That(cueSheet.cueList[2].categoryId, Is.EqualTo(3));
            Assert.That(cueSheet.cueList[2].throttleLimit, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[2].throttleType, Is.EqualTo(ThrottleType.PriorityOrder));
            Assert.That(cueSheet.cueList[2].volume, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[2].volumeRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].pitch, Is.EqualTo(1.065574f));
            Assert.That(cueSheet.cueList[2].pitchRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].pitchInvert, Is.True);
            Assert.That(cueSheet.cueList[2].playType, Is.EqualTo(CuePlayType.Sequential));
            Assert.That(cueSheet.cueList[2].trackList.Count, Is.EqualTo(2));
            Assert.That(cueSheet.cueList[2].trackList[0].name, Is.EqualTo("Voice1"));
            Assert.That(cueSheet.cueList[2].trackList[0].audioClip, Is.Null);
            Assert.That(cueSheet.cueList[2].trackList[0].volume, Is.EqualTo(0.6f));
            Assert.That(cueSheet.cueList[2].trackList[0].volumeRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[0].pitch, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[2].trackList[0].pitchRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[0].pitchInvert, Is.False);
            Assert.That(cueSheet.cueList[2].trackList[0].startSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[0].endSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[0].loopStartSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[0].isLoop, Is.False);
            Assert.That(cueSheet.cueList[2].trackList[0].randomWeight, Is.EqualTo(4));
            Assert.That(cueSheet.cueList[2].trackList[0].priority, Is.EqualTo(5));
            Assert.That(cueSheet.cueList[2].trackList[0].fadeTime, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].name, Is.EqualTo("Voice2"));
            Assert.That(cueSheet.cueList[2].trackList[1].audioClip, Is.Null);
            Assert.That(cueSheet.cueList[2].trackList[1].volume, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[2].trackList[1].volumeRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].pitch, Is.EqualTo(1));
            Assert.That(cueSheet.cueList[2].trackList[1].pitchRange, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].pitchInvert, Is.False);
            Assert.That(cueSheet.cueList[2].trackList[1].startSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].endSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].loopStartSample, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].isLoop, Is.False);
            Assert.That(cueSheet.cueList[2].trackList[1].randomWeight, Is.EqualTo(5));
            Assert.That(cueSheet.cueList[2].trackList[1].priority, Is.EqualTo(0));
            Assert.That(cueSheet.cueList[2].trackList[1].fadeTime, Is.EqualTo(0));

            history.Undo();

            Assert.That(cueSheet.name, Is.EqualTo(initialCueSheet.name));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(initialCueSheet.throttleLimit));
            Assert.That(cueSheet.throttleType, Is.EqualTo(initialCueSheet.throttleType));
            Assert.That(cueSheet.volume, Is.EqualTo(initialCueSheet.volume));
            Assert.That(cueSheet.pitch, Is.EqualTo(initialCueSheet.pitch));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(initialCueSheet.pitchInvert));
            Assert.That(cueSheet.cueList.Count, Is.EqualTo(initialCueSheet.cueList.Count));
            for (var i = 0; i < cueSheet.cueList.Count; i++)
            {
                var cue = cueSheet.cueList[i];
                var expectedCue = initialCueSheet.cueList[i];
                Assert.That(cue.name, Is.EqualTo(expectedCue.name));
                Assert.That(cue.categoryId, Is.EqualTo(expectedCue.categoryId));
                Assert.That(cue.throttleLimit, Is.EqualTo(expectedCue.throttleLimit));
                Assert.That(cue.throttleType, Is.EqualTo(expectedCue.throttleType));
                Assert.That(cue.volume, Is.EqualTo(expectedCue.volume));
                Assert.That(cue.volumeRange, Is.EqualTo(expectedCue.volumeRange));
                Assert.That(cue.pitch, Is.EqualTo(expectedCue.pitch));
                Assert.That(cue.pitchRange, Is.EqualTo(expectedCue.pitchRange));
                Assert.That(cue.pitchInvert, Is.EqualTo(expectedCue.pitchInvert));
                Assert.That(cue.playType, Is.EqualTo(expectedCue.playType));
                Assert.That(cue.trackList.Count, Is.EqualTo(expectedCue.trackList.Count));
                for (var j = 0; j < cue.trackList.Count; j++)
                {
                    var track = cue.trackList[j];
                    var expectedTrack = expectedCue.trackList[j];
                    Assert.That(track.name, Is.EqualTo(expectedTrack.name));
                    Assert.That(track.audioClip, Is.EqualTo(expectedTrack.audioClip));
                    Assert.That(track.volume, Is.EqualTo(expectedTrack.volume));
                    Assert.That(track.volumeRange, Is.EqualTo(expectedTrack.volumeRange));
                    Assert.That(track.pitch, Is.EqualTo(expectedTrack.pitch));
                    Assert.That(track.pitchRange, Is.EqualTo(expectedTrack.pitchRange));
                    Assert.That(track.pitchInvert, Is.EqualTo(expectedTrack.pitchInvert));
                    Assert.That(track.startSample, Is.EqualTo(expectedTrack.startSample));
                    Assert.That(track.endSample, Is.EqualTo(expectedTrack.endSample));
                    Assert.That(track.loopStartSample, Is.EqualTo(expectedTrack.loopStartSample));
                    Assert.That(track.isLoop, Is.EqualTo(expectedTrack.isLoop));
                    Assert.That(track.randomWeight, Is.EqualTo(expectedTrack.randomWeight));
                    Assert.That(track.priority, Is.EqualTo(expectedTrack.priority));
                    Assert.That(track.fadeTime, Is.EqualTo(expectedTrack.fadeTime));
                }
            }
        }
    }
}
