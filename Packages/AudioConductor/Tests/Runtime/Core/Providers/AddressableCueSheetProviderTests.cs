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
using AudioConductor.Core.Providers;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AudioConductor.Core.Tests.Providers
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
        public IEnumerator LoadAsync_ValidKey_ReturnsAsset()
        {
            var task = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task);

            Assert.That(task.Result, Is.Not.Null);

            _provider.Release(task.Result);
        }

        [UnityTest]
        public IEnumerator Release_AfterLoadAsync_DoesNotThrow()
        {
            var task = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task);

            Assert.That(() => _provider.Release(task.Result), Throws.Nothing);
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

            _provider.Release(task1.Result);
            _provider.Release(task2.Result);

            Assert.Pass();
        }

        [Test]
        public void Release_NullAsset_DoesNotThrow()
        {
            Assert.That(() => _provider.Release(null), Throws.Nothing);
        }

        [Test]
        public void Release_UnloadedAsset_DoesNotThrow()
        {
            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();

            Assert.That(() => _provider.Release(asset), Throws.Nothing);

            Object.DestroyImmediate(asset);
        }

        [UnityTest]
        public IEnumerator Release_MoreTimesThanLoaded_DoesNotThrow()
        {
            var task = _provider.LoadAsync(TestAddress);
            yield return WaitForTask(task);

            _provider.Release(task.Result);

            Assert.That(() => _provider.Release(task.Result), Throws.Nothing);
        }
    }
}
#endif
