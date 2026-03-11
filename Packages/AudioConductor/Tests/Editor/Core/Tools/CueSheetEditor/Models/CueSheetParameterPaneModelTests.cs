// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using AudioConductor.Core.Enums;
using AudioConductor.Core.Models;
using AudioConductor.Core.Shared;
using AudioConductor.Editor.Core.Models;
using AudioConductor.Editor.Core.Tests;
using AudioConductor.Editor.Core.Tools.Shared;
using AudioConductor.Editor.Foundation.CommandBasedUndo;
using AudioConductor.Editor.Foundation.TinyRx;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AudioConductor.Editor.Core.Tools.CueSheetEditor.Models.Tests
{
    internal sealed class CueSheetParameterPaneModelTests
    {
        private const string RootFolder = "Assets/gen/" + nameof(CueSheetParameterPaneModelTests);
        private CueSheetAsset _asset = null!;
        private AudioConductorEditorSettings _settings = null!;

        [SetUp]
        public void SetUp()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);
            Utility.CreateFolderRecursively(RootFolder);

            _settings = ScriptableObject.CreateInstance<AudioConductorEditorSettings>();
            _settings.defaultCodeGenOutputPath = "Assets/ProjectGenerated";
            _settings.defaultCodeGenNamespace = "Project.Generated";
            _settings.defaultCodeGenClassSuffix = "ProjectIds";
            AssetDatabase.CreateAsset(_settings, RootFolder + "/AudioConductorEditorSettings.asset");
            AssetDatabase.Refresh();

            _asset = ScriptableObject.CreateInstance<CueSheetAsset>();
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            Object.DestroyImmediate(_asset);
            if (_settings != null)
                Object.DestroyImmediate(_settings, true);
        }

        [Test]
        public void NameHistory()
        {
            var defaultValue = Utility.RandomString;
            var sameValue = Utility.RandomString;
            var lastValue = Utility.RandomString;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                name = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.Name = testValue;

            Assert.That(model.Name, Is.EqualTo(lastValue));
            Assert.That(cueSheet.name, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            Assert.That(cueSheet.name, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Name, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.name, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(sameValue));
            Assert.That(cueSheet.name, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Name, Is.EqualTo(lastValue));
            Assert.That(cueSheet.name, Is.EqualTo(lastValue));
        }

        [Test]
        public void ThrottleTypeHistory()
        {
            const ThrottleType defaultValue = ThrottleType.PriorityOrder;
            const ThrottleType sameValue = ThrottleType.FirstComeFirstServed;
            const ThrottleType lastValue = ThrottleType.PriorityOrder;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                throttleType = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.ThrottleType = testValue;

            Assert.That(model.ThrottleType, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.ThrottleType, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.ThrottleType, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.ThrottleType, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.ThrottleType, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleType, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeThrottleLimit_LessThanMin()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Min - 1, ValueRangeConst.ThrottleLimit.Min);
        }

        [Test]
        public void ChangeThrottleLimit_EqualMin()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Min);
        }

        [Test]
        public void ChangeThrottleLimit_GreaterThanMax()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Max + 1, ValueRangeConst.ThrottleLimit.Max);
        }

        [Test]
        public void ChangeThrottleLimit_EqualThanMax()
        {
            ChangeThrottleLimit(ValueRangeConst.ThrottleLimit.Max, ValueRangeConst.ThrottleLimit.Max);
        }

        [Test]
        public void ChangeThrottleLimit_InRange(
            [Random(ValueRangeConst.ThrottleLimit.Min, ValueRangeConst.ThrottleLimit.Max, 3)]
            int testValue)
        {
            ChangeThrottleLimit(testValue, testValue);
        }

        private void ChangeThrottleLimit(int testValue, int expected)
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            using (model.ThrottleLimitObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(expected)); }))
            {
                model.ThrottleLimit = testValue;
                Assert.That(model.ThrottleLimit, Is.EqualTo(expected));
                Assert.That(cueSheet.throttleLimit, Is.EqualTo(expected));
            }
        }

        [Test]
        public void ThrottleLimitHistory()
        {
            const int defaultValue = 1;
            var sameValue = Random.Range(100, 199);
            var lastValue = Random.Range(300, 399);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                throttleLimit = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.ThrottleLimit = testValue;

            Assert.That(model.ThrottleLimit, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(sameValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.ThrottleLimit, Is.EqualTo(lastValue));
            Assert.That(cueSheet.throttleLimit, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangeVolume_LessThanMin()
        {
            ChangeVolume(ValueRangeConst.Volume.Min - 1, ValueRangeConst.Volume.Min);
        }

        [Test]
        public void ChangeVolume_EqualMin()
        {
            ChangeVolume(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Min);
        }

        [Test]
        public void ChangeVolume_GreaterThanMax()
        {
            ChangeVolume(ValueRangeConst.Volume.Max + 1, ValueRangeConst.Volume.Max);
        }

        [Test]
        public void ChangeVolume_EqualMax()
        {
            ChangeVolume(ValueRangeConst.Volume.Max, ValueRangeConst.Volume.Max);
        }

        [Test]
        public void ChangeVolume_InRange(
            [Random(ValueRangeConst.Volume.Min, ValueRangeConst.Volume.Max, 3)]
            float testValue)
        {
            ChangeVolume(testValue, testValue);
        }

        private void ChangeVolume(float testValue, float expected)
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            using (model.VolumeObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(expected)); }))
            {
                model.Volume = testValue;
                Assert.That(model.Volume, Is.EqualTo(expected));
                Assert.That(cueSheet.volume, Is.EqualTo(expected));
            }
        }

        [Test]
        public void VolumeHistory()
        {
            const float defaultValue = 1;
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                volume = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.Volume = testValue;

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            Assert.That(cueSheet.volume, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            Assert.That(cueSheet.volume, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Volume, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.volume, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(sameValue));
            Assert.That(cueSheet.volume, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Volume, Is.EqualTo(lastValue));
            Assert.That(cueSheet.volume, Is.EqualTo(lastValue));
        }

        [Test]
        public void ChangePitch_LessThanMin()
        {
            ChangePitch(ValueRangeConst.Pitch.Min - 1, ValueRangeConst.Pitch.Min);
        }

        [Test]
        public void ChangePitch_EqualMin()
        {
            ChangePitch(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Min);
        }

        [Test]
        public void ChangePitch_GreaterThanMax()
        {
            ChangePitch(ValueRangeConst.Pitch.Max + 1, ValueRangeConst.Pitch.Max);
        }

        [Test]
        public void ChangePitch_EqualThanMax()
        {
            ChangePitch(ValueRangeConst.Pitch.Max, ValueRangeConst.Pitch.Max);
        }

        [Test]
        public void ChangePitch_InRange(
            [Random(ValueRangeConst.Pitch.Min, ValueRangeConst.Pitch.Max, 3)]
            float testValue)
        {
            ChangePitch(testValue, testValue);
        }

        private void ChangePitch(float testValue, float expected)
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            using (model.PitchObservable.Skip(1).Subscribe(v => { Assert.That(v, Is.EqualTo(expected)); }))
            {
                model.Pitch = testValue;
                Assert.That(model.Pitch, Is.EqualTo(expected));
                Assert.That(cueSheet.pitch, Is.EqualTo(expected));
            }
        }

        [Test]
        public void PitchHistory()
        {
            const float defaultValue = 1;
            var sameValue = Random.Range(0.20f, 0.29f);
            var lastValue = Random.Range(0.30f, 0.39f);
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                pitch = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.Pitch = testValue;

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.Pitch, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.Pitch, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitch, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_PitchInvert()
        {
            const bool defaultValue = false;
            const bool sameValue = true;
            const bool lastValue = false;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet
            {
                pitchInvert = defaultValue
            };
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.PitchInvert = testValue;

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.PitchInvert, Is.EqualTo(defaultValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(sameValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.PitchInvert, Is.EqualTo(lastValue));
            Assert.That(cueSheet.pitchInvert, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_CodeGenEnabled()
        {
            const bool defaultValue = false;
            const bool sameValue = true;
            const bool lastValue = false;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.CodeGenEnabled = testValue;

            Assert.That(model.CodeGenEnabled, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenEnabled, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.CodeGenEnabled, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenEnabled, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.CodeGenEnabled, Is.EqualTo(defaultValue));
            Assert.That(_asset.codeGenEnabled, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.CodeGenEnabled, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenEnabled, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.CodeGenEnabled, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenEnabled, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_CodeGenMode()
        {
            const CueSheetCodeGenMode defaultValue = CueSheetCodeGenMode.Manual;
            const CueSheetCodeGenMode sameValue = CueSheetCodeGenMode.OnSave;
            const CueSheetCodeGenMode lastValue = CueSheetCodeGenMode.Manual;
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.CodeGenMode = testValue;

            Assert.That(model.CodeGenMode, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenMode, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.CodeGenMode, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenMode, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.CodeGenMode, Is.EqualTo(defaultValue));
            Assert.That(_asset.codeGenMode, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.CodeGenMode, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenMode, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.CodeGenMode, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenMode, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_CodeGenOutputPath()
        {
            var defaultValue = "Assets/Scripts/Generated/";
            var sameValue = "Assets/Generated";
            var lastValue = "Assets/Output";
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet();
            _asset.useDefaultCodeGenOutputPath = false;
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.CodeGenOutputPath = testValue;

            Assert.That(model.CodeGenOutputPath, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenOutputPath, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.CodeGenOutputPath, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenOutputPath, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.CodeGenOutputPath, Is.EqualTo(defaultValue));
            Assert.That(_asset.codeGenOutputPath, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.CodeGenOutputPath, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenOutputPath, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.CodeGenOutputPath, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenOutputPath, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_CodeGenNamespace()
        {
            var defaultValue = string.Empty;
            var sameValue = "AudioConductor.Generated";
            var lastValue = "MyGame.Audio";
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet();
            _asset.useDefaultCodeGenNamespace = false;
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.CodeGenNamespace = testValue;

            Assert.That(model.CodeGenNamespace, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenNamespace, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.CodeGenNamespace, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenNamespace, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.CodeGenNamespace, Is.EqualTo(defaultValue));
            Assert.That(_asset.codeGenNamespace, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.CodeGenNamespace, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenNamespace, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.CodeGenNamespace, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenNamespace, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_CodeGenClassSuffix()
        {
            var defaultValue = string.Empty;
            var sameValue = "AudioIds";
            var lastValue = "Enums";
            var testValues = new[]
            {
                sameValue,
                sameValue,
                lastValue
            };

            var cueSheet = new CueSheet();
            _asset.useDefaultCodeGenClassSuffix = false;
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            foreach (var testValue in testValues)
                model.CodeGenClassSuffix = testValue;

            Assert.That(model.CodeGenClassSuffix, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenClassSuffix, Is.EqualTo(lastValue));

            history.Undo();

            Assert.That(model.CodeGenClassSuffix, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenClassSuffix, Is.EqualTo(sameValue));

            history.Undo();

            Assert.That(model.CodeGenClassSuffix, Is.EqualTo(defaultValue));
            Assert.That(_asset.codeGenClassSuffix, Is.EqualTo(defaultValue));

            history.Redo();

            Assert.That(model.CodeGenClassSuffix, Is.EqualTo(sameValue));
            Assert.That(_asset.codeGenClassSuffix, Is.EqualTo(sameValue));

            history.Redo();

            Assert.That(model.CodeGenClassSuffix, Is.EqualTo(lastValue));
            Assert.That(_asset.codeGenClassSuffix, Is.EqualTo(lastValue));
        }

        [Test]
        public void History_DifferentValue_UseDefaultCodeGenOutputPath()
        {
            var cueSheet = new CueSheet();
            _asset.codeGenOutputPath = "Assets/ExplicitOutput";
            _asset.useDefaultCodeGenOutputPath = false;
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            model.UseDefaultCodeGenOutputPath = true;

            Assert.That(model.UseDefaultCodeGenOutputPath, Is.True);
            Assert.That(model.CodeGenOutputPath, Is.EqualTo("Assets/ProjectGenerated"));
            Assert.That(_asset.useDefaultCodeGenOutputPath, Is.True);

            history.Undo();

            Assert.That(model.UseDefaultCodeGenOutputPath, Is.False);
            Assert.That(model.CodeGenOutputPath, Is.EqualTo("Assets/ExplicitOutput"));
            Assert.That(_asset.useDefaultCodeGenOutputPath, Is.False);
        }

        [Test]
        public void History_DifferentValue_UseDefaultCodeGenNamespace()
        {
            var cueSheet = new CueSheet();
            _asset.codeGenNamespace = "Explicit.Namespace";
            _asset.useDefaultCodeGenNamespace = false;
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            model.UseDefaultCodeGenNamespace = true;

            Assert.That(model.UseDefaultCodeGenNamespace, Is.True);
            Assert.That(model.CodeGenNamespace, Is.EqualTo("Project.Generated"));
            Assert.That(_asset.useDefaultCodeGenNamespace, Is.True);

            history.Undo();

            Assert.That(model.UseDefaultCodeGenNamespace, Is.False);
            Assert.That(model.CodeGenNamespace, Is.EqualTo("Explicit.Namespace"));
            Assert.That(_asset.useDefaultCodeGenNamespace, Is.False);
        }

        [Test]
        public void History_DifferentValue_UseDefaultCodeGenClassSuffix()
        {
            var cueSheet = new CueSheet();
            _asset.codeGenClassSuffix = "ExplicitSuffix";
            _asset.useDefaultCodeGenClassSuffix = false;
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            model.UseDefaultCodeGenClassSuffix = true;

            Assert.That(model.UseDefaultCodeGenClassSuffix, Is.True);
            Assert.That(model.CodeGenClassSuffix, Is.EqualTo("ProjectIds"));
            Assert.That(_asset.useDefaultCodeGenClassSuffix, Is.True);

            history.Undo();

            Assert.That(model.UseDefaultCodeGenClassSuffix, Is.False);
            Assert.That(model.CodeGenClassSuffix, Is.EqualTo("ExplicitSuffix"));
            Assert.That(_asset.useDefaultCodeGenClassSuffix, Is.False);
        }

        [Test]
        public void Constructor_DefaultUseDefaultFlags_AreTrue()
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            Assert.That(_asset.useDefaultCodeGenOutputPath, Is.True);
            Assert.That(_asset.useDefaultCodeGenNamespace, Is.True);
            Assert.That(_asset.useDefaultCodeGenClassSuffix, Is.True);
            Assert.That(model.UseDefaultCodeGenOutputPath, Is.True);
            Assert.That(model.UseDefaultCodeGenNamespace, Is.True);
            Assert.That(model.UseDefaultCodeGenClassSuffix, Is.True);
        }

        [Test]
        public void RefreshResolvedCodeGenDefaults_WhenUseDefaultEnabled_UpdatesDisplayedValues()
        {
            var cueSheet = new CueSheet();
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            _settings.defaultCodeGenOutputPath = "Assets/UpdatedGenerated";
            _settings.defaultCodeGenNamespace = "Project.Updated";
            _settings.defaultCodeGenClassSuffix = "UpdatedIds";

            model.RefreshResolvedCodeGenDefaults();

            Assert.That(model.CodeGenOutputPath, Is.EqualTo("Assets/UpdatedGenerated"));
            Assert.That(model.CodeGenNamespace, Is.EqualTo("Project.Updated"));
            Assert.That(model.CodeGenClassSuffix, Is.EqualTo("UpdatedIds"));
        }

        [Test]
        public void RefreshResolvedCodeGenDefaults_WhenUseDefaultDisabled_KeepsExplicitValues()
        {
            var cueSheet = new CueSheet();
            _asset.useDefaultCodeGenOutputPath = false;
            _asset.codeGenOutputPath = "Assets/ExplicitOutput";
            _asset.useDefaultCodeGenNamespace = false;
            _asset.codeGenNamespace = "Explicit.Namespace";
            _asset.useDefaultCodeGenClassSuffix = false;
            _asset.codeGenClassSuffix = "ExplicitIds";
            var history = new AutoIncrementHistory();
            var assetSaveService = new AssetSaveService();
            var model = new CueSheetParameterPaneModel(cueSheet, history, assetSaveService, _asset);

            _settings.defaultCodeGenOutputPath = "Assets/UpdatedGenerated";
            _settings.defaultCodeGenNamespace = "Project.Updated";
            _settings.defaultCodeGenClassSuffix = "UpdatedIds";

            model.RefreshResolvedCodeGenDefaults();

            Assert.That(model.CodeGenOutputPath, Is.EqualTo("Assets/ExplicitOutput"));
            Assert.That(model.CodeGenNamespace, Is.EqualTo("Explicit.Namespace"));
            Assert.That(model.CodeGenClassSuffix, Is.EqualTo("ExplicitIds"));
        }
    }
}
