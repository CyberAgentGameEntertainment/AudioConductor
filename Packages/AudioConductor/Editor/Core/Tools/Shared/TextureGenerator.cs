// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class TextureGenerator
    {
        public static Texture2D CreateTexture(int width, int height, Color color)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            var pixelData = texture.GetPixelData<Color32>(0);
            for (var i = 0; i < pixelData.Length; ++i)
                pixelData[i] = color;
            texture.Apply();

            return texture;
        }
    }
}
