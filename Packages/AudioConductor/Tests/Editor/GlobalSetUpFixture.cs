// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using UnityEditor;

namespace AudioConductor.Editor
{
    [SetUpFixture]
    internal sealed class GlobalSetUpFixture
    {
        internal const string GenFolder = "Assets/gen";

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (AssetDatabase.IsValidFolder(GenFolder))
                AssetDatabase.DeleteAsset(GenFolder);
        }
    }
}
