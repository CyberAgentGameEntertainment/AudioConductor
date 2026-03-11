// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if AUDIOCONDUCTOR_ADDRESSABLES
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.TestTools;

namespace AudioConductor.Core.Providers.Tests
{
    public class AddressableCueSheetProviderTests : IPrebuildSetup, IPostBuildCleanup
    {
        private const string TestAddress = "TestCueSheetAddress";
        private const string RootFolder = "Assets/gen/AddressableCueSheetProviderTests";

        private AddressableCueSheetProvider _provider = null!;

        void IPostBuildCleanup.Cleanup()
        {
            if (AssetDatabase.IsValidFolder("Assets/gen"))
                AssetDatabase.DeleteAsset("Assets/gen");
        }

        void IPrebuildSetup.Setup()
        {
            if (Directory.Exists(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            Directory.CreateDirectory(RootFolder);

            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var assetPath = RootFolder + "/TestCueSheet.asset";
            AssetDatabase.CreateAsset(asset, assetPath);

            var settings = AddressableAssetSettings.Create(
                RootFolder + "/Settings",
                "AddressableAssetSettings.Tests",
                true, true);

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup, false, false);
            entry.address = TestAddress;

            AssetDatabase.SaveAssets();

            var buildContext = new AddressablesDataBuilderInput(settings);
            foreach (var db in settings.DataBuilders)
                if (db is BuildScriptFastMode builder)
                {
                    builder.BuildData<AddressableAssetBuildResult>(buildContext);
                    break;
                }
        }

        [SetUp]
        public void SetUp()
        {
            _provider = new AddressableCueSheetProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
        }

        private static IEnumerator WaitForTask(Task task)
        {
            while (!task.IsCompleted)
            {
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }
        }

        [Test]
        public void Load_ThrowsNotSupportedException()
        {
            Assert.That(() => _provider.Load(TestAddress), Throws.TypeOf<NotSupportedException>());
        }

        [UnityTest]
        public IEnumerator LoadAsync_ValidKey_ReturnsLoadInfo()
        {
            var task = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task);

            Assert.That(task.Result, Is.Not.Null);
            Assert.That(task.Result!.Value.Asset, Is.Not.Null);
            Assert.That(task.Result.Value.LoadId, Is.GreaterThan(0u));

            _provider.Release(task.Result.Value.LoadId);
        }

        [UnityTest]
        public IEnumerator Release_AfterLoadAsync_DoesNotThrow()
        {
            var task = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task);

            Assert.That(() => _provider.Release(task.Result!.Value.LoadId), Throws.Nothing);
        }

        [UnityTest]
        public IEnumerator LoadAsync_MultipleTimes_EachReleaseWorks()
        {
            var task1 = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task1);

            var task2 = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task2);

            Assert.That(task1.Result, Is.Not.Null);
            Assert.That(task2.Result, Is.Not.Null);

            _provider.Release(task1.Result!.Value.LoadId);
            _provider.Release(task2.Result!.Value.LoadId);

            Assert.Pass();
        }

        [Test]
        public void Release_ZeroLoadId_DoesNotThrow()
        {
            Assert.That(() => _provider.Release(0), Throws.Nothing);
        }

        [Test]
        public void Release_UnknownLoadId_DoesNotThrow()
        {
            Assert.That(() => _provider.Release(999), Throws.Nothing);
        }

        [UnityTest]
        public IEnumerator Release_MoreTimesThanLoaded_DoesNotThrow()
        {
            var task = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task);

            _provider.Release(task.Result!.Value.LoadId);

            // Second release with same loadId should be ignored
            Assert.That(() => _provider.Release(task.Result.Value.LoadId), Throws.Nothing);
        }
    }
}
#endif
