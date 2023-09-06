// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class AudioUtilProxy
    {
        private static Type _audioUtil;
        private static Dictionary<string, MethodInfo> _cache;

        private static Type AudioUtil => _audioUtil ??= typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        private static Dictionary<string, MethodInfo> Cache => _cache ??= new Dictionary<string, MethodInfo>();

        private static object InvokeMethod(string name, params object[] args)
        {
            Cache.TryGetValue(name, out var method);
            if (method == null)
            {
                var types = args.Select(arg => arg.GetType()).ToArray();
                method = AudioUtil.GetMethod(name, types);
                Cache[name] = method;
            }

            Assert.IsNotNull(method);
            return method.Invoke(null, args);
        }

        public static AudioImporter GetImporterFromClip(AudioClip clip)
            => InvokeMethod("GetImporterFromClip", clip) as AudioImporter;

        public static float[] GetMinMaxData(AudioImporter importer)
            => InvokeMethod("GetMinMaxData", importer) as float[];
    }
}
