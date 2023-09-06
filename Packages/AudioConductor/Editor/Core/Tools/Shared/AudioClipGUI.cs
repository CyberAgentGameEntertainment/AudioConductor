// --------------------------------------------------------------
// Copyright 2023 CyberAgent, Inc.
// --------------------------------------------------------------

using System;
using System.Buffers;
using UnityEditor;
using UnityEngine;

namespace AudioConductor.Editor.Core.Tools.Shared
{
    internal static class AudioClipGUI
    {
        public static readonly Color DefaultCurveColor = new(1.0f, 140.0f / 255.0f, 0.0f, 1.0f);

        public static PreviewCache DoRenderPreview(AudioClip clip, Rect wantedRect, float scaleFactor = 1)
            => DoRenderPreview(clip, wantedRect, scaleFactor, DefaultCurveColor);

        public static PreviewCache DoRenderPreview(AudioClip clip, Rect wantedRect, float scaleFactor, Color curveColor)
        {
            // References: https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Inspector/AudioClipInspector.cs#L160

            scaleFactor *= 0.95f;

            var audioImporter = AudioUtilProxy.GetImporterFromClip(clip);
            var minMaxData = AudioUtilProxy.GetMinMaxData(audioImporter);
            var numChannels = clip.channels;
            var numSamples = minMaxData == null ? 0 : minMaxData.Length / (2 * numChannels);
            var h = wantedRect.height / numChannels;

            var cache = new PreviewCache(clip, numChannels, wantedRect, numSamples);

            for (var i = 0; i < numChannels; ++i)
            {
                var channel = i;
                var sample = 0;

                var channelRect = new Rect(wantedRect.x, wantedRect.y + h * channel, wantedRect.width, h);
                cache.ChannelRect[channel] = channelRect;

                void Eval(float x, out Color col, out float minValue, out float maxValue)
                {
                    col = curveColor;
                    if (numSamples <= 0)
                    {
                        minValue = 0.0f;
                        maxValue = 0.0f;
                    }
                    else
                    {
                        var p = Mathf.Clamp(x * (numSamples - 2), 0.0f, numSamples - 2);
                        var floorP = (int)Mathf.Floor(p);
                        var offset1 = (floorP * numChannels + channel) * 2;
                        var offset2 = offset1 + numChannels * 2;
                        minValue = Mathf.Min(minMaxData[offset1 + 1], minMaxData[offset2 + 1]) * scaleFactor;
                        maxValue = Mathf.Max(minMaxData[offset1 + 0], minMaxData[offset2 + 0]) * scaleFactor;
                        if (minValue > maxValue)
                            (minValue, maxValue) = (maxValue, minValue);
                    }

                    cache.Data[channel][sample] = new Vector2(minValue, maxValue);
                    ++sample;
                }

                AudioCurveRendering.DrawMinMaxFilledCurve(channelRect, Eval);
            }

            return cache;
        }

        public static void DoRenderPreview(PreviewCache cache)
            => DoRenderPreview(cache, DefaultCurveColor);

        public static void DoRenderPreview(PreviewCache cache, Color curveColor)
        {
            var numChannels = cache.Clip.channels;

            for (var i = 0; i < numChannels; ++i)
            {
                var channel = i;
                var sample = 0;

                void Eval(float x, out Color col, out float minValue, out float maxValue)
                {
                    col = curveColor;
                    minValue = maxValue = 0.0f;

                    if (x <= 0f)
                        return;

                    minValue = cache.Data[channel][sample].x;
                    maxValue = cache.Data[channel][sample].y;
                    ++sample;
                }

                AudioCurveRendering.DrawMinMaxFilledCurve(cache.ChannelRect[channel], Eval);
            }
        }

        internal sealed class PreviewCache : IDisposable
        {
            private readonly int _channels;

            public PreviewCache(AudioClip clip, int channels, Rect rect, int samples)
            {
                Clip = clip;
                _channels = channels;
                Data = ArrayPool<Vector2[]>.Shared.Rent(channels);
                ChannelRect = ArrayPool<Rect>.Shared.Rent(channels);
                Rect = rect;

                var pool = ArrayPool<Vector2>.Shared;
                for (var i = 0; i < _channels; ++i)
                    Data[i] = pool.Rent(samples);
            }

            public AudioClip Clip { get; }
            public Vector2[][] Data { get; private set; }

            public Rect[] ChannelRect { get; private set; }
            public Rect Rect { get; }

            public void Dispose()
            {
                if (Data != null)
                {
                    var pool = ArrayPool<Vector2>.Shared;
                    for (var i = 0; i < _channels; ++i)
                        pool.Return(Data[i]);
                    ArrayPool<Vector2[]>.Shared.Return(Data);
                    Data = null;
                }

                if (ChannelRect != null)
                {
                    ArrayPool<Rect>.Shared.Return(ChannelRect);
                    ChannelRect = null;
                }
            }
        }
    }
}
