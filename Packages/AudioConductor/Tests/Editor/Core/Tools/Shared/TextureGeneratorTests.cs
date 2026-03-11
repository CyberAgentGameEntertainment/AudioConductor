// --------------------------------------------------------------
// Copyright 2026 CyberAgent, Inc.
// --------------------------------------------------------------

#nullable enable

using NUnit.Framework;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared.Tests
{
    internal class TextureGeneratorTests
    {
        [Test]
        public void CreateTexture_ReturnsTextureWithExpectedSize()
        {
            var texture = TextureGenerator.CreateTexture(16, 16, Color.red);
            try
            {
                Assert.That(texture.width, Is.EqualTo(16));
                Assert.That(texture.height, Is.EqualTo(16));
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void CreateTexture_ReturnsTextureWithExpectedColor()
        {
            var color = new Color32(42, 98, 216, 255);
            var texture = TextureGenerator.CreateTexture(2, 2, color);
            try
            {
                var pixels = texture.GetPixels32();
                foreach (var pixel in pixels)
                {
                    Assert.That(pixel.r, Is.EqualTo(color.r));
                    Assert.That(pixel.g, Is.EqualTo(color.g));
                    Assert.That(pixel.b, Is.EqualTo(color.b));
                    Assert.That(pixel.a, Is.EqualTo(color.a));
                }
            }
            finally
            {
                Object.DestroyImmediate(texture);
            }
        }
    }
}
