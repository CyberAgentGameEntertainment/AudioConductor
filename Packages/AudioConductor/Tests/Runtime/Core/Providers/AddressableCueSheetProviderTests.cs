// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

#if AUDIOCONDUCTOR_ADDRESSABLES
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AudioConductor.Core.Models;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.TestTools;

namespace AudioConductor.Core.Providers.Tests
{
    public class AddressableCueSheetProviderTests
    {
        private const string TestAddress = "TestCueSheetAddress";
        private const string RootFolder = GlobalSetUpFixture.GenFolder + "/" + nameof(AddressableCueSheetProviderTests);
        private const string ConfigName = "AddressableAssetSettings.Tests";

        private AddressableCueSheetProvider _provider = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            CreateFolderRecursively(RootFolder);

            var asset = ScriptableObject.CreateInstance<CueSheetAsset>();
            var assetPath = RootFolder + "/TestCueSheet.asset";
            AssetDatabase.CreateAsset(asset, assetPath);

            var settings = AddressableAssetSettings.Create(
                RootFolder + "/Settings",
                ConfigName,
                true, true);

            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup, false, false);
            entry.address = TestAddress;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var buildContext = new AddressablesDataBuilderInput(settings);
            foreach (var db in settings.DataBuilders)
                if (db is BuildScriptFastMode builder)
                {
                    builder.BuildData<AddressableAssetBuildResult>(buildContext);
                    PlayerPrefs.Save();
                    break;
                }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            PlayerPrefs.DeleteKey(Addressables.kAddressablesRuntimeDataPath);
            PlayerPrefs.Save();

            EditorBuildSettings.RemoveConfigObject(ConfigName);

            if (AssetDatabase.IsValidFolder(RootFolder))
                AssetDatabase.DeleteAsset(RootFolder);

            if (AssetDatabase.IsValidFolder(GlobalSetUpFixture.GenFolder))
                AssetDatabase.DeleteAsset(GlobalSetUpFixture.GenFolder);

            AssetDatabase.Refresh();
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

        private static void CreateFolderRecursively(string path)
        {
            if (!path.StartsWith("Assets/"))
                return;

            var dirs = path.Split('/');
            var combinePath = dirs[0];
            foreach (var dir in dirs.Skip(1))
            {
                if (!AssetDatabase.IsValidFolder(combinePath + '/' + dir))
                    AssetDatabase.CreateFolder(combinePath, dir);
                combinePath += '/' + dir;
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

        [UnityTest]
        public IEnumerator LoadAsync_WithInvalidKey_ReturnsNull()
        {
            LogAssert.Expect(LogType.Error, new Regex("InvalidKeyException"));

            var task = _provider.LoadAsync("invalid_key_that_does_not_exist");
            yield return WaitForTask(task);

            Assert.That(task.Result, Is.Null);
        }
    }
}
#endif
