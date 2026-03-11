// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using UnityEditor;

namespace AudioConductor.Editor.Core.Tests
{
    [SetUpFixture]
    internal sealed class GlobalSetUpFixture
    {
        private const string GenFolder = "Assets/gen";

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (AssetDatabase.IsValidFolder(GenFolder))
                AssetDatabase.DeleteAsset(GenFolder);
        }
    }
}
