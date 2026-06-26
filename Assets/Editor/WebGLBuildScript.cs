// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

using System.IO;
using UnityEditor;
using UnityEngine;

public static class WebGLBuildScript
{
    [MenuItem("Build/WebGL Sample")]
    public static void BuildWebGL()
    {
        var outputPath = Path.Combine(Path.GetTempPath(), "AudioConductorWebGL");
        if (Directory.Exists(outputPath))
            Directory.Delete(outputPath, true);

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Development/WebGLLoopTest/BgmLoopTestScene.unity" },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.Development
        };

        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"Build result: {report.summary.result}");
    }
}
